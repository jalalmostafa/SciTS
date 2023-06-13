using System;

namespace BenchmarkTool
{
    public class LogRecordRead
    {
        public long Date { get; }
        public double SucceededDataPoints { get; }
        public double FailedDataPoints { get; }
        public string Operation { get; }
        public string TargetDatabase { get; }
        public double Latency { get; }
        public long Duration { get; }
        public int Loop { get; }
        public DateTime StartDate { get; }
        public int Aggregation { get; }

        public int Dimensions { get; }

        public LogRecordRead(double latency, double succeededPoints, long epoch,
                            double failedPoints, Operation operation,
                            DateTime startDate, long duration, int aggregation)
        {
            Latency = latency;
            SucceededDataPoints = succeededPoints;
            FailedDataPoints = failedPoints;
            Operation = operation.ToString();
            Date = epoch;
            TargetDatabase = Config.GetTargetDatabase();
            StartDate = startDate;
             Dimensions = Config.GetDataDimensionsNr();
            Duration = duration;
            Aggregation = aggregation;
            Loop = Config.GetTestRetries();


        }
    }
}
