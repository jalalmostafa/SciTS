using InfluxDB.Client.Core;
using System;

namespace BenchmarkTool.Generators
{
    public class RecordInflux : IRecord
    {
        public int SensorID { get; set; }
        public double[] ValuesArray { get; set; }
         public DateTime Time { get; set; }
        double getFirstValue()
        {
            return ValuesArray[0];
        }
        public RecordInflux(int sensorId, DateTime timestamp, double value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            ValuesArray = new double[1];
            ValuesArray[0] = value;

        }
        public RecordInflux(int sensorId, DateTime timestamp, double[] values)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            ValuesArray = values;
        }
    }

}
