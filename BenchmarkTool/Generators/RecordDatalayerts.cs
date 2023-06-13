using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordDatalayerts : IRecord, IEnumerable
    {
        public RecordDatalayerts(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = timestamp;
            // Value = value;
            ValuesArray = new float[1];
            ValuesArray[0] = value;
        }
        public RecordDatalayerts(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
            // polyDim = true;
        }
        float getFirstValue(){
            return ValuesArray[0];
        }
         public float[] ValuesArray { get; set; }
         public float Value { get; set; }
        //  public bool polyDim { get;  }

        public int SensorID { get; set; }

        public DateTime Time { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return SensorID;

            yield return Time;
            // if (polyDim == true)
                yield return ValuesArray;
            // else
            //     yield return Value;
        }

    }
}
