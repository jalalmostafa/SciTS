using System;

namespace BenchmarkTool.Database.Queries
{
    public class MySQLQuery : IQuery<String>
    {
        public String RangeRaw => throw new NotImplementedException();
        public String RangeRawAllDims => throw new NotImplementedException();
        public String RangeRawLimited => throw new NotImplementedException();
        public String RangeRawAllDimsLimited => throw new NotImplementedException();
        public String RangeAgg => throw new NotImplementedException();

        public String OutOfRange => throw new NotImplementedException();

        public String StdDev => throw new NotImplementedException();

        public String AggDifference => throw new NotImplementedException();

    }
}
