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

                var action = args != null && args.Length > 0 ? args[0] : "write";
                switch (action)
                {
                    case "populate":
                        await Batching(false);
                        break;

                    case "read":
                        await BenchmarkReadData();
                        break;

                    case "write":
                        await Batching(true);
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

        private static Task<List<QueryStatusWrite>> RunQueryTask(ClientWrite client)
        {
            return client.RunQuery();
        }

        private async static Task Batching(bool log)
        {

            var resultsLogger = new CsvLogger<LogRecordWrite>();

            int sensorsNb = Config.GetSensorNumber();
            int[] clientNumberArray = Config.GetClientNumberOptions();
            int[] batchSizeArray = Config.GetBatchSizeOptions();
            var daySpan = Config.GetDaySpan();
            int loop = 0;

            foreach (var clientsNb in clientNumberArray)
            {
                foreach (var batchSize in batchSizeArray)
                {
                    var date = Config.GetStartTime().AddDays(loop * daySpan);
                    var clients = new List<ClientWrite>();
                    for (var i = 0; i < clientsNb; i++)
                    {
                        clients.Add(new ClientWrite(i, clientsNb, sensorsNb, batchSize, date));
                    }
                    var glances = new GlancesStarter(Operation.BatchIngestion, clientsNb, batchSize, sensorsNb);
                    glances.BeginMonitor();
                    var results = await clients.ParallelForEachAsync(RunQueryTask, clientsNb);
                    if (log)
                    {
                        glances.Commit();
                        foreach (var result in results)
                        {
                            var record = result.PerformanceMetric.ToLogRecord(
                                result.Timestamp, batchSize, clientsNb, sensorsNb,
                                result.Client, result.Iteration);
                            resultsLogger.WriteRecord(record);
                        }
                    }
                    glances.EndMonitor();
                    loop++;
                }
            }
            resultsLogger.Dispose();
        }

        private static async Task BenchmarkReadData()
        {
            var client = new ClientRead();
            var sensorsNb = Config.GetSensorNumber();
            var glances = new GlancesStarter(Config.GetQueryType().ToEnum<Operation>(), 1, 0, sensorsNb);
            glances.BeginMonitor();
            var results = await client.RunQuery();

            glances.Commit();
            using (var csvLogger = new CsvLogger<LogRecordRead>())
            {
                foreach (var result in results)
                {
                    var record = result.PerformanceMetric.ToLogRecord(result.Timestamp, -1, -1, -1, 0, result.Iteration);
                    csvLogger.WriteRecord(record);
                }
            }
            glances.EndMonitor();
        }
    }
}
