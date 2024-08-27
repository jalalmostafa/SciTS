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
            ValuesArray = new double[1];
            ValuesArray[0] = value;
        }
        public RecordDatalayerts(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }

        public double[] ValuesArray { get; set; }

        public int SensorID { get; set; }

        public DateTime Time { get; set; }

        public IEnumerator GetEnumerator()
        {
            yield return SensorID;

            yield return Time;
            yield return ValuesArray;

        }

    }
}
