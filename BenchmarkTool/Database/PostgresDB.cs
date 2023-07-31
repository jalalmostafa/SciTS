using Npgsql;
using PostgreSQLCopyHelper;
using Serilog;
using System;
using System.Text;

using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;

namespace BenchmarkTool.Database
{
    public class PostgresDB : IDatabase
    {
        private NpgsqlConnection _connection;
        private PostgreSQLCopyHelper<IRecord> _copyHelper;
        private IQuery<String> _query;
        private string _connectionConfig;
        private int _aggInterval;

        protected PostgresDB(IQuery<String> query, string connectionConfig)
        {
            _query = query;
            _connectionConfig = connectionConfig;
            _aggInterval = Config.GetAggregationInterval();
        }

        public PostgresDB() : this(new PostgresQuery(), Config.GetPostgresConnection())
        {
        }

        public void Cleanup()
        {
            // var command = _connection.CreateCommand();
            // command.CommandText = $"DELETE FROM {Config.GetPolyDimTableName()} WHERE {Constants.SensorID} IS NOT NULL";
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
                Log.Error(String.Format("Failed to close. Exception: {0}", ex.ToString()));
            }
        }
        static bool _TableCreated = false;
        public void Init()
        {

            try
            {
                _connection = new NpgsqlConnection(_connectionConfig);
                _connection.Open();

                if (Config.GetMultiDimensionStorageType() == "column")
                {
                    // create table


                    if (_TableCreated == false)
                    {
                        var command = _connection.CreateCommand();
                        int c = 0; StringBuilder builder = new StringBuilder("");
                        while (c < Config.GetDataDimensionsNr()) { builder.Append(", value_" + c + " double precision"); c++; }
                        command.CommandText = String.Format("CREATE TABLE IF NOT EXISTS " + Config.GetPolyDimTableName() + " ( time timestamp(6) with time zone NOT NULL, sensor_id integer " + builder + ") ; CREATE INDEX ON " + Config.GetPolyDimTableName() + " ( sensor_id, time DESC); --UNIQUE;  ");
                        _TableCreated = true;
                        command.ExecuteNonQuery();
                    }


                    _copyHelper = new PostgreSQLCopyHelper<IRecord>(Constants.SchemaName, Config.GetPolyDimTableName())
                                .MapTimeStamp(Constants.Time, x => x.Time)
                                .MapInteger(Constants.SensorID, x => x.SensorID);
                    // Console.WriteLine(Config.GetDataDimensionsNr());
                    for (var i = 0; i < Config.GetDataDimensionsNr(); i++)
                    {//TODO weird error OOB
                        int j = i;
                        _copyHelper = _copyHelper.MapReal(Constants.Value + "_" + i, x => x.ValuesArray[j]);
                        // Console.WriteLine(j);
                    }
                    //debugdummy(x,j) 


                }
                else
                {
                    _copyHelper = new PostgreSQLCopyHelper<IRecord>(Constants.SchemaName, Config.GetPolyDimTableName())
                           .MapTimeStamp(Constants.Time, x => x.Time)
                           .MapInteger(Constants.SensorID, x => x.SensorID)
                            .MapArray(Constants.Value, x => x.ValuesArray);
                }

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to initialize. Exception: {0}", ex.ToString()));
            }
        }

        public async Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            try
            {
                using var cmd = new NpgsqlCommand(_query.OutOfRange, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorID, NpgsqlTypes.NpgsqlDbType.Integer, query.SensorID);
                cmd.Parameters.AddWithValue(QueryParams.MaxVal, NpgsqlTypes.NpgsqlDbType.Double, query.MaxValue);
                cmd.Parameters.AddWithValue(QueryParams.MinVal, NpgsqlTypes.NpgsqlDbType.Double, query.MinValue);

                var points = 0;
                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Out of Range Query. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery), ex, ex.ToString());
            }
        }


        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            try
            {
                using var cmd = new NpgsqlCommand(_query.AggDifference, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.FirstSensorID, NpgsqlTypes.NpgsqlDbType.Integer, query.FirstSensorID);
                cmd.Parameters.AddWithValue(QueryParams.SecondSensorID, NpgsqlTypes.NpgsqlDbType.Integer, query.SecondSensorID);

                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                var points = 0;
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.DifferenceAggQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Difference beween agg sensor values query. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, 0, Operation.DifferenceAggQuery), ex, ex.ToString());
            }
        }


        public async Task<QueryStatusRead> RangeQueryAgg(RangeQuery query)
        {
            try
            {
                Log.Information("Start date: {0}, end date: {1}, sensors: {2}", query.StartDate, query.EndDate, query.SensorFilter);
                using var cmd = new NpgsqlCommand(_query.RangeAgg, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);

                var points = 0;
                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Aggregated Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {
            try
            {
                Log.Information(String.Format("Start Date: {0}", query.StartDate.ToString()));
                Log.Information(String.Format("End Date: {0}", query.EndDate.ToString()));

                using var cmd = new NpgsqlCommand(_query.RangeRaw, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);
                var points = 0;
                cmd.Prepare();
                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }
        public async Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery query)
        {
            try
            {
                Log.Information(String.Format("Start Date: {0}", query.StartDate.ToString()));
                Log.Information(String.Format("End Date: {0}", query.EndDate.ToString()));

                using var cmd = new NpgsqlCommand(_query.RangeRawAllDims, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);
                var points = 0;
                cmd.Prepare();
                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            try
            {
                using var cmd = new NpgsqlCommand(_query.StdDev, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorID, NpgsqlTypes.NpgsqlDbType.Integer, query.SensorID);

                Stopwatch sw = Stopwatch.StartNew();
                using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                var points = 0;
                while (reader.Read())
                {
                    points++;
                }
                sw.Stop();
                await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute STD Dev query. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            try
            {
                var stmt = $"INSERT INTO {Config.GetPolyDimTableName()} ({Constants.SensorID}, {Constants.Value}, {Constants.Time}) VALUES ({record.SensorID}, {record.ValuesArray}, {record.Time})";
                await using (var cmd = new NpgsqlCommand(stmt, _connection))
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    await cmd.ExecuteNonQueryAsync();
                    sw.Stop();

                    return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.StreamIngestion));
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.StreamIngestion), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                await _copyHelper.SaveAllAsync(_connection, batch.Records);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }

        public async Task Print(object result, string query, bool enabled)
        {
            if (enabled == true)
                while (((NpgsqlDataReader)result).Read())
                {
                    await Console.Out.WriteLineAsync(" read | " + result.ToString() + " at " + result.ToString() + "in: " + result.ToString() + "from Query:| " + query);
                }
        }
    }
}
