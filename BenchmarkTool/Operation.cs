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
        RangeQueryRawAllDimsData,
                RangeQueryRawLimitedData,
        RangeQueryRawAllDimsLimitedData,
        RangeQueryAggData,
        OutOfRangeQuery,
        DifferenceAggQuery,
        STDDevQuery
    }
}
