using System;

namespace BenchmarkTool.Generators
{
    public interface IRecord 
    {
        int SensorID { get; set; }
        float Value { get; set; }
        DateTime Time { get; set; }
    }
}
