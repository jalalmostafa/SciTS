namespace BenchmarkTool.Database.Queries
{
    public interface IQuery
    {
        string RangeAgg { get; }

        string RangeRaw { get; }

        string OutOfRange { get; }

        string StdDev { get; }

        string AggDifference { get; }

    }
}