using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTool
{
    public enum Operation
    {
        BatchIngestion,
        StreamIngestion,
        Population,
        RangeQueryRawData,
        RangeQueryAggData,
        OutOfRangeQuery,
        DifferenceAggQuery,
        STDDevQuery
    }
}
