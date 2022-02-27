using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class Batch
    {
        public Batch (){
            Records = new List<IRecord>();
        }

        public Batch(int size)
        {
            Size = size;
            Records = new List<IRecord>();
        }
        public List<IRecord> Records { get; set; }
        public int Size { get; set; }
    }
}
