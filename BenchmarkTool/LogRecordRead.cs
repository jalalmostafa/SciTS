using System;

namespace BenchmarkTool
{
    public class LogRecordRead
    {
        public long Date { get; }
        public double SucceededDataPoints { get; }
        public double FailedDataPoints { get; }
        public string Operation { get; }
        public double Latency { get; }
        public double ClientLatency { get; }
        public string Mode { get; }
        public int WLPercentage { get; }
        public string TargetDatabase { get; }
        public int Client { get; }
        public int ClientsNumber { get; }
        public int SensorsNumber { get; }
        public DateTime StartDate { get; }
        public long Duration { get; }
        public int Iteration { get; }
        public int Aggregation { get; }

        public int Dimensions { get; }

        public LogRecordRead(double latency, double clientlatency, double succeededPoints, long epoch, int sensNb,
                            double failedPoints, Operation operation, string mode,int percentage, int client, int clientsNb,
                            DateTime startDate, long duration, int aggregation, int iteration, int dimNb)
        {
            SensorsNumber = sensNb;
            Latency = latency;
            ClientLatency = clientlatency;
            SucceededDataPoints = succeededPoints;
            FailedDataPoints = failedPoints;
            Operation = operation.ToString();
            Mode = mode;
            WLPercentage = percentage;

            Date = epoch;
            Client = client;
            ClientsNumber = clientsNb;
            TargetDatabase = Config.GetTargetDatabase();
            StartDate = startDate;
            Dimensions = dimNb;
            Duration = duration;
            Aggregation = aggregation;
            Iteration = iteration;



        }
    }
}
