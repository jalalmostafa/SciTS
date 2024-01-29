using System;

namespace BenchmarkTool.Generators
{
    public interface IRecord
    {
        int SensorID { get; set; }
         double[] ValuesArray { get; set; }  
        DateTime Time { get; set; }
 
        double getFirstValue(){
            return ValuesArray[0];
        }



    }
}
