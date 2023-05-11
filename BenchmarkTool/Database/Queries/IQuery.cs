using BenchmarkTool.Queries;

namespace BenchmarkTool.Database.Queries
{
    public interface IQuery<T> 
    {
        T RangeAgg { get; }
  
        T RangeRaw  { get; }//(RangeQuery query)

        T OutOfRange { get; }

        T StdDev { get; }

        T AggDifference { get; }

    }
}