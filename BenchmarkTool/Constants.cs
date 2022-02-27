namespace BenchmarkTool
{
    public static class Constants
    {
        public const string TableName = "sensor_data";
        public const string SchemaName = "public";

        public const string SensorID = "sensor_id";
        public const string Value = "value";
        public const string Time = "time";

        public const string TimescaleDBClass = "TimescaleDB";
        public const string InfluxDBClass = "InfluxDB";
        public const string ClickhouseClass = "ClickhouseDB";
        public const string MySQLClass = "MySQLDB";
        public const string PostgresClass = "PostgresDB";
    }
}
