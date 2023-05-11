using System;

namespace BenchmarkTool.Queries
{
    public class OORangeQuery
    {
        public DateTime StartDate { get; }

        public DateTime EndDate
        {
            get
            {
                return StartDate.AddMinutes(DurationMinutes);
            }
        }

        public long DurationMinutes { get; }

        public int SensorID { get; }

        public double MaxValue { get; }

        public double MinValue { get; }
        public OORangeQuery(DateTime startDate, long duration, int sensorId, double max, double min)
        {
            StartDate = startDate;
            DurationMinutes = duration;
            SensorID = sensorId;
            MaxValue = max;
            MinValue = min;
        }
    }
}
