using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;

namespace BenchmarkTool.Database
{
    public interface IDatabase
    {
        void Init();
        
        static bool _TableCreated;

        void CheckOrCreateTable();

        void Cleanup();

        void Close();

        Task<QueryStatusWrite> WriteBatch(Batch batch);
        Task<QueryStatusWrite> WriteRecord(IRecord record);
        Task<QueryStatusRead> RangeQueryRaw(RangeQuery rangeQuery );  
        Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery rangeQuery );  
        Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery rangeQuery, int limit); 
        Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery rangeQuery, int limit); 
        Task<QueryStatusRead> RangeQueryAgg(RangeQuery rangeQuery);
        Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query);
        Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query);
        Task<QueryStatusRead> StandardDevQuery(SpecificQuery query);

    }
}
