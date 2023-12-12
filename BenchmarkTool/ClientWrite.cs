using Serilog;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using BenchmarkTool.Generators;
using System.Collections.Generic;
using BenchmarkTool.Database;
using System.Linq;

namespace BenchmarkTool
{
    public class ClientWrite
    {
        private DateTime _date;
        private IDatabase _targetDb;
        private int _daySpan;

        public int _chosenClientIndex { get; private set; }
        public int _totalClientsNumber { get;   set; }
        public int _SensorsNumber { get; private set; }
        public int _BatchSize { get; private set; }
        public int _DimNb { get; private set; }

        public ClientWrite(int chosenClientIndex, int totalClientsNumber, int sensorNumber, int batchSize, int dimNb, DateTime date)
        {
            try
            {
 Thread.CurrentThread.CurrentCulture =  CultureInfo.InvariantCulture;

                _chosenClientIndex = chosenClientIndex;
                _totalClientsNumber = totalClientsNumber;
                _SensorsNumber = sensorNumber;
                _BatchSize = batchSize;
                _date = date;
                _daySpan = Config.GetDaySpan();
                var dbFactory = new DatabaseFactory();
                _targetDb = dbFactory.Create();
                _targetDb.Init();
                _DimNb = dimNb;
                _targetDb.CheckOrCreateTable();


            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        public async Task<List<QueryStatusWrite>> RunIngestion(int TestRetryWriteIteration)
        {
            return await RunIngestion(new ExtendedDataGenerator(), TestRetryWriteIteration);
        }

        private async Task<List<QueryStatusWrite>> RunIngestion(ExtendedDataGenerator dataGenerator, int TestRetryWriteIteration)
        {

            // new logic: modulo

            if (_totalClientsNumber > _SensorsNumber) throw new ArgumentException("clientsnr  must be lower then sensornumber for reg.TS ingestion");

            List<int> sensorIdsForThisClientList = new List<int>();
            for (int chosenSensor = 1; chosenSensor <= _SensorsNumber; chosenSensor++) // inspired by tilted time frame algorithm
            {
                if (chosenSensor % _totalClientsNumber == _chosenClientIndex - 1)
                    sensorIdsForThisClientList.Add(chosenSensor);
            }

            var statuses = new List<QueryStatusWrite>();

            {
                var batchStartdate = _date;
                Batch batch = dataGenerator.GenerateBatch(_BatchSize, sensorIdsForThisClientList, batchStartdate, _DimNb);

                var status = await _targetDb.WriteBatch(batch).ConfigureAwait(false);
                Console.WriteLine($"[ClientID:{_chosenClientIndex}-Iteraton:{TestRetryWriteIteration}-Date:{batchStartdate}] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [Client Number{_chosenClientIndex} out of totalClNb:{_totalClientsNumber} - Batch Size {_BatchSize} - Sensors Numbers {String.Join( ';' , sensorIdsForThisClientList)} of {_SensorsNumber} with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");
                status.Iteration = TestRetryWriteIteration;
                status.Client = _chosenClientIndex;
                status.StartDate=batchStartdate;
                statuses.Add(status);
            }


            _targetDb.Cleanup();
            _targetDb.Close();
            return statuses;
        }

        private async Task<List<QueryStatusWrite>> RunIngestion(SimpleDataGenerator dataGenerator) // deprecated
        {
            var operation = Config.GetQueryType();
            int loop = Config.GetTestRetries();

            decimal sensorsPerClient = _SensorsNumber > _totalClientsNumber ?
                                        Math.Floor(Convert.ToDecimal(_SensorsNumber / _totalClientsNumber)) : _SensorsNumber;
            int startId = _SensorsNumber > _totalClientsNumber ? Convert.ToInt32(_chosenClientIndex * sensorsPerClient) : 0;

            var statuses = new List<QueryStatusWrite>();
            var period = 24.0 / loop;
            for (var day = 0; day < _daySpan; day++)
            {
                var batchStartdate = _date.AddDays(day);
                for (var i = 0; i < loop; i++)
                {
                    // uniformly distribute batches on one day long data
                    batchStartdate = batchStartdate.AddHours(period);
                    Batch batch;
                    batch = dataGenerator.GenerateBatch(_BatchSize, startId, sensorsPerClient, i, _chosenClientIndex, batchStartdate);
                    var status = await _targetDb.WriteBatch(batch);
                    Console.WriteLine($"[ClientID{_chosenClientIndex}-Iteration:{i}-{batchStartdate}] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [Client Number{_chosenClientIndex} out of totalClNb:{_totalClientsNumber}  - Batch Size {_BatchSize} - Sensors Number {_SensorsNumber}] {status.PerformanceMetric.Latency}");
                    status.Iteration = i;
                    status.Client = _chosenClientIndex;
                    statuses.Add(status);
                }
            }

            _targetDb.Cleanup();
            _targetDb.Close();
            return statuses;
        }
    }
}

