using System;

namespace BenchmarkTool.Generators
{
    public class DataGenerator : IDataGenerator
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

        private DateTime GetRecordTimestamp(DateTime baseTime)
        {
            return baseTime.AddMilliseconds(_rnd.Next(1000));
        }
    }
}
