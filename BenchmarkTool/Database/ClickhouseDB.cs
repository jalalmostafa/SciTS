using ClickHouse.Ado;
using ClickHouse.Client.ADO;
using ClickHouse.Client.Utility;
using Serilog;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;
using System.Globalization;

// TODO fix async

namespace BenchmarkTool.Database
{
    public class ClickhouseDB : IDatabase
    {
        private ClickHouse.Client.ADO.ClickHouseConnection _read_connection;
        private ClickHouse.Ado.ClickHouseConnection _write_connection;

        private IQuery<String> _query;
        private int _aggInterval;
        private static bool _TableCreated;


        public void Cleanup()
        {
            // var command = _connection.CreateCommand();
            // command.CommandText = $"ALTER TABLE { Config.GetPolyDimTableName() } DELETE WHERE {Constants.SensorID} IS NOT NULL";
            // command.ExecuteNonQuery();
        }

        public void Close()
        {
            try
            {
                if (_read_connection != null)
                    _read_connection.Close();
                if (_write_connection != null)
                    _write_connection.Close();

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
                // TODO if both connections work properly async ,  kill one of them



                _read_connection = new ClickHouse.Client.ADO.ClickHouseConnection("Host=" + Config.GetClickhouseHost() + ";Protocol=https;Port=" + Config.GetClickhousePort() + ";Username=" + Config.GetClickhouseUser());
                _read_connection.ChangeDatabase(Config.GetClickhouseDatabase());
                _read_connection.OpenAsync();


                var write_settings = new ClickHouse.Ado.ClickHouseConnectionSettings()
                {
                    Host = Config.GetClickhouseHost(),
                    Port = Config.GetClickhousePort(),
                    Database = Config.GetClickhouseDatabase(),
                    User = Config.GetClickhouseUser(),
                    SocketTimeout = 15000,
                    Async = true,
                };

                _write_connection = new ClickHouse.Ado.ClickHouseConnection(write_settings);
                _write_connection.Open();

                _query = new ClickhouseQuery();
                _aggInterval = Config.GetAggregationInterval();

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to initialize Clickhouse. Exception: {0}", ex.ToString()));
            }
        }



        public void CheckOrCreateTable()
        {
            try
            {
                var dimNb = 0;
                if (_TableCreated != true)
                {

                    var commandDB = _write_connection.CreateCommand();
                    commandDB.CommandText = String.Format("CREATE DATABASE IF NOT EXISTS " + Config.GetClickhouseDatabase() + ";");
                    commandDB.ExecuteNonQuery();

                    if (Config.GetMultiDimensionStorageType() == "column")
                    {
                        foreach (var tableName in Config.GetAllPolyDimTableNames())
                        {
                            var actualDim = Config.GetDataDimensionsNrOptions()[dimNb];


                            // create table
                            var command = _write_connection.CreateCommand();
                            int c = 0; StringBuilder builder = new StringBuilder("");

                            while (c < actualDim) { builder.Append(", value_" + c + " Float64"); c++; }

                            command.CommandText = String.Format("CREATE TABLE IF NOT EXISTS " + tableName + " ( time DateTime64(9) , sensor_id Int32 " + builder + ") ENGINE = MergeTree() PARTITION BY toYYYYMMDD(time) ORDER BY (sensor_id, time);");

                            command.ExecuteNonQuery();
                            _TableCreated = true;
                            dimNb++;
                        }
                    }
                    else
                        throw new NotImplementedException();



                }




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
                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                // while (reader.Read())
                // {
                //     points++;
                // }
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
                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                // while (reader.Read())
                // {
                //     points++;
                // }
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







                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                // var reader = await cmd.ExecuteReaderAsync();       TODO delete if delete read_conn        
                // while (reader.Read())
                // {
                //     points++;
                // }
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

        public async Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery query)
        {
            int points = 0;
            try
            {
                string sql = _query.RangeRawAllDims;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);
                Log.Information(sql);







                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                // var reader = await cmd.ExecuteReaderAsync();       TODO delete if delete read_conn        
                // while (reader.Read())
                // {
                //     points++;
                // }
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });

                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery query, int limit)
        {
            int points = 0;
            try
            {
                string sql = _query.RangeRawLimited;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);
                Log.Information(sql);



                sql = sql.Replace(QueryParams.Limit, limit.ToString());


                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                // var reader = await cmd.ExecuteReaderAsync();       TODO delete if delete read_conn        
                // while (reader.Read())
                // {
                //     points++;
                // }
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

        public async Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery query, int limit)
        {
            int points = 0;
            try
            {
                string sql = _query.RangeRawAllDimsLimited;
                sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
                sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);
                Log.Information(sql);



                sql = sql.Replace(QueryParams.Limit, limit.ToString());


                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                // var reader = await cmd.ExecuteReaderAsync();       TODO delete if delete read_conn        
                // while (reader.Read())
                // {
                //     points++;
                // }
                var reader = cmd.ExecuteReader();
                reader.ReadAll(rowReader => { points++; });

                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on ClickhouseDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
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
                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                // while (reader.Read())
                // {
                //     points++;
                // }
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
                var cmd = _write_connection.CreateCommand();
                cmd.CommandText = sql;

                Stopwatch sw = Stopwatch.StartNew();
                var reader = cmd.ExecuteReader();
                // while (reader.Read())
                // {
                //     points++;
                // }
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

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                var command = _write_connection.CreateCommand();
                int c = 0; StringBuilder builderKey = new StringBuilder("");
                List<object> builderVal = new List<object>(); builderVal.Add(Config.GetPolyDimTableName()); builderVal.Add(Constants.Time); builderVal.Add(Constants.SensorID);
                while (c < Config.GetDataDimensionsNr()) { builderKey.Append(", {" + (3 + c) + "} "); builderVal.Add(Constants.Value + "_" + c); c++; }

                command.CommandText = String.Format("INSERT INTO {0} ({1}, {2}" + builderKey + " ) VALUES @bulk", builderVal.ToArray());
                command.Parameters.Add(new ClickHouseParameter
                {
                    ParameterName = "bulk",
                    Value = batch.Records.AsEnumerable()
                });

                Stopwatch sw = Stopwatch.StartNew();
                command.ExecuteNonQuery();
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into Clickhouse. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            try
            {
                var command = _write_connection.CreateCommand();

                command.CommandText = String.Format("INSERT INTO {0} ({1}, {2}, {3}) VALUES @record", Config.GetPolyDimTableName(), Constants.SensorID, Constants.Value, Constants.Time);
                command.Parameters.Add(new ClickHouseParameter
                {
                    ParameterName = "record",
                    Value = record
                });

                Stopwatch sw = Stopwatch.StartNew();
                command.ExecuteNonQuery();
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.StreamIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into Clickhouse. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.StreamIngestion), ex, ex.ToString());
            }
        }
    }
}
