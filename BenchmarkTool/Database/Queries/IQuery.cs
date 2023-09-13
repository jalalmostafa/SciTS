using BenchmarkTool.Queries;

namespace BenchmarkTool.Database.Queries
{
    public interface IQuery<T>
    {
        T RangeAgg { get; }

        T RangeRaw { get; }
         T RangeRawLimited { get; }

        T RangeRawAllDims { get; }
        T RangeRawAllDimsLimited { get; }

        T OutOfRange { get; }

        T StdDev { get; }

        T AggDifference { get; }

    }
}