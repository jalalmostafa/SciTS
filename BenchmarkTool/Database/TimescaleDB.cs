using BenchmarkTool.Database.Queries;

namespace BenchmarkTool.Database
{
    public class TimescaleDB : PostgresDB
    {
        public TimescaleDB() : base(new TimescaleQuery(),
                                    Config.GetTimescaleConnection())
        { }
    }
}
