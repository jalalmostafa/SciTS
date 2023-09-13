using System;

namespace BenchmarkTool.Generators
{
    public interface IRecord
    {
        int SensorID { get; set; }
        // double Value { get; set; }
        double[] ValuesArray { get; set; }  
        DateTime Time { get; set; }
        // bool polyDim { get;  }

        double getFirstValue(){
            return ValuesArray[0];
        }



    }
}
