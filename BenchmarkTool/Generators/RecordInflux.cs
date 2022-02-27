using InfluxDB.Client.Core;
using System;

namespace BenchmarkTool.Generators
{
    public class RecordInflux : IRecord
    {
        public int SensorID { get; set; }

        public float Value { get; set; }

        public DateTime Time { get; set; }

        public RecordInflux(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            Value = value;
        }
    }

}
