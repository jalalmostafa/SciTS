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
                Constants.DatalayertsDBClass => new RecordDatalayerts(sensorId, timestamp, value),
                Constants.TimescaleDBClass => new RecordTimescaleDB(sensorId, timestamp, value),
                Constants.InfluxDBClass => new RecordInflux(sensorId, timestamp, value),
                Constants.ClickhouseClass => new RecordClickhouse(sensorId, timestamp, value),
                Constants.MySQLClass => new RecordMySQLDB(sensorId, timestamp, value),
                Constants.DummyClass => new RecordMySQLDB(sensorId, timestamp, value),
                Constants.PostgresClass => new RecordTimescaleDB(sensorId, timestamp, value),

                _ => throw new NotImplementedException(),
            };
        }
        public IRecord Create(int sensorId, DateTime timestamp, float[] values)
        {
            return database switch
            {
                Constants.DatalayertsDBClass => new RecordDatalayerts(sensorId, timestamp, values),
                Constants.TimescaleDBClass => new RecordTimescaleDB(sensorId, timestamp, values),
                Constants.InfluxDBClass => new RecordInflux(sensorId, timestamp, values),
                Constants.ClickhouseClass => new RecordClickhouse(sensorId, timestamp, values),
                Constants.MySQLClass => new RecordMySQLDB(sensorId, timestamp, values),
                Constants.DummyClass => new RecordMySQLDB(sensorId, timestamp, values),
                Constants.PostgresClass => new RecordTimescaleDB(sensorId, timestamp, values),
                Constants.VictoriametricsDBClass => new RecordVictoriametrics(sensorId, timestamp, values),


                _ => throw new NotImplementedException(),
            };
        }
    }
}
