namespace BenchmarkTool
{
    public abstract class PerformanceMetricBase<T> : IMetric<T>
    {

        public PerformanceMetricBase(double latency, long succeededDataPoints,
                                    long failedDataPoints, Operation operation)
        {
            Latency = latency;
            SucceededDataPoints = succeededDataPoints;
            FailedDataPoints = failedDataPoints;
            PerformedOperation = operation;
        }

        public double Latency { get; }

        public long SucceededDataPoints { get; }

        public long FailedDataPoints { get; }

        public Operation PerformedOperation { get; }

        public abstract T ToLogRecord(long timestamp, int batchSize, int clientsNb, int sensorNb, int client, int iteration);
    }
}