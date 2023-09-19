using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using Serilog;
using Serilog.Events;
using BenchmarkTool.Database;
using BenchmarkTool.System;
using BenchmarkTool;
using System.Linq;
using MemoryPack;




namespace BenchmarkTool
{
    static class Program
    {

        // most of them can be deleted TODO
        public static string Mode { get; private set; }
        static bool _WriteComplete = false;
        static bool _ReadComplete = false;
        static bool _OnePass = false;
        public static int _currentReadClientsNR { get; private set; }
        public static int _currentClientsNR { get; private set; }
        public static int _currentWriteClientsNR { get; private set; }
        public static int _currentWriteBatchSize { get; private set; }
        static int _TestRetryWriteIteration;
        static int _TestRetryReadIteration;
        static int _TestRetryIteration;
        static string[] _QueryArray;
        static int _percentageOfQueryPoints;
        private static object _currentdimNb;

        static async Task Main(string[] args)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.File("ts-bench.log", restrictedToMinimumLevel: LogEventLevel.Information)
                            .CreateLogger();

                Console.WriteLine("Starting...");
                Log.Information("Application started");

                var action = args != null && args.Length > 0 ? args[0] : "read";
                string setting; string ingType;
                if (args.Length >= 2)
                {
                    action = args[0];
                    ingType = args[1];
                    Config.SetIngestionType(ingType);
                    if (args.Length >= 3)
                    {
                        setting = args[2];
                        Config.SetTargetDatabase(setting);
                    }




                }
                switch (action)
                {
                    case "populate":
                        _ReadComplete = true;
                        Mode = "populate_1Day_" + Config.GetIngestionType();
                        PopulateOneDayRegularData(true);
                        break;

                    case "read":
                        _WriteComplete = true;
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await BenchmarkReadData();
                        break;

                    case "write":
                        _ReadComplete = true;
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching(true);
                        break;
                    case "consecutive":
                        _ReadComplete = true;
                        _WriteComplete = true;
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching(true);
                        await BenchmarkReadData();
                        break;
                    case "mixed":
                        Mode = "mixed_" + Config.GetIngestionType();
                        _percentageOfQueryPoints = 100;
                        await mixedWL(true);

                        break;
                    case var s when action.Contains("mixed-") & action.Contains("%Q"):
                        if (Config.GetQueryType().Contains("All"))
                            Config.QueryTypeOnRunTime = "RangeQueryRawAllDimsLimitedData";
                        else
                            Config.QueryTypeOnRunTime = "RangeQueryLimitedData";

                        int i1 = s.IndexOf("-") + 1;
                        int i2 = s.IndexOf("%");
                        string sub = s.Substring(i1, i2 - i1);
                        _percentageOfQueryPoints = int.Parse(sub);
                        Mode = "mixedLimited-" + _percentageOfQueryPoints.ToString() + "%Q" + "_" + Config.GetIngestionType();
                        await mixedWL(true);
                        break;


                    default:
                        Console.WriteLine($"Unknown option {action}");
                        break;
                }
                Log.Information("Completed...");
                Console.WriteLine("Completed...");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        private async static Task<List<QueryStatusWrite>> RunIngestionTask(ClientWrite client)
        {
            return await client.RunIngestion(_TestRetryWriteIteration);
        }

        private async static Task<List<QueryStatusRead>> RunReadTask(ClientRead client)
        {

            int limit = (int)((double)_currentWriteBatchSize * ((double)_percentageOfQueryPoints / 100));
            return await client.RunQuery(_TestRetryReadIteration, limit);
        }

        private static void PopulateOneDayRegularData(bool log)
        {
            int batchSize = Config.GetSensorNumber() * Config.GetDataDimensionsNr() * 60 * (1000 / Config.GetRegularTsScaleMilliseconds());
            int minute = 0;
            int totalClientsNb = Config.GetClientNumberOptions().Last();
            while (minute < 24 * 60)
            {
                var client = new ClientWrite(1, 1, Config.GetSensorNumber(), batchSize, Config.GetDataDimensionsNrOptions().Last<int>(), Config.GetStartTime().AddMinutes(minute));
                Task.FromResult(client.RunIngestion(1));

                minute++;
            }
        }

        private async static Task mixedWL(bool log)
        {

            int TestRetryIteration = 0;



            if (Config.GetQueryType() == "All")
                _QueryArray = new string[] { "RangeQueryRawData", "RangeQueryRawAllDimsData", "RangeQueryAggData", "OutOfRangeQuery", "DifferenceAggQuery", "STDDevQuery" };
            else
                _QueryArray = new string[] { Config.GetQueryType() };

            int[] clientNumberArray = Config.GetClientNumberOptions();

            {
                while (TestRetryIteration < Config.GetTestRetries())
                {

                    TestRetryIteration++; // maybe obsolete
                    _TestRetryIteration = TestRetryIteration;
                    _TestRetryReadIteration = TestRetryIteration;
                    _TestRetryWriteIteration = TestRetryIteration;

                    var resultsLogger = new CsvLogger<LogRecordWrite>("write");
                    int sensorsNb = Config.GetSensorNumber();
                    clientNumberArray = Config.GetClientNumberOptions();
                    int[] batchSizeArray = Config.GetBatchSizeOptions();
                    int[] dimNbArray = Config.GetDataDimensionsNrOptions();

                    var daySpan = Config.GetDaySpan();
                    int loop = 0;



                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentClientsNR = totalClientsNb;
                        _currentReadClientsNR = totalClientsNb;
                        _currentWriteClientsNR = totalClientsNb;


                        foreach (var dimNb in dimNbArray)
                        {
                            _currentdimNb = dimNb;
                            Config._actualDataDimensionsNr = dimNb;

                            foreach (var batchSize in batchSizeArray)
                            {
                                _currentWriteBatchSize = batchSize;



                                var date = Config.GetStartTime().AddDays(loop * daySpan); // TODO ask if keep "new client -new day" or change to "all clients one day, overwrite "
                                var clients = new List<ClientWrite>();
                                for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                {
                                    clients.Add(new ClientWrite(chosenClientIndex, totalClientsNb, sensorsNb, batchSize, dimNb, date));
                                }
                                var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, sensorsNb);
                                glancesW.BeginMonitor();
                                var resultsW = await clients.ParallelForEachAsync(RunIngestionTask, totalClientsNb);
                                if (log)
                                {
                                    glancesW.Commit();
                                    foreach (var result in resultsW)
                                    {
                                        var recordW = result.PerformanceMetric.ToLogRecord(Mode,
                                            result.Timestamp, batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        resultsLogger.WriteRecord(recordW);
                                    }
                                }
                                glancesW.EndMonitor();
                                loop++;


                                foreach (string Query in _QueryArray)
                                {


                                    Config.QueryTypeOnRunTime = Query;

                                    var client = new ClientRead();
                                    var glancesR = new GlancesStarter(Config.GetQueryType().ToEnum<Operation>(), 1, 0, sensorsNb);
                                    glancesR.BeginMonitor();
                                    var resultsR = new List<QueryStatusRead>();
                                    var ParallelClients = new List<ClientRead>();


                                    for (int p = 1; p <= totalClientsNb; p++)
                                        ParallelClients.Add(new ClientRead()); //client.RunQuery(TestRetryReadIteration)
                                    resultsR = await ParallelClients.ParallelForEachAsync(RunReadTask, totalClientsNb);



                                    glancesR.Commit();
                                    using (var csvLogger = new CsvLogger<LogRecordRead>("read"))
                                    {
                                        foreach (var result in resultsR)
                                        {
                                            var recordR = result.PerformanceMetric.ToLogRecord(Mode, result.Timestamp, -1, -1, -1, 0, result.Iteration, dimNb);
                                            csvLogger.WriteRecord(recordR);
                                        }
                                    }
                                    glancesR.EndMonitor();
                                }

                            }
                        }
                    }
                    resultsLogger.Dispose();







                }

            }

        }



        private async static Task Batching(bool log)
        {

            int TestRetryWriteIteration = 0;
            {
                while (!((_WriteComplete && _ReadComplete) || _OnePass) | TestRetryWriteIteration < Config.GetTestRetries())
                {
                    TestRetryWriteIteration++;

                    _TestRetryWriteIteration = TestRetryWriteIteration;


                    var resultsLogger = new CsvLogger<LogRecordWrite>("write");

                    int sensorsNb = Config.GetSensorNumber();
                    int[] clientNumberArray = Config.GetClientNumberOptions();
                    int[] batchSizeArray = Config.GetBatchSizeOptions();
                    var daySpan = Config.GetDaySpan();
                    int loop = 0;
                    int[] dimNbArray = Config.GetDataDimensionsNrOptions();


                    foreach (var ClientsNb in clientNumberArray)
                    {
                        var totalClientsNb = ClientsNb;
                        _currentWriteClientsNR = totalClientsNb;


                        foreach (var dimNb in dimNbArray)
                        {
                            _currentdimNb = dimNb;
                            Config._actualDataDimensionsNr = dimNb;
                            foreach (var batchSize in batchSizeArray)
                            {
                                _currentWriteBatchSize = batchSize;

                                if (TestRetryWriteIteration > Config.GetTestRetries())
                                {
                                    totalClientsNb = clientNumberArray.Last() + 1;
                                }

                                var date = Config.GetStartTime().AddDays(loop * daySpan); // TODO ask if keep "new client -new day" or change to "all clients one day, overwrite "
                                var clients = new List<ClientWrite>();
                                for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                {
                                    clients.Add(new ClientWrite(chosenClientIndex, totalClientsNb, sensorsNb, batchSize, dimNb, date));
                                }
                                var glances = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, sensorsNb);
                                glances.BeginMonitor();
                                var results = await clients.ParallelForEachAsync(RunIngestionTask, totalClientsNb);
                                if (log)
                                {
                                    glances.Commit();
                                    foreach (var result in results)
                                    {
                                        var record = result.PerformanceMetric.ToLogRecord(Mode,
                                            result.Timestamp, batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        resultsLogger.WriteRecord(record);
                                    }
                                }
                                glances.EndMonitor();
                                loop++;
                            }
                        }
                    }
                    resultsLogger.Dispose();
                    if (TestRetryWriteIteration == Config.GetTestRetries()) _WriteComplete = true;
                }

            }
        }

        private static async Task BenchmarkReadData()
        {
            string[] _QueryArray;

            if (Config.GetQueryType() == "All")
                _QueryArray = new string[] { "RangeQueryRawData", "RangeQueryRawAllDimsData", "RangeQueryAggData", "OutOfRangeQuery", "DifferenceAggQuery", "STDDevQuery" };
            else
                _QueryArray = new string[] { Config.GetQueryType() };

            int[] clientNumberArray = Config.GetClientNumberOptions();
            int[] dimNbArray = Config.GetDataDimensionsNrOptions();


            int TestRetryReadIteration = 0;
            {
                while (!((_WriteComplete && _ReadComplete) || _OnePass) | TestRetryReadIteration < Config.GetTestRetries())
                {
                    TestRetryReadIteration++;
                    _TestRetryReadIteration = TestRetryReadIteration;
                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentReadClientsNR = totalClientsNb;

                        foreach (var dimNb in dimNbArray)
                        {
                            _currentdimNb = dimNb;
                            Config._actualDataDimensionsNr = dimNb;

                            foreach (string Query in _QueryArray)
                            {
                                if (TestRetryReadIteration > Config.GetTestRetries())
                                {
                                    _currentReadClientsNR = clientNumberArray.Last() + 1;
                                }


                                Config.QueryTypeOnRunTime = Query;

                                var client = new ClientRead();
                                var sensorsNb = Config.GetSensorNumber();
                                var glances = new GlancesStarter(Config.GetQueryType().ToEnum<Operation>(), 1, 0, sensorsNb);
                                glances.BeginMonitor();
                                var results = new List<QueryStatusRead>();
                                var ParallelClients = new List<ClientRead>();


                                for (int p = 1; p <= _currentReadClientsNR; p++)
                                    ParallelClients.Add(new ClientRead()); //client.RunQuery(TestRetryReadIteration)
                                results = await ParallelClients.ParallelForEachAsync(RunReadTask, _currentReadClientsNR);


                                glances.Commit();
                                using (var csvLogger = new CsvLogger<LogRecordRead>("read"))
                                {
                                    foreach (var result in results)
                                    {
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, result.Timestamp, -1, -1, -1, 0, result.Iteration, dimNb);
                                        csvLogger.WriteRecord(record);
                                    }
                                }
                                glances.EndMonitor();
                            }
                        }
                    }
                    if (TestRetryReadIteration == Config.GetTestRetries()) _ReadComplete = true;
                }

            }
        }
    }
}
