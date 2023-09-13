using System;
using System.Collections.Generic;
using System.Text;

namespace BenchmarkTool
{
    static class ConfigurationKeys
    {
        public const string GlancesStorageFileSystem = "GlancesStorageFileSystem";
        public const string PrintModeEnabled = "PrintModeEnabled";
        public const string PostgresConnection = "PostgresConnection";
        public const string DatalayertsConnection = "DatalayertsConnection";
        public const string DatalayertsUser = "DatalayertsUser";
        public const string DatalayertsPassword = "DatalayertsPassword";
        public const string DatalayertsScaleMilliseconds = "DatalayertsScaleMilliseconds";
        public const string TimescaleConnection = "TimescaleConnection";
        public const string MySQLConnection = "MySQLConnection";

        public const string TargetDatabase = "TargetDatabase";
        public const string BatchSize = "BatchSize";
        public const string ClientNumber = "ClientNumber";
        public const string SensorNumber = "SensorNumber";
        public const string DataDimensionsNr = "DataDimensionsNr"; 
        public const string DataDimensionsNrOptions = "DataDimensionsNrOptions"; 
        public const string StartTime = "StartTime";
        public const string BatchTimeOffset = "BatchTimeOffset";
        public const string GlobalTimeOffset = "GlobalTimeOffset";
        public const string MetricsCSVPath = "MetricsCSVPath";
        public const string TestRetries = "TestRetries";
        public const string InfluxDBHost = "InfluxDBHost";
        public const string InfluxDBToken = "InfluxDBToken";
        public const string InfluxDBBucket = "InfluxDBBucket";
        public const string InfluxDBOrganization = "InfluxDBOrganization";
        public const string ClickhouseHost = "ClickhouseHost";
        public const string ClickhousePort = "ClickhousePort";
        public const string ClickhouseUser = "ClickhouseUser";
        public const string ClickhouseDatabase = "ClickhouseDatabase";
        public const string VictoriametricsDBHost = "VictoriametricsDBHost";
        public const string VictoriametricsDBToken = "VictoriametricsDBToken";
        public const string VictoriametricsDBBucket = "VictoriametricsDBBucket";
        public const string VictoriametricsDBOrganization = "VictoriametricsDBOrganization";

        public const string IngestionType = "IngestionType";
        public const string MultiDimensionStorageType = "MultiDimensionStorageType";
        public const string QueryType = "QueryType";
        public const string TimescaleDBRangeQuery = "TimescaleDBRangeQuery";
        public const string AggregationFunction = "AggregationFunction";
        public const string AggregationInterval = "AggregationIntervalHour";
        public const string DurationMinutes = "DurationMinutes";
        public const string SensorsFilter = "SensorsFilter";
        public const string SensorID = "SensorID";
        public const string MaxValue = "MaxValue";
        public const string MinValue = "MinValue";
        public const string FirstSensorID = "FirstSensorID";
        public const string SecondSensorID = "SecondSensorID";
        public const string ClientNumberOptions = "ClientNumberOptions";
        public const string BatchSizeOptions = "BatchSizeOptions";
        public const string DaySpan = "DaySpan";
        public const string HourSpan = "HourSpan";
        public const string GlancesUrl = "GlancesUrl";
        public const string GlancesDatabasePid = "GlancesDatabasePid";
        public const string GlancesPeriod = "GlancesPeriod";
        public const string GlancesOutput = "GlancesOutput";
        public const string GlancesDisk = "GlancesDisk";
        public const string GlancesNIC = "GlancesNIC";
    }
}
