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
                case Constants.TimescaleDBClass:
                    return new TimescaleDB();
                case Constants.InfluxDBClass:
                    return new InfluxDB();
                case Constants.ClickhouseClass:
                    return new ClickhouseDB();
                case Constants.PostgresClass:
                    return new PostgresDB();
                case Constants.MySQLClass:
                    return new MySQLDB();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

