using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordClickhouse : IRecord, IEnumerable
    {
        public int SensorID { get; set; }
         public double[] ValuesArray { get; set; }
        public DateTime Time { get; set; }

        public RecordClickhouse(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = new double[1];
            ValuesArray[0] = value;

        }
        public bool polyDim { get; }

        public RecordClickhouse(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
        double getFirstValue()
        {
            return ValuesArray[0];
        }
        public IEnumerator GetEnumerator()
        {
            yield return Time;
            yield return SensorID;
            foreach (int n in ValuesArray)
            {
                yield return ValuesArray[n];
            }

        }
    }
}

