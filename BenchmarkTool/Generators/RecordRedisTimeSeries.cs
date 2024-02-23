using System;

namespace BenchmarkTool.Generators
{
    public class RecordRedisTimeSeries : IRecord
    {
        public int SensorID { get; set; }

        public float Value { get; set; }

        public DateTime Time { get; set; }

        public RecordRedisTimeSeries(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            Value = value;
        }
    }

}
