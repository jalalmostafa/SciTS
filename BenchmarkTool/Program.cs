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

        public static int _currentReadClientsNR { get; private set; }
        public static int _currentClientsNR { get; private set; }
        public static int _currentWriteClientsNR { get; private set; }
        public static int _currentWriteBatchSize { get; private set; }
        static int _TestRetryWriteIteration;
        static int _TestRetryReadIteration;

        public static int _currentlimit { get; private set; }

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
                        Mode = "populate_Day+0_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(0);
                        break;
                    case var s when action.Contains("populate+"):
                        _ReadComplete = true;
                        int i1 = s.IndexOf("+") + 1;
                        int i2 = s.Length;
                        int day = int.Parse(s.Substring(i1, i2 - i1));
                        Mode = "populate_Day+" + day + "_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(day);
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
                    case "mixed-LimitedQueries":
                        Mode = "mixed-LimitedQueries_" + Config.GetIngestionType();
                        await mixedWL(true);

                        break;
                    case "mixed-AggQueries":
                        Mode = "mixed-AggQueries_" + Config.GetIngestionType();
                        await mixedWL(true);

                        break;
                    case var s when action.Contains("mixed-") & action.Contains("%LimitedQueries"):

                        int ii1 = s.IndexOf("-") + 1;
                        int ii2 = s.IndexOf("%");
                        string sub = s.Substring(ii1, ii2 - ii1);
                        Config._actualMixedWLPercentage = int.Parse(sub);
                        Mode = "mixed-" + sub + "%LimitedQueries_" + Config.GetIngestionType();
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

            return await client.RunQuery(_TestRetryReadIteration, _currentReadClientsNR, _currentlimit);
        }

        private async static Task PopulateOneDayRegularData(int dayAfterStartdate)
        {
            var init = Config.GetQueryType(); // Just for Init the Array
            int batchSize = Config.GetSensorNumber() * Config.GetDataDimensionsNr() * 60 * (1000 / Config.GetRegularTsScaleMilliseconds()); // one minute ingestion

            int totalClientsNb = Config.GetClientNumberOptions().Last();

            foreach (var dimNb in Config.GetDataDimensionsNrOptions())
            {
                int minute = 0;
                Config._actualDataDimensionsNr = dimNb;


                while (minute < 24 * 60) // if not all day ingested, continue
                {
                    // var client = new ClientWrite(1, 1, Config.GetSensorNumber(), batchSize, dimNb, Config.GetStartTime().AddDays(dayAfterStartdate).AddMinutes(minute));
                    // Task.FromResult(client.RunIngestion(1));
                    var clientArray = new ClientWrite[totalClientsNb];
                    //  new List<ClientWrite>();
                    for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                    {
                        clientArray[chosenClientIndex - 1] = new ClientWrite(chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, Config.GetStartTime().AddDays(dayAfterStartdate).AddMinutes(minute));
                        //  clients.Add(new ClientWrite(chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, Config.GetStartTime().AddDays(dayAfterStartdate).AddMinutes(minute)));
                    }
                    minute++;

                    var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, Config.GetSensorNumber());
                    // var resultsW = await clients.ParallelForEachAsync(RunIngestionTask, totalClientsNb);
                    // ARmin COde
                    var resultsW = new QueryStatusWrite[totalClientsNb];
                    await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsW[index] = (await RunIngestionTask(clientArray[index]).ConfigureAwait(false)).Single(); });

                    await glancesW.CommitAsync().ConfigureAwait(false);

                    using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                        foreach (var result in resultsW)
                        {
                            var recordW = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                result.Timestamp, result.StartDate, batchSize, totalClientsNb, Config.GetSensorNumber(),
                                result.Client, result.Iteration, dimNb);
                            csvLoggerW.WriteRecord(recordW);
                        }

                    await glancesW.EndMonitorAsync().ConfigureAwait(false);

                    GC.Collect();





                }





            }
        }


        private async static Task mixedWL(bool log)
        {
            var init = Config.GetQueryType(); // Just for Init the Array

            int TestRetryIteration = 0;
            int[] clientNumberArray = Config.GetClientNumberOptions();

            while (TestRetryIteration < Config.GetTestRetries())
            {

                TestRetryIteration++; // maybe obsolete
                _TestRetryReadIteration = TestRetryIteration;
                _TestRetryWriteIteration = TestRetryIteration;

                // var resultsLoggerW = new CsvLogger<LogRecordWrite>("write");
                // var resultsLoggerR = new CsvLogger<LogRecordRead>("read");

                int sensorsNb = Config.GetSensorNumber();
                int[] percentageArray = Config.GetMixedWLPercentageOptions();
                clientNumberArray = Config.GetClientNumberOptions();
                int[] batchSizeArray = Config.GetBatchSizeOptions();
                int[] dimNbArray = Config.GetDataDimensionsNrOptions();

                var daySpan = Config.GetDaySpan();



                if (Mode.Contains("dedicated_") | Mode.Contains("mixed-AggQueries_"))
                    percentageArray = new int[1] { 0 };
                if (Mode.Contains("%"))
                    percentageArray = new int[1] { Config._actualMixedWLPercentage };
                if (Mode.Contains("Limited"))
                    Config._QueryArray = new string[] { "RangeQueryRawAllDimsLimitedData" };


                foreach (var percentage in percentageArray)
                {

                    Config._actualMixedWLPercentage = percentage;

                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentClientsNR = totalClientsNb;
                        _currentReadClientsNR = totalClientsNb;
                        _currentWriteClientsNR = totalClientsNb;


                        foreach (var dimNb in dimNbArray)
                        {
                            Config._actualDataDimensionsNr = dimNb;
                            int loop = 0;
                            foreach (var batchSize in batchSizeArray)
                            {
                                _currentWriteBatchSize = batchSize;
                                _currentlimit = (int)((double)_currentWriteBatchSize * ((double)Config._actualMixedWLPercentage / 100));



                                var date = Config.GetStartTime().AddDays(loop * daySpan); // TODO ask if keep "new batch -new day" + "new client new hour" or change to "all clients one day, overwrite "
                                var clients = new List<ClientWrite>();
                                for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                {
                                    clients.Add(new ClientWrite(chosenClientIndex, totalClientsNb, sensorsNb, batchSize, dimNb, date.AddHours(chosenClientIndex)));
                                }
                                var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, sensorsNb);
                                var resultsW = await clients.ParallelForEachAsync(RunIngestionTask, totalClientsNb);


                                await glancesW.CommitAsync().ConfigureAwait(false);

                                using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                                    foreach (var result in resultsW)
                                    {
                                        var recordW = result.PerformanceMetric.ToLogRecord(Mode, percentage,
                                            result.Timestamp, result.StartDate, batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        csvLoggerW.WriteRecord(recordW);
                                    }

                                await glancesW.EndMonitorAsync().ConfigureAwait(false);

                                loop++;


                                foreach (string Query in Config._QueryArray)
                                {


                                    Config.QueryTypeOnRunTime = Query;

                                    var client = new ClientRead();
                                    var glancesR = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentClientsNR, batchSize, sensorsNb);
                                    var resultsR = new List<QueryStatusRead>();
                                    var ParallelClients = new List<ClientRead>();


                                    for (int p = 1; p <= totalClientsNb; p++)
                                        ParallelClients.Add(new ClientRead()); //client.RunQuery(TestRetryReadIteration)
                                    resultsR = await ParallelClients.ParallelForEachAsync(RunReadTask, totalClientsNb);



                                    await glancesR.CommitAsync().ConfigureAwait(false);
                                    // using (var csvLogger = new CsvLogger<LogRecordRead>("read")) //relict, dont know why "using" has been used
                                    using (var csvLoggerR = new CsvLogger<LogRecordRead>("read"))
                                    {
                                        foreach (var result in resultsR)
                                        {
                                            var recordR = result.PerformanceMetric.ToLogRecord(Mode, percentage, result.Timestamp, result.StartDate, batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                            csvLoggerR.WriteRecord(recordR);
                                        }
                                    }
                                    await glancesR.EndMonitorAsync().ConfigureAwait(false);
                                }

                            }
                        }
                        GC.Collect();
                    }
                    // resultsLoggerW.Dispose();
                    // resultsLoggerR.Dispose();




                }




            }
            Console.Out.WriteLine("MixedWL-Completed");

        }



        private async static Task Batching(bool log)
        {
            var init = Config.GetQueryType(); // Just for Init the Array

            int TestRetryWriteIteration = 0;
            {
                while (!((_WriteComplete && _ReadComplete)) | TestRetryWriteIteration < Config.GetTestRetries())
                {
                    TestRetryWriteIteration++;

                    _TestRetryWriteIteration = TestRetryWriteIteration;


                    var resultsLogger = new CsvLogger<LogRecordWrite>("write");

                    int sensorsNb = Config.GetSensorNumber();
                    int[] clientNumberArray = Config.GetClientNumberOptions();
                    int[] batchSizeArray = Config.GetBatchSizeOptions();
                    var daySpan = Config.GetDaySpan();
                    int[] dimNbArray = Config.GetDataDimensionsNrOptions();


                    foreach (var ClientsNb in clientNumberArray)
                    {
                        var totalClientsNb = ClientsNb;
                        _currentWriteClientsNR = totalClientsNb;


                        foreach (var dimNb in dimNbArray)
                        {
                            Config._actualDataDimensionsNr = dimNb;
                            int loop = 0;

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
                                var results = await clients.ParallelForEachAsync(RunIngestionTask, totalClientsNb);
                                if (log)
                                {
                                    await glances.CommitAsync().ConfigureAwait(false);

                                    foreach (var result in results)
                                    {
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                            result.Timestamp, result.StartDate, batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        resultsLogger.WriteRecord(record);
                                    }
                                }
                                await glances.EndMonitorAsync().ConfigureAwait(false);
                                loop++;
                            }
                        }
                        GC.Collect();
                    }
                    resultsLogger.Dispose(); //? warum hier aber nicht in Read? funktion?
                    if (TestRetryWriteIteration == Config.GetTestRetries()) _WriteComplete = true;

                }


            }
        }

        private static async Task BenchmarkReadData()
        {
            var init = Config.GetQueryType(); // Just for Init the Array



            int[] clientNumberArray = Config.GetClientNumberOptions();
            int[] dimNbArray = Config.GetDataDimensionsNrOptions();
            // var resultsLogger = new CsvLogger<LogRecordRead>("read"); -> better using


            int TestRetryReadIteration = 0;
            {
                while (!((_WriteComplete && _ReadComplete)) | TestRetryReadIteration < Config.GetTestRetries())
                {
                    TestRetryReadIteration++;
                    _TestRetryReadIteration = TestRetryReadIteration;
                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentReadClientsNR = totalClientsNb;

                        foreach (var dimNb in dimNbArray)
                        {

                            Config._actualDataDimensionsNr = dimNb;
                            foreach (string Query in Config._QueryArray)
                            {
                                if (TestRetryReadIteration > Config.GetTestRetries())
                                {
                                    _currentReadClientsNR = clientNumberArray.Last() + 1;
                                }
                                _currentlimit = (int)((double)Config.GetBatchSizeOptions().Last());


                                Config.QueryTypeOnRunTime = Query;

                                var client = new ClientRead();
                                var sensorsNb = Config.GetSensorNumber();
                                var glances = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentReadClientsNR, _currentlimit, sensorsNb);
                                var results = new List<QueryStatusRead>();
                                var ParallelClients = new List<ClientRead>();


                                for (int p = 1; p <= _currentReadClientsNR; p++)
                                    ParallelClients.Add(new ClientRead()); //client.RunQuery(TestRetryReadIteration)
                                results = await ParallelClients.ParallelForEachAsync(RunReadTask, _currentReadClientsNR);


                                await glances.CommitAsync().ConfigureAwait(false);
                                using (var csvLogger = new CsvLogger<LogRecordRead>("read"))
                                {
                                    foreach (var result in results)
                                    {
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, -1, result.Timestamp, result.StartDate, _currentlimit, totalClientsNb, sensorsNb,
                                          result.Client, result.Iteration, dimNb);
                                        csvLogger.WriteRecord(record);
                                    }
                                }
                                await glances.EndMonitorAsync().ConfigureAwait(false);
                            }
                        }
                    }
                    GC.Collect();
                    // resultsLogger.Dispose();
                    if (TestRetryReadIteration == Config.GetTestRetries()) _ReadComplete = true;
                }

            }
        }
    }
}
