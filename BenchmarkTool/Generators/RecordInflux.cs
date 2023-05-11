﻿using InfluxDB.Client.Core;
using System;

namespace BenchmarkTool.Generators
{
    public class RecordInflux : IRecord
    {
        public int SensorID { get; set; }
        public bool polyDim { get; }

        public float Value { get; set; }
        public float[] ValuesArray { get; set; }
         public DateTime Time { get; set; }
        float getFirstValue()
        {
            return ValuesArray[1];
        }
        public RecordInflux(int sensorId, DateTime timestamp, float value)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            Value = value;
        }
        public RecordInflux(int sensorId, DateTime timestamp, float[] values)
        {
            SensorID = sensorId;
            Time = TimeZoneInfo.ConvertTimeToUtc(timestamp);
            ValuesArray = values;
        }
    }

}
