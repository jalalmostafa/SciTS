using System;

namespace BenchmarkTool
{
    public class QueryStatusWrite : QueryStatus<PerformanceMetricWrite, LogRecordWrite>
    {
        public int Client { get; set; }
        

        public QueryStatusWrite(bool succeeded,
                                 PerformanceMetricWrite metric)
                                 : base(succeeded, metric)
        {
        }

        public QueryStatusWrite(bool succeeded, int dataPoints,
                                PerformanceMetricWrite metric)
                                : base(succeeded, dataPoints, metric)
        {
        }

        public QueryStatusWrite(bool succeeded, int dataPoints,
                                PerformanceMetricWrite metric,
                                Exception exception, string errorMessage)
                : base(succeeded, dataPoints, metric, exception, errorMessage)
        {
        }
    }
}