using System;

namespace BenchmarkTool
{
    public class QueryStatusRead : QueryStatus<PerformanceMetricRead, LogRecordRead>
    {
        public QueryStatusRead(bool succeeded, PerformanceMetricRead metric) : base(succeeded, metric)
        {
        }

        public QueryStatusRead(bool succeeded, int dataPoints, PerformanceMetricRead metric) : base(succeeded, dataPoints, metric)
        {
        }

        public QueryStatusRead(bool succeeded, int dataPoints, PerformanceMetricRead metric, Exception exception, string errorMessage) : base(succeeded, dataPoints, metric, exception, errorMessage)
        {
        }
    }
}