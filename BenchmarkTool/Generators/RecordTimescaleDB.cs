using System;

namespace BenchmarkTool.Generators
{
    public class RecordTimescaleDB : IRecord
    {
        public int SensorID { get; set; }
        public double[] ValuesArray { get; set; }
        public DateTime Time { get; set; }
        
        public RecordTimescaleDB(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = new double[1];
            ValuesArray[0] = value;

        }
        public RecordTimescaleDB(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = timestamp;
            ValuesArray = values;
        }
    }
}
