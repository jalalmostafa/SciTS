using System;

namespace BenchmarkTool.Database
{
    class DatabaseFactory
    {

        private string _database;

        public DatabaseFactory()
        {
            _database = Config.GetTargetDatabase();
        }

        public IDatabase Create()
        {
            switch (_database)
            {
                case Constants.DatalayertsDBClass:
                    return new DatalayertsDB();
                case Constants.TimescaleDBClass:
                    return new TimescaleDB();
                case Constants.InfluxDBClass:
                    return new InfluxDB();
                case Constants.ClickhouseClass:
                    return new ClickhouseDB();
                case Constants.PostgresClass:
                    return new PostgresDB();
                case Constants.DummyClass:
                    return new DummyDB();
                case Constants.MySQLClass:
                    return new MySQLDB();
                case Constants.VictoriametricsDBClass:
                    return new VictoriametricsDB();
                case Constants.RedisTimeSeriesClass:
                    return new RedisTimeSeriesDB();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

