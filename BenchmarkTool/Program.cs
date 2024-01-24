using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using BenchmarkTool.System;
using System.Linq;
using System.Drawing.Text;




namespace BenchmarkTool
{
    static class Program
    {

        
        public static string Mode { get; private set; }
        public static int _currentReadClientsNR { get; private set; }
        public static int _currentClientsNR { get; private set; }
        public static int _currentWriteClientsNR { get; private set; }
        public static int _currentWriteBatchSize { get; private set; }
        static int _TestRetryIteration;
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
                        Mode = "populate_Day+0_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(0);
                        break;
                    case var s when action.Contains("populate+"):
                        int i1 = s.IndexOf("+") + 1;
                        int i2 = s.Length;
                        int day = int.Parse(s.Substring(i1, i2 - i1));
                        Mode = "populate_Day+" + day + "_" + Config.GetIngestionType();
                        await PopulateOneDayRegularData(day);
                        break;

                    case "read":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await BenchmarkReadData();
                        break;
                    case "write":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching(true);
                        break;
                    case "consecutive":
                        Mode = "dedicated_" + Config.GetIngestionType();
                        await Batching(true);
                        await BenchmarkReadData();
                        break;
                    case "mixed-LimitedQueries":
                        Mode = "mixed-LimitedQueries_" + Config.GetIngestionType();
                        await MixedWL(true);
                        break;
                    case "mixed-AggQueries":
                        Mode = "mixed-AggQueries_" + Config.GetIngestionType();
                        await MixedWL(true);
                        break;
                    case var s when action.Contains("mixed-") & action.Contains("%LimitedQueries"):
                        int ii1 = s.IndexOf("-") + 1;
                        int ii2 = s.IndexOf("%");
                        string sub = s.Substring(ii1, ii2 - ii1);
                        Config._actualMixedWLPercentage = int.Parse(sub);
                        Mode = "mixed-" + sub + "%LimitedQueries_" + Config.GetIngestionType();
                        await MixedWL(true);
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


        private async static Task<QueryStatusWrite> RunIngestionTask(ClientWrite client)
        {
            return await client.RunIngestion(_TestRetryIteration);
        }

        private async static Task<QueryStatusRead> RunReadTask(ClientRead client)
        {
            return await client.RunQuery(_TestRetryIteration, _currentReadClientsNR, _currentlimit);
        }

        private async static Task PopulateOneDayRegularData(int dayAfterStartdate)
        {
            var init = Config.GetQueryType(); // Just for Init the Array
            int batchSize = Config.GetSensorNumber()  * 60 * (1000 / Config.GetRegularTsScaleMilliseconds()); // one minute ingestion
            int totalClientsNb = Config.GetClientNumberOptions().Last();

            foreach (var dimNb in Config.GetDataDimensionsNrOptions())
            {
                int minute = 0;
                Config._actualDataDimensionsNr = dimNb;

                while (minute < 24 * 60) // if not all day ingested, continue
                {
                    var clientArray = new ClientWrite[totalClientsNb];
                    for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                    {
                        clientArray[chosenClientIndex - 1] = new ClientWrite(chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, Config.GetStartTime().AddDays(dayAfterStartdate).AddMinutes(minute));
                    }
                    minute++;

                    var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, Config.GetSensorNumber());
                    var resultsW = new QueryStatusWrite[totalClientsNb];
                    await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsW[index] = await RunIngestionTask(clientArray[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                    await glancesW.EndMonitorAsync().ConfigureAwait(false);

                    using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                        foreach (var result in resultsW)
                        { 
                            var recordW = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                result.Timestamp, result.StartDate, batchSize, totalClientsNb, Config.GetSensorNumber(),
                                result.Client, result.Iteration, dimNb);
                            csvLoggerW.WriteRecord(recordW);
                        }
                    GC.Collect();
                }
            }
        }


        private async static Task MixedWL(bool log)
        {
            var init = Config.GetQueryType(); // Just for Init the Array

            int _TestRetryIteration = 0;
            int[] clientNumberArray = Config.GetClientNumberOptions();

            while (_TestRetryIteration < Config.GetTestRetries())
            {
                _TestRetryIteration++; 
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
                                var date = Config.GetStartTime().AddDays(loop * daySpan); 
                                var clientArrayW = new ClientWrite[_currentWriteClientsNR];
                                for (var chosenClientIndex = 1; chosenClientIndex <= _currentWriteClientsNR; chosenClientIndex++)
                                {
                                    clientArrayW[chosenClientIndex - 1] = new ClientWrite(chosenClientIndex, _currentWriteClientsNR, Config.GetSensorNumber(), batchSize, dimNb, date);
                                }
                                var glancesW = new GlancesStarter(Operation.BatchIngestion, _currentWriteClientsNR, batchSize, sensorsNb);
                                var resultsW = new QueryStatusWrite[_currentWriteClientsNR];  
                                await Parallel.ForEachAsync(Enumerable.Range(0, _currentWriteClientsNR), new ParallelOptions() { MaxDegreeOfParallelism = _currentWriteClientsNR }, async (index, token) => { resultsW[index] = await RunIngestionTask(clientArrayW[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                await glancesW.EndMonitorAsync().ConfigureAwait(false);

                                using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                                {
                                    foreach (var result in resultsW)
                                    { 
                                        var recordW = result.PerformanceMetric.ToLogRecord(Mode, percentage,
                                            result.Timestamp, result.StartDate,   batchSize, _currentWriteClientsNR, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        csvLoggerW.WriteRecord(recordW);
                                    }
                                }

                                loop++;


                                foreach (string Query in Config._QueryArray)
                                {
                                    Config.QueryTypeOnRunTime = Query;
                                    var glancesR = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentClientsNR, batchSize, sensorsNb);
                                    var clientArrayR = new ClientRead[_currentReadClientsNR];

                                    for (int chosenClientIndex = 1; chosenClientIndex <= _currentReadClientsNR; chosenClientIndex++)
                                    {
                                        clientArrayR[chosenClientIndex - 1] = new ClientRead();

                                    }
                                    var resultsR = new QueryStatusRead[_currentReadClientsNR]; 
                                    await Parallel.ForEachAsync(Enumerable.Range(0, _currentReadClientsNR), new ParallelOptions() { MaxDegreeOfParallelism = _currentReadClientsNR }, async (index, token) => { resultsR[index] = await RunReadTask(clientArrayR[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                    await glancesR.EndMonitorAsync().ConfigureAwait(false);

                                    using (var csvLoggerR = new CsvLogger<LogRecordRead>("read"))
                                    {                                    
                                        foreach (var result in resultsR)
                                        { 
                                            var recordR = result.PerformanceMetric.ToLogRecord(Mode, percentage, result.Timestamp, result.StartDate,   batchSize, _currentReadClientsNR, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                            csvLoggerR.WriteRecord(recordR);
                                        }                                        
                                    }

                                }

                            }
                        }
                        GC.Collect();
                    }
                }




            }
            Console.Out.WriteLine("MixedWL-Completed");

        }



        private async static Task Batching(bool log)
        {
            var init = Config.GetQueryType(); // Just for Init the Array

            int _TestRetryIteration = 0;
            {
                while ( _TestRetryIteration < Config.GetTestRetries())
                {
                    _TestRetryIteration++;
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

                                if (_TestRetryIteration > Config.GetTestRetries())
                                {
                                    totalClientsNb = clientNumberArray.Last() + 1;
                                }

                                var date = Config.GetStartTime().AddDays(loop * daySpan); 
                                var clientArrayW = new ClientWrite[totalClientsNb];

                                for (var chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                {
                                    clientArrayW[chosenClientIndex - 1] = new ClientWrite(chosenClientIndex, totalClientsNb, Config.GetSensorNumber(), batchSize, dimNb, date);
                                }
                                var glancesW = new GlancesStarter(Operation.BatchIngestion, totalClientsNb, batchSize, sensorsNb);
                                var resultsW = new QueryStatusWrite[totalClientsNb]; 
                                await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsW[index] = (await RunIngestionTask(clientArrayW[index]).ConfigureAwait(false)); }).ConfigureAwait(false);
                                await glancesW.EndMonitorAsync().ConfigureAwait(false);

                                using (var csvLoggerW = new CsvLogger<LogRecordWrite>("write"))
                                {

                                    foreach (var result in resultsW)
                                    {  
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, 0,
                                            result.Timestamp, result.StartDate,  batchSize, totalClientsNb, sensorsNb,
                                            result.Client, result.Iteration, dimNb);
                                        csvLoggerW.WriteRecord(record);
                                    }
                                }
                                loop++;
                            }
                        }
                        GC.Collect();
                    }
                }
            }
        }

        private static async Task BenchmarkReadData()
        {
            var init = Config.GetQueryType(); // Just to Init the Array
            int[] clientNumberArray = Config.GetClientNumberOptions();
            int[] dimNbArray = Config.GetDataDimensionsNrOptions();
            int _TestRetryIteration = 0;
            {
                while ( _TestRetryIteration < Config.GetTestRetries())
                {
                    _TestRetryIteration++;
                    foreach (var totalClientsNb in clientNumberArray)
                    {
                        _currentReadClientsNR = totalClientsNb;

                        foreach (var dimNb in dimNbArray)
                        {
                            Config._actualDataDimensionsNr = dimNb;
                            foreach (string Query in Config._QueryArray)
                            {
                                if (_TestRetryIteration > Config.GetTestRetries())
                                {
                                    _currentReadClientsNR = clientNumberArray.Last() + 1;
                                }
                                _currentlimit = (int)(double)Config.GetBatchSizeOptions().Last();


                                Config.QueryTypeOnRunTime = Query;
                                var client = new ClientRead();
                                var sensorsNb = Config.GetSensorNumber();

                                    var glancesR = new GlancesStarter(Config.QueryTypeOnRunTime.ToEnum<Operation>(), _currentClientsNR, _currentlimit, sensorsNb);                  
                                    var clientArrayR = new ClientRead[totalClientsNb]; 

                                    for (int chosenClientIndex = 1; chosenClientIndex <= totalClientsNb; chosenClientIndex++)
                                    {
                                            clientArrayR[chosenClientIndex - 1] = new ClientRead();
                                    }
                                    var resultsR = new QueryStatusRead[totalClientsNb];   
                                    await Parallel.ForEachAsync(Enumerable.Range(0, totalClientsNb), new ParallelOptions() { MaxDegreeOfParallelism = totalClientsNb }, async (index, token) => { resultsR[index] = await RunReadTask(clientArrayR[index]).ConfigureAwait(false); }).ConfigureAwait(false);
                                    await glancesR.EndMonitorAsync().ConfigureAwait(false);
                                     
                                using (var csvLogger = new CsvLogger<LogRecordRead>("read"))
                                {
                                    foreach (var result in resultsR)
                                    {   
                                        var record = result.PerformanceMetric.ToLogRecord(Mode, -1, result.Timestamp, result.StartDate,   _currentlimit, totalClientsNb, sensorsNb,
                                          result.Client, result.Iteration, dimNb);
                                        csvLogger.WriteRecord(record);
                                    }
                                }
                            }
                        }
                    }
                    GC.Collect();
                }

            }
        }
    }
}
