using System;

namespace BenchmarkTool.Generators
{
    public interface IRecord
    {
        int SensorID { get; set; }
        // float Value { get; set; }
        float[] ValuesArray { get; set; } // TODO check if obj ansatz besser sist
        DateTime Time { get; set; }
        bool polyDim { get;  }

        float getFirstValue(){
            return ValuesArray[1];
        }



    }
}
