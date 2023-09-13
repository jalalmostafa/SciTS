using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordDatalayerts : IRecord, IEnumerable
    {
        public RecordDatalayerts(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = timestamp;
            // Value = value;
            ValuesArray = new double[1];
            ValuesArray[0] = value;
        }
        public RecordDatalayerts(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
            // polyDim = true;
        }
        double getFirstValue(){
            return ValuesArray[0];
        }
         public double[] ValuesArray { get; set; }
         public double Value { get; set; }
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
