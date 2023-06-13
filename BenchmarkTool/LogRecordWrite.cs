using System;

namespace BenchmarkTool
{
    public class LogRecordWrite
    {
        public long Date { get; }
        public double SucceededDataPoints { get; }
        public double FailedDataPoints { get; }
        public string Operation { get; }
        public string Mode {get;}
        public string TargetDatabase { get; }
        public double Latency { get; }
        public int ClientsNumber { get; }
        public int BatchSize { get; }
        public int Loop { get; }
        public int SensorsNumber { get; }
        public int Client { get; }
        public int Iteration { get; }

        public int Dimensions { get; }

        public LogRecordWrite(double latency, double succeededPoints, long epoch,
                                double failedPoints, Operation operation,string mode, int clientsNb,
                                int batchSize, int sensorsNb, int client, int iteration)
        {
            Latency = latency;
            SucceededDataPoints = succeededPoints;
            FailedDataPoints = failedPoints;
            Operation = operation.ToString();
            Mode = mode;
            Date = epoch;
            TargetDatabase = Config.GetTargetDatabase();
            SensorsNumber = sensorsNb;
            Dimensions = Config.GetDataDimensionsNr();
            BatchSize = batchSize;
            ClientsNumber = clientsNb;
            Loop = Config.GetTestRetries();
            Client = client;
            Iteration = iteration;
            
        }
    }
}
