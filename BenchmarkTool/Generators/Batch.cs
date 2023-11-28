using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class Batch
    {
        public Batch (){
            RecordsList = new List<IRecord>();
        }

        public Batch(int size)
        {
            Size = size;
             // Records = new List<IRecord>(); // OLD
            RecordsArray = new IRecord[size];
        }
        public List<IRecord> RecordsList { get; set; }
        public IRecord[] RecordsArray { get; set; }
        public int Size { get; set; }
    }
}
