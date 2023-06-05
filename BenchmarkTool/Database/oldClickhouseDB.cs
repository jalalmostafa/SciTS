using ClickHouse.Ado;
using Serilog;
using System;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;

namespace BenchmarkTool.Database
{
    public class oldClickhouseDB : IDatabase
    {
        private ClickHouseConnection _connection;
        private IQuery<String> _query;
        private int _aggInterval;

        public void Cleanup()
        {
            // var command = _connection.CreateCommand();
            // command.CommandText = $"ALTER TABLE {Constants.TableName} DELETE WHERE {Constants.SensorID} IS NOT NULL";
            // command.ExecuteNonQuery();
        }

        public void Close()
        {
            try
            {
                if (_connection != null)
                    _connection.Close();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to close Clickhouse. Exception: {0}", ex.ToString()));
            }
        }

        public void Init()
        {
            try
            {
                var settings = new ClickHouseConnectionSettings()
                {
                    Host = Config.GetClickhouseHost(),
                    Port = Config.GetClickhousePort(),
                    Database = Config.GetClickhouseDatabase(),
                    User = Config.GetClickhouseUser(),
                    SocketTimeout = 5000
                };
                _connection = new ClickHouseConnection(settings);
                _connection.Open();
                _query = new ClickhouseQuery();
                _aggInterval = Config.GetAggregationInterval();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to initialize Clickhouse. Exception: {0}", ex.ToString()));
            }
        }

        public async Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.OutOfRange;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDParam, query.SensorID.ToString());
                sql = sql.Replace(QueryParams.MinValParam, query.MinValue.ToString());
                sql = sql.Replace(QueryParams.MaxValParam, query.MaxValue.ToString());

                Log.Information(sql);
                var cmd = _connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });
                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Out of Range Query on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryAgg(RangeQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.RangeAgg;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);

                Log.Information(sql);
                var cmd = _connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });
                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Agg Data on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.RangeRaw;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);
                Log.Information(sql);
                var cmd = _connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });
                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.AggDifference;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.FirstSensorIDParam, query.FirstSensorID.ToString());
                sql = sql.Replace(QueryParams.SecondSensorIDParam, query.SecondSensorID.ToString());

                Log.Information(sql);
                var cmd = _connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });
                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Difference beween agg sensor values query on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.StdDev;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDParam, query.SensorID.ToString());
                Log.Information(sql);
                var cmd = _connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });
                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute STD Dev query on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        public Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                var command = _connection.CreateCommand();

                command.CommandText = String.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES @bulk", Config.GetPolyDimTableName(), Constants.SensorID, Constants.Value, Constants.Time);
                command.Parameters.Add(new ClickHouseParameter
                {
                    ParameterName = "bulk",
                    Value = batch.Records
                });

                Stopwatch sw = Stopwatch.StartNew();
                command.ExecuteNonQuery();
                sw.Stop();
                return Task.FromResult(new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion)));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into Clickhouse. Exception: {0}", ex.ToString()));
                return Task.FromResult(new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString()));
            }
        }

        public Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            try
            {
                var command = _connection.CreateCommand();

                command.CommandText = String.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES @record", Constants.TableName, Constants.SensorID, Constants.Value, Constants.Time);
                command.Parameters.Add(new ClickHouseParameter
                {
                    ParameterName = "record",
                    Value = record
                });

                Stopwatch sw = Stopwatch.StartNew();
                command.ExecuteNonQuery();
                sw.Stop();
                return Task.FromResult(new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.StreamIngestion)));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into Clickhouse. Exception: {0}", ex.ToString()));
                return Task.FromResult(new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.StreamIngestion), ex, ex.ToString()));
            }
        }
    }
}
