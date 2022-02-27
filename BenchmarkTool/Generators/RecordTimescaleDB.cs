using System;

namespace BenchmarkTool.Generators
{
    public class RecordTimescaleDB : IRecord
    {
        public int SensorID { get; set; }
        public float Value { get; set; }
        public DateTime Time { get; set; }

        public RecordTimescaleDB(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            Value = value;
        }
    }
}
