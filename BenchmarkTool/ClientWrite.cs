using Serilog;
using System;
using System.Threading.Tasks;
using System.Threading;
using BenchmarkTool.Generators;
using System.Collections.Generic;
using BenchmarkTool.Database;

namespace BenchmarkTool
{
    public class ClientWrite
    {
        private DateTime _date;
        private IDatabase _targetDb;
        private DataGenerator _dataGenerator = new DataGenerator();
        private int _daySpan;

        public int Index { get; private set; }
        public int ClientsNumber { get; private set; }
        public int SensorsNumber { get; private set; }
        public int BatchSize { get; private set; }

        public ClientWrite(int index, int clientsNumber, int sensorNumber, int batchSize, DateTime date)
        {
            try
            {
                Index = index;
                ClientsNumber = clientsNumber;
                SensorsNumber = sensorNumber;
                BatchSize = batchSize;
                _date = date;
                _daySpan = Config.GetDaySpan();
                var dbFactory = new DatabaseFactory();
                _targetDb = dbFactory.Create();
                _targetDb.Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public async Task<List<QueryStatusWrite>> RunQuery()
        {
            var operation = Config.GetQueryType();
            int loop = Config.GetTestRetries();

            decimal sensorsPerClient = SensorsNumber > ClientsNumber ?
                                        Math.Floor(Convert.ToDecimal(SensorsNumber / ClientsNumber)) : SensorsNumber;
            int startId = SensorsNumber > ClientsNumber ? Convert.ToInt32(Index * sensorsPerClient) : 0;

            var statuses = new List<QueryStatusWrite>();
            var period = 24.0 / loop;
            for (var day = 0; day < _daySpan; day++)
            {
                var date = _date.AddDays(day);
                for (var i = 0; i < loop; i++)
                {
                    // uniformly distribute batches on one day long data
                    date = date.AddHours(period);
                    var batch = _dataGenerator.GenerateBatch(BatchSize, startId, sensorsPerClient, i, Index, date);
                    var status = await _targetDb.WriteBatch(batch);
                    // Console.WriteLine($"[{Index}-{i}-{date}] [Clients Number {ClientsNumber} - Batch Size {BatchSize} - Sensors Number {SensorsNumber}] {status.PerformanceMetric.Latency}");
                    status.Iteration = i;
                    status.Client = Index;
                    statuses.Add(status);
                }
            }

            _targetDb.Cleanup();
            _targetDb.Close();
            return statuses;
        }
    }
}

