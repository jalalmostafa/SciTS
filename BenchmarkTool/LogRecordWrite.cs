using System;

namespace BenchmarkTool
{
    public class LogRecordWrite
    {
        public long Date { get; }
        public double SucceededDataPoints { get; }
        public double FailedDataPoints { get; }
        public string Operation { get; }
        public double Latency { get; }
        public string Mode { get; }
        public int WLPercentage { get; }

        public string TargetDatabase { get; }
        public int Client { get; }
        public int ClientsNumber { get; }
        public int SensorsNumber { get; }
                public DateTime StartDate { get; }

        public int BatchSize { get; }

        public int Iteration { get; }

        public int Dimensions { get; }

        public LogRecordWrite(double latency, double succeededPoints, long epoch, DateTime startDate,
                                double failedPoints, Operation operation, string mode, int percentage, int clientsNb,
                                int batchSize, int sensorsNb, int client, int iteration, int dimNb)
        {
            Latency = latency;
            SucceededDataPoints = succeededPoints;
            FailedDataPoints = failedPoints;
            Operation = operation.ToString();
            StartDate = startDate;

            Mode = mode;
            WLPercentage = percentage;
            Date = epoch;
            TargetDatabase = Config.GetTargetDatabase();
            SensorsNumber = sensorsNb;
            BatchSize = batchSize;
            ClientsNumber = clientsNb;
            Client = client;
            Iteration = iteration;
            Dimensions = dimNb;

        }
    }
}
