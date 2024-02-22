using System;
using BenchmarkTool;

namespace BenchmarkTool
{
    public abstract class QueryStatus<T, E> where T : PerformanceMetricBase<E>
    {
        private Exception _thrownException;
        private string _errorMessage;
        public bool Succeeded { get; }
        public int DataPoints { get; }
        public int Iteration { get; set; }     
        public DateTime StartDate { get; set; }     
        public T PerformanceMetric { get; }
        public long Timestamp { get; }

        public QueryStatus(bool succeeded, T metric)
        {
            Succeeded = succeeded;
            PerformanceMetric = metric;
            Timestamp = Helper.GetMilliEpoch();        
            }

        public QueryStatus(bool succeeded, int dataPoints, T metric) : this(succeeded, metric)
        {
            DataPoints = dataPoints;
        }

        public QueryStatus(bool succeeded, int dataPoints, T metric,
                            Exception exception, string errorMessage)
                            : this(succeeded, dataPoints, metric)
        {
            _errorMessage = errorMessage;
            _thrownException = exception;
        }
    }
}
