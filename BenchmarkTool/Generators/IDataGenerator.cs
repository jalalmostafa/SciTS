using System;

namespace BenchmarkTool.Generators
{
    public interface IDataGenerator
    {
        Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date);
    }
}
