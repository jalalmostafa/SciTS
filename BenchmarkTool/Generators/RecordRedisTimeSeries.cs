using System;

namespace BenchmarkTool.Generators
{
    public class RecordRedisTimeSeries : IRecord
    {
        public int SensorID { get; set; }

        public DateTime Time { get; set; }

        public double[] ValuesArray { get; set; }

        public RecordRedisTimeSeries(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            ValuesArray[0] = value;
        }

        public RecordRedisTimeSeries(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
    }

}
