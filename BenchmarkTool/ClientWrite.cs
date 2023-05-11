﻿using Serilog;
using System;
using System.Threading.Tasks;
using System.Threading;
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
        public int _totalClientsNumber { get; private set; }
        public int _SensorsNumber { get; private set; }
        public int _BatchSize { get; private set; }

        public ClientWrite(int chosenClientIndex, int totalClientsNumber, int sensorNumber, int batchSize, DateTime date)
        {
            try
            {
                _chosenClientIndex = chosenClientIndex;
                _totalClientsNumber = totalClientsNumber;
                _SensorsNumber = sensorNumber;
                _BatchSize = batchSize;
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
        public async Task<List<QueryStatusWrite>> RunIngestion()
        {
                    return await RunIngestion(new EnhancedDataGenerator());
        }

        public async Task<List<QueryStatusWrite>> RunIngestion(EnhancedDataGenerator dataGenerator) //Reg() // instead TODO: RunINgestion(RegularDataGenerator ddd)
        {

            // new logic: modulo

            if (_totalClientsNumber > _SensorsNumber) throw new ArgumentException("clientsnr  must be lower then sensornumber for reg.TS ingestion");

            List<int> sensorIdsForThisClientList = new List<int>();
            for (int chosenSensor = 1; chosenSensor <= _SensorsNumber; chosenSensor++)
            {
                if (chosenSensor % _totalClientsNumber == _chosenClientIndex - 1)
                    sensorIdsForThisClientList.Add(chosenSensor);
            }

            var statuses = new List<QueryStatusWrite>();

            for (var i = 0; i < Config.GetTestRetries(); i++) // every Benchmark iteration writes to another year
            {
                var batchStartdate = _date.AddYears(i);
                Batch batch = dataGenerator.GenerateBatch(_BatchSize, sensorIdsForThisClientList, batchStartdate, Config.GetDataDimensionsNr());

                var status = await _targetDb.WriteBatch(batch);
                Console.WriteLine($"[{_chosenClientIndex}-{i}-{batchStartdate}] [Clients Number {_totalClientsNumber} - Batch Size {_BatchSize} - Sensors Number {_SensorsNumber} with Dimensions:{Config.GetDataDimensionsNr()}] Latency:{status.PerformanceMetric.Latency}");
                status.Iteration = i;
                status.Client = _chosenClientIndex;
                statuses.Add(status);
            }


            _targetDb.Cleanup();
            _targetDb.Close();
            return statuses;
        }

        public async Task<List<QueryStatusWrite>> RunIngestion(SimpleDataGenerator dataGenerator)
        {
            var operation = Config.GetQueryType();
            int loop = Config.GetTestRetries();

            decimal sensorsPerClient = _SensorsNumber > _totalClientsNumber ?
                                        Math.Floor(Convert.ToDecimal(_SensorsNumber / _totalClientsNumber)) : _SensorsNumber;
            int startId = _SensorsNumber > _totalClientsNumber ? Convert.ToInt32(_chosenClientIndex * sensorsPerClient) : 0;

            var statuses = new List<QueryStatusWrite>();
            var period = 24.0 / loop;
            for (var day = 0; day < _daySpan; day++)   // TODO  hae? adds day in a until dayspan loop
            {
                var batchStartdate = _date.AddDays(day);
                for (var i = 0; i < loop; i++)
                {
                    // uniformly distribute batches on one day long data
                    batchStartdate = batchStartdate.AddHours(period);
                    Batch batch;
                    batch = dataGenerator.GenerateBatch(_BatchSize, startId, sensorsPerClient, i, _chosenClientIndex, batchStartdate);
                    var status = await _targetDb.WriteBatch(batch);
                    Console.WriteLine($"[{_chosenClientIndex}-{i}-{batchStartdate}] [Clients Number {_totalClientsNumber} - Batch Size {_BatchSize} - Sensors Number {_SensorsNumber}] {status.PerformanceMetric.Latency}");
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

