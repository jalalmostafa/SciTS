using System;

namespace BenchmarkTool.Generators
{
    public class RecordMySQLDB : IRecord
    {
        public int SensorID { get; set; }
        public float Value { get; set; }
        public DateTime Time { get; set; }

        public RecordMySQLDB(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            Value = value;
        }
    }
}
