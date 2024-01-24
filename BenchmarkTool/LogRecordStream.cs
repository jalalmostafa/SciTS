using System;

namespace BenchmarkTool
{
    public class LogRecordStream
    {
        public long Duration { get; }
        public long SucceededDataPoints { get; }
        public long FailedDataPoints { get; }
        public string Operation { get; }
        public string TargetDatabase { get; }
        public double Latency { get; }
        public int ClientsNumber { get; }
        public int Loop { get; }
        public int SensorsNumber { get; }
        public int Dimensions { get; }
        public LogRecordStream(double latency, long succeededPoints, long epoch,
                                long failedPoints, Operation operation, int clientsNb,
                                int sensorsNb)
        {
            Latency = latency;
            SucceededDataPoints = succeededPoints;
            FailedDataPoints = failedPoints;
            Operation = operation.ToString();
            Duration = epoch;
            TargetDatabase = Config.GetTargetDatabase();
            SensorsNumber = sensorsNb;
            Dimensions = Config.GetDataDimensionsNr();
            ClientsNumber = clientsNb;
            Loop = Config.GetTestRetries();
        }
    }
}
