using System;
using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class DefaultDataGenerator : IDataGenerator
    {
        private Random _rnd = new Random();

        public Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date)
        {
            int loop = Config.GetTestRetries();
            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random();
            Batch batch = new Batch(batchSize);
            var start = (loop * clientOffset + offset) * batchSize + sensorStartId >= sensorsPerClient + sensorStartId ? sensorStartId : (loop * clientOffset + offset) * batchSize + sensorStartId;
            for (int k = 0; k < batchSize; k++)
            {
                var recordTimestamp = GetRecordTimestamp(date);
                batch.Records.Add(recordFactory.Create(start, recordTimestamp, rndval.Next()));
                start++;
                if (start >= sensorStartId + sensorsPerClient)
                    start = sensorStartId;
            }
            return batch;
        }

        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date) // date is here relative to the number of batches which have been written before and th eTestretries
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
                    var recordTimestamp = GetRecordTimestamp(date);

                    batch.Records.Add(recordFactory.Create(chosenSensor, recordTimestamp, rndval.Next()));
                }

            
            return batch;
        }
        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions) // date is here relative to the number of batches which have been written before and th eTestretries
        {

            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random(); //TODO seeded
            Batch batch = new Batch(batchSize);
            
             List<float>inputList = new List<float>();
                for (int c = 1; c <= dimensions; c++)
                {
                   inputList.Add(rndval.Next() );
                }
                


                int timeindex = 0;
                for (int dataPointNr = 0; dataPointNr < batchSize; dataPointNr++)
                {
                    var chosenSensor = dataPointNr % sensorIdsForThisClientList.Count;
                    if (chosenSensor == 0 && dataPointNr != 0)
                    {
                        timeindex++;
                    }
                    var recordTimestamp = GetRecordTimestamp(date);

                    batch.Records.Add(recordFactory.Create(chosenSensor, recordTimestamp, inputList.ToArray() ));
                }

            
            return batch;
        }

        private DateTime GetRecordTimestamp(DateTime baseTime)
        {
            return baseTime.AddMilliseconds(_rnd.Next(1000));
        }
    }
}
          