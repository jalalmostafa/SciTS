using BenchmarkTool.Queries;

namespace BenchmarkTool.Database.Queries
{
    public interface IQuery<T>
    {
        T RangeAgg { get; }

        T RangeRaw { get; }

        T RangeRawAllDims { get; }

        T OutOfRange { get; }

        T StdDev { get; }

        T AggDifference { get; }

    }
}