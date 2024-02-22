using System;

namespace BenchmarkTool.Queries
{
    public class RangeQuery
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

        public int[] SensorIDs { get; } // can be renamed in SensorFilterIDs

        public RangeQuery(DateTime startDate, long duration, int[] sensorfilter)
        {
            StartDate = startDate;
            DurationMinutes = duration;
            SensorIDs = sensorfilter;
        }
    }
}
