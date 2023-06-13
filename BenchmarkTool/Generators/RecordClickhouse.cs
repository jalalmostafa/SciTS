using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordClickhouse : IRecord, IEnumerable
    {
        public int SensorID { get; set; }
        // public float Value { get; set; }
        public float[] ValuesArray { get; set; }
        public DateTime Time { get; set; }

        public RecordClickhouse(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            // Value = value;
                        ValuesArray = new float[1];

            ValuesArray[0] = value;

        }
        public bool polyDim { get; }

        public RecordClickhouse(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
        float getFirstValue()
        {
            return ValuesArray[0];
        }
        public IEnumerator GetEnumerator()
        {
            yield return Time;
            yield return SensorID;
            // yield return ValuesArray;
            foreach (int n in ValuesArray)
            {
                yield return ValuesArray[n];
            }

        }
    }
}

