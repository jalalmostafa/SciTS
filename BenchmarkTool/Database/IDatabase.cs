using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;

namespace BenchmarkTool.Database
{
    public interface IDatabase
    {
        void Init();

        void Cleanup();

        void Close();

        Task<QueryStatusWrite> WriteBatch(Batch batch);

        Task<QueryStatusWrite> WriteRecord(IRecord record);

        QueryStatusRead RangeQueryRaw(RangeQuery rangeQuery);

        QueryStatusRead RangeQueryAgg(RangeQuery rangeQuery);

        QueryStatusRead OutOfRangeQuery(OORangeQuery query);

        QueryStatusRead AggregatedDifferenceQuery(ComparisonQuery query);

        QueryStatusRead StandardDevQuery(SpecificQuery query);

    }
}
