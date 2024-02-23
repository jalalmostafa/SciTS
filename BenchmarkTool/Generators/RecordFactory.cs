using System;

namespace BenchmarkTool.Generators
{
    class RecordFactory
    {
        string database;

        public RecordFactory()
        {
            database = Config.GetTargetDatabase();
        }

        public IRecord Create(int sensorId, DateTime timestamp, float value)
        {
            return database switch
            {
                Constants.TimescaleDBClass => new RecordTimescaleDB(sensorId, timestamp, value),
                Constants.InfluxDBClass => new RecordInflux(sensorId, timestamp, value),
                Constants.ClickhouseClass => new RecordClickhouse(sensorId, timestamp, value),
                Constants.MySQLClass => new RecordMySQLDB(sensorId, timestamp, value),
                Constants.PostgresClass => new RecordTimescaleDB(sensorId, timestamp, value),
                Constants.RedisTimeSeriesClass => new RecordRedisTimeSeries(sensorId, timestamp, value),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
