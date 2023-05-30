using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordMySQLDB : IRecord
    {
        public int SensorID { get; set; }
        // public float Value { get; set; }
                public float[] ValuesArray { get; set; }
// public bool polyDim { get;  }
        public DateTime Time { get; set; }

        public RecordMySQLDB(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            // Value = value;
                    ValuesArray[0] = value;

        }
        public RecordMySQLDB(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }

        float getFirstValue(){
            return ValuesArray[0];
        }
        public IEnumerator GetEnumerator()
        {
            yield return SensorID;
            yield return ValuesArray;
            yield return Time;
        }
    }
}
