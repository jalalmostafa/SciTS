using System;

namespace BenchmarkTool.Queries
{
    public class ComparisonQuery
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

        public int FirstSensorID { get; }

        public int SecondSensorID { get; }

        public ComparisonQuery(DateTime startDate, long duration, int id1, int id2)
        {
            StartDate = startDate;
            DurationMinutes = duration;
            FirstSensorID = id1;
            SecondSensorID = id2;
        }
    }
}
