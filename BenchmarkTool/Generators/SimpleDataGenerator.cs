using System;
using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class SimpleDataGenerator : IDataGenerator // OLD -> see ExtendedDataGenerator.cs
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
                batch.RecordsList.Add(recordFactory.Create(start, recordTimestamp, rndval.Next()));
                start++;
                if (start >= sensorStartId + sensorsPerClient)
                    start = sensorStartId;
            }
            return batch;
        }

        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions) // date is here relative to the number of batches which have been written before and th eTestretries
        {
            throw new NotImplementedException();
        }

        private DateTime GetRecordTimestamp(DateTime baseTime)
        {
            return baseTime.AddMilliseconds(_rnd.Next(1000));
        }
    }
}
