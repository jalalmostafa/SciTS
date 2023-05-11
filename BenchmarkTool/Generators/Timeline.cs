using System.Collections.Generic;

namespace BenchmarkTool.Generators
{
    public class Timeline
    {
        public Timeline (){
            Records = new List<IRecord>();
        }

        public Timeline(int size)
        {
            Size = size;
            Records = new List<IRecord>();
        }
        public List<IRecord> Records { get; set; }
        public int Size { get; set; }
    }
}
