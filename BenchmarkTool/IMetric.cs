namespace BenchmarkTool
{
    public interface IMetric<T>
    {
        double Latency { get; }

        long SucceededDataPoints { get; }

        long FailedDataPoints { get; }
        int DimensionsNb {get;}

        Operation PerformedOperation { get; }
        string Mode { get;}

        T ToLogRecord( string mode,long timestamp, int batchSize, int clientsNb, int sensorNb, int client, int iteration, int dimNb);

    }
}
