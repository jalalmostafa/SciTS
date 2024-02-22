using NodaTime.TimeZones;
using System;

namespace BenchmarkTool
{
    public interface IMetric<T>
    {
        double Latency { get; }
        double ClientLatency { get;} // measures not only the server's answer time, but also the Clients Batch Generation Time and the API or DatabaseDB.class-calculation times.

        long SucceededDataPoints { get; }

        long FailedDataPoints { get; }
        int DimensionsNb {get;}

        Operation PerformedOperation { get; }
        string Mode { get;}
        int Percentage { get; }

        T ToLogRecord( string mode, int percentage, long timestamp, DateTime startDate, int batchSize, int clientsNb, int sensorNb, int client, int iteration, int dimNb);

    }
}
