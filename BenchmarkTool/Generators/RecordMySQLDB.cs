using System;
using System.Collections;

namespace BenchmarkTool.Generators
{
    public class RecordMySQLDB : IRecord
    {
        public int SensorID { get; set; }
        
        public double[] ValuesArray { get; set; }
        
        public DateTime Time { get; set; }

        public RecordMySQLDB(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = new double[1];

            ValuesArray[0] = value;

        }
        public RecordMySQLDB(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }

        double getFirstValue(){
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
