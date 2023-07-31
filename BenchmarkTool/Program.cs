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

namespace BenchmarkTool
{
    static class Program
    {
        static string Mode;
        static bool _WriteComplete = false;
        static bool _ReadComplete = false;
        public static int _currentReadClientsNR;
        static int _TestRetryWriteIteration;
        static int _TestRetryReadIteration;

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
                        PopulateOneDayRegularData();
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
                        await Task.WhenAll(new Task[] { Batching(true), BenchmarkReadData() }.AsParallel());
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
            return await client.RunQuery(_TestRetryReadIteration);
        }

        private static void PopulateOneDayRegularData()
        {

            var client = new ClientWrite(1, 1, Config.GetSensorNumber(), Config.GetSensorNumber() * Config.GetDataDimensionsNr() * 86400 * (1000 / Config.GetDatalayertsScaleMilliseconds()), Config.GetStartTime());
            Task.FromResult(client.RunIngestion(1));
        }
        private async static Task Batching(bool log)
        {

            int TestRetryWriteIteration = 0;
            {
                while (!(_WriteComplete && _ReadComplete) || TestRetryWriteIteration < Config.GetTestRetries())
                {
                    TestRetryWriteIteration++;

                    _TestRetryWriteIteration = TestRetryWriteIteration;


                    var resultsLogger = new CsvLogger<LogRecordWrite>();

                    int sensorsNb = Config.GetSensorNumber();
                    int[] clientNumberArray = Config.GetClientNumberOptions();
                    int[] batchSizeArray = Config.GetBatchSizeOptions();
                    var daySpan = Config.GetDaySpan();
                    int loop = 0;


                    foreach (var ClientsNb in clientNumberArray)
                    {
                        var totalClientsNb = ClientsNb;
                        foreach (var batchSize in batchSizeArray)
                        {
                            if (TestRetryWriteIteration > Config.GetTestRetries())
                            {
                                totalClientsNb = clientNumberArray.Last() + 1;
                            }

                            var date = Config.GetStartTime().AddDays(loop * daySpan); // TODO ask if keep "new client -new day" or change to "all clients one day, overwrite "
                            var clients = new List<ClientWrite>();
                            for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                            {
                                clients.Add(new ClientWrite(chosenClientIndex, totalClientsNb, sensorsNb, batchSize, date));
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
                                        result.Client, result.Iteration);
                                    resultsLogger.WriteRecord(record);
                                }
                            }
                            glances.EndMonitor();
                            loop++;
                        }
                    }
                    resultsLogger.Dispose();
                    if (TestRetryWriteIteration == Config.GetTestRetries()) _WriteComplete = true;
                }

            }
        }

        private static async Task BenchmarkReadData()
        {
            string[] QueryArray;

            if (Config.GetQueryType() == "All")
                QueryArray = new string[] { "RangeQueryRawData", "RangeQueryRawAllDimsData", "RangeQueryAggData", "OutOfRangeQuery", "DifferenceAggQuery", "STDDevQuery" };
            else
                QueryArray = new string[] { Config.GetQueryType() };

            int[] clientNumberArray = Config.GetClientNumberOptions();


            int TestRetryReadIteration = 0;
            {
                while (!(_WriteComplete && _ReadComplete) || TestRetryReadIteration < Config.GetTestRetries())
                {
                    TestRetryReadIteration++;
                    _TestRetryReadIteration = TestRetryReadIteration;
                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentReadClientsNR = totalClientsNb;
                        foreach (string Query in QueryArray)
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

                            if (Mode.Contains("mixed"))
                            {
                                for (int p = 1; p <= _currentReadClientsNR; p++)
                                    ParallelClients.Add(new ClientRead()); //client.RunQuery(TestRetryReadIteration)
                                results = await ParallelClients.ParallelForEachAsync(RunReadTask, _currentReadClientsNR);
                            }
                            else
                                results = await client.RunQuery(TestRetryReadIteration);

                            glances.Commit();
                            using (var csvLogger = new CsvLogger<LogRecordRead>())
                            {
                                foreach (var result in results)
                                {
                                    var record = result.PerformanceMetric.ToLogRecord(Mode, result.Timestamp, -1, -1, -1, 0, result.Iteration);
                                    csvLogger.WriteRecord(record);
                                }
                            }
                            glances.EndMonitor();
                        }
                    }
                    if (TestRetryReadIteration == Config.GetTestRetries()) _ReadComplete = true;
                }

            }
        }
    }
}
