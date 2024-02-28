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

        public IDatabase Create(int clientsNumber, int sensorsNumber, int batchSize)
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
                case Constants.RedisTimeSeriesClass:
                    return new RedisTimeSeriesDB(clientsNumber, sensorsNumber, batchSize);
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

