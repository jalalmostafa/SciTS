using System;

namespace BenchmarkTool.Queries
{
    public class SpecificQuery
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

        public SpecificQuery(DateTime startDate, long duration, int sensorId)
        {
            StartDate = startDate;
            DurationMinutes = duration;
            SensorID = sensorId;
        }
    }
}
