using System;

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
            DimensionsNb = Config._actualDataDimensionsNr;

        }
        public string Mode { get; }
         public int Percentage { get; }
        public double Latency { get; }

        public long SucceededDataPoints { get; }

        public long FailedDataPoints { get; }
        public int DimensionsNb {get;}

        public Operation PerformedOperation { get; }

        public abstract T ToLogRecord(string mode, int percentage, long timestamp, DateTime startDate, int batchSize, int clientsNb, int sensorNb, int client, int iteration, int dimNb);
    }
}