using System;
using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class RegularDataGenerator : IDataGenerator
    {
        private Random _rnd = new Random();
        private int timeindex;

        public Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date) // date is here relative to the number of batches which have been written before and th eTestretries
        {

            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random(); //TODO seeded
            Batch batch = new Batch(batchSize);
            var sensorid = (clientOffset + offset) * batchSize + sensorStartId >= sensorsPerClient + sensorStartId ? sensorStartId : (clientOffset + offset) * batchSize + sensorStartId;
            timeindex = 0;
            for (int k = 0; k < batchSize; k++)
            {
                var recordTimestamp = GetRecordTimestamp(date, timeindex);
                batch.Records.Add(recordFactory.Create(sensorid, recordTimestamp, rndval.Next()));
                sensorid++;
                if (sensorid >= sensorStartId + sensorsPerClient)
                {
                    sensorid = sensorStartId;
                    timeindex++;
                }
            }
            return batch;
        }
        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date) // date is here relative to the number of batches which have been written before and th eTestretries
        {

            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random(); //TODO seeded
            Batch batch = new Batch(batchSize);

            timeindex = 0;
            for (int dataPointNr = 0; dataPointNr < batchSize; dataPointNr++)
            {
                var chosenSensor = dataPointNr % sensorIdsForThisClientList.Count;
                if (chosenSensor == 0 && dataPointNr != 0)
                {
                    timeindex++;
                }
                var recordTimestamp = GetRecordTimestamp(date, timeindex);
                batch.Records.Add(recordFactory.Create(chosenSensor, recordTimestamp, rndval.Next()));


            }
            return batch;
        }
        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions) // date is here relative to the number of batches which have been written before and th eTestretries
        {

            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random(); //TODO seeded
            Batch batch = new Batch(batchSize);




            int timeindex = 0;
            for (int dataPointNr = 0; dataPointNr < batchSize; dataPointNr++)
            {
                var chosenSensor = dataPointNr % sensorIdsForThisClientList.Count;
                if (chosenSensor == 0 && dataPointNr != 0)
                {
                    timeindex++;
                }
                var recordTimestamp = GetRecordTimestamp(date, timeindex);

                batch.Records.Add(recordFactory.Create(chosenSensor, recordTimestamp, GetInput(dimensions)));
            }


            return batch;
        }
        private float[] GetInput(int dimensions)
        {

            List<float> inputList = new List<float>();
            for (int c = 1; c <= dimensions; c++)
            {
                inputList.Add(new Random().Next());
            }
            return inputList.ToArray();
        }
        private DateTime GetRecordTimestamp(DateTime baseTime, int timeindex)
        {
            return baseTime.AddMilliseconds(Config.GetDatalayertsScaleMilliseconds() * timeindex);
        }
    }
}
