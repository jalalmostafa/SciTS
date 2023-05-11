using System;

namespace BenchmarkTool.Generators
{
    public class RecordTimescaleDB : IRecord
    {
        public int SensorID { get; set; }
        public float Value { get; set; }
                public float[] ValuesArray { get; set; }
public bool polyDim { get;  }
        public DateTime Time { get; set; }
        float getFirstValue(){
            return ValuesArray[1];
        }
        public RecordTimescaleDB(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            Value = value;
        }
        public RecordTimescaleDB(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
    }
}
