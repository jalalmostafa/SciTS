using NodaTime.TimeZones;
using System;

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
           int Percentage { get; }

        T ToLogRecord( string mode, int percentage, long timestamp, DateTime startDate, int batchSize, int clientsNb, int sensorNb, int client, int iteration, int dimNb);

    }
}
