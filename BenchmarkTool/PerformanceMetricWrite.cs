using BenchmarkTool;
namespace BenchmarkTool
{
    public class PerformanceMetricWrite : PerformanceMetricBase<LogRecordWrite>
    {
        public PerformanceMetricWrite(double latency, long succeededDataPoints,
                                     long failedDataPoints, Operation operation)
                : base(latency, succeededDataPoints, failedDataPoints, operation)
        {
        }

        public override LogRecordWrite ToLogRecord(long timestamp, int batchSize,
                                                    int clientsNb, int sensorNb,
                                                    int client, int iteration)
        {
            return new LogRecordWrite(Latency, SucceededDataPoints, timestamp,
                                    FailedDataPoints, PerformedOperation, clientsNb,
                                    batchSize, sensorNb, client, iteration);
        }
    }
}
