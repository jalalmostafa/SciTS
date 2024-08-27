using Serilog;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using NRedisStack.Literals.Enums;
using SERedis = StackExchange.Redis;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Linq;
using System.Collections.Immutable;
using NRedisStack.DataTypes;
using StackExchange.Redis;

namespace BenchmarkTool.Database
{
    public class RedisTimeSeriesDB : IDatabase
    {
        private static volatile bool _initialized = false;

        private static ConnectionMultiplexer _connection;
        private SERedis.IDatabase _redisDB;
        private TimeSeriesCommands _redists;

        private int _aggInterval;
        private int _clientsNumber;
        private int _sensorsNumber;
        private int _batchSize;

        public RedisTimeSeriesDB(int clientsNumber, int sensorsNumber, int batchSize)
        {
            _clientsNumber = clientsNumber;
            _sensorsNumber = sensorsNumber;
            _batchSize = batchSize;
        }

        public void Cleanup()
        {
        }

        public void Close()
        {
            try
            {
                if (_connection != null)
                {
                    _connection.Dispose();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to close RedisTimeSeries. Exception: {0}", ex.ToString()));
            }
        }

        public void Init()
        {
            try
            {
                if (_connection == null)
                {
                    var options = new ConfigurationOptions()
                    {
                        SocketManager = new SocketManager("test", _clientsNumber),
                        EndPoints = { { Config.GetRedisHost(), Config.GetRedisPort() } },
                    };
                    _connection = ConnectionMultiplexer.Connect(options);
                }
                _redisDB = _connection.GetDatabase();
                _redists = _redisDB.TS();
                if (!_initialized)
                {
                    Console.WriteLine($"Warming up RedisTimeSeries for {Config.GetSensorNumber()} sensors...");
                    foreach (var item in Enumerable.Range(0, Config.GetSensorNumber()).Select(i => i.ToString()))
                    {
                        if (!_redisDB.KeyExists(item))
                            _redists.Create(item, labels: [new TimeSeriesLabel("id", item)], duplicatePolicy: TsDuplicatePolicy.LAST);
                    }
                    _initialized = true;
                    Console.WriteLine("Finished Warming up!");
                }
                _aggInterval = Config.GetAggregationInterval();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to initialize Redis. Exception: {0}", ex.ToString()));
            }
        }

        public QueryStatusRead OutOfRangeQuery(OORangeQuery query)
        {
            try
            {
                // TODO: make them async
                Stopwatch sw = Stopwatch.StartNew();
                var rangeOutput = _redists.Range(query.SensorID.ToString(),
                                                new TimeStamp(query.StartDate),
                                                new TimeStamp(query.EndDate),
                                                filterByValue: ((long)query.MinValue, (long)query.MaxValue));
                sw.Stop();
                int points = rangeOutput.Count;

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to execute Range Query Raw Data on RedisTimeSeries. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }

        public QueryStatusRead RangeQueryAgg(RangeQuery query)
        {
            // int points = 0;
            // try
            // {
            //     string sql = _query.RangeAgg;
            //     sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.SensorIDsParam, query.SensorFilter);

            //     Log.Information(sql);
            //     var cmd = _connection.CreateCommand();
            //     cmd.CommandText = sql;

            //     Stopwatch sw = Stopwatch.StartNew();
            //     var reader = cmd.ExecuteReader();
            //     reader.ReadAll(rowReader => { points++; });
            //     sw.Stop();

            //     return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            // }
            // catch (Exception ex)
            // {
            //     Log.Error(String.Format("Failed to execute Range Query Agg Data on ClickhouseDB. Exception: {0}", ex.ToString()));
            //     return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            // }
            throw new NotImplementedException();

        }

        public QueryStatusRead RangeQueryRaw(RangeQuery query)
        {
            try
            {
                // TODO: make them async
                Stopwatch sw = Stopwatch.StartNew();
                var rangeOutput = _redists.MRange(new TimeStamp(query.StartDate),
                                                new TimeStamp(query.EndDate),
                                                [$"id=({query.SensorFilter})"]);
                sw.Stop();
                int points = rangeOutput.Select(i => i.values.Count).Sum();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to execute Range Query Raw Data on RedisTimeSeries. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }

        public QueryStatusRead AggregatedDifferenceQuery(ComparisonQuery query)
        {
            // int points = 0;
            // try
            // {
            //     string sql = _query.AggDifference;
            //     sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.FirstSensorIDParam, query.FirstSensorID.ToString());
            //     sql = sql.Replace(QueryParams.SecondSensorIDParam, query.SecondSensorID.ToString());

            //     Log.Information(sql);
            //     var cmd = _connection.CreateCommand();
            //     cmd.CommandText = sql;

            //     Stopwatch sw = Stopwatch.StartNew();
            //     var reader = cmd.ExecuteReader();
            //     reader.ReadAll(rowReader => { points++; });
            //     sw.Stop();
            //     return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery));
            // }
            // catch (Exception ex)
            // {
            //     Log.Error(String.Format("Failed to execute Difference beween agg sensor values query on ClickhouseDB. Exception: {0}", ex.ToString()));
            //     return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery), ex, ex.ToString());
            // }
            throw new NotImplementedException();
        }

        public QueryStatusRead StandardDevQuery(SpecificQuery query)
        {
            // int points = 0;
            // try
            // {
            //     string sql = _query.StdDev;
            //     sql = sql.Replace(QueryParams.StartParam, query.StartDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.EndParam, query.EndDate.ToString("yyyy-MM-dd HH:mm:ss"));
            //     sql = sql.Replace(QueryParams.SensorIDParam, query.SensorID.ToString());
            //     Log.Information(sql);
            //     var cmd = _connection.CreateCommand();
            //     cmd.CommandText = sql;

            //     Stopwatch sw = Stopwatch.StartNew();
            //     var reader = cmd.ExecuteReader();
            //     reader.ReadAll(rowReader => { points++; });
            //     sw.Stop();
            //     return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
            // }
            // catch (Exception ex)
            // {
            //     Log.Error(String.Format("Failed to execute STD Dev query on ClickhouseDB. Exception: {0}", ex.ToString()));
            //     return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, points, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery), ex, ex.ToString());
            // }
            throw new NotImplementedException();

        }

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                var list = batch.Records.Select(i => (i.SensorID.ToString(), new TimeStamp(i.Time), (double)i.Value)).ToImmutableList();
                Stopwatch sw = Stopwatch.StartNew();
                await _redists.MAddAsync(list);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to insert batch into RedisTimeSeries. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteRecord(IRecord i)
        {
            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                var command = await _redists.AddAsync(i.SensorID.ToString(), new TimeStamp(i.Time), (double)i.Value);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Failed to insert batch into RedisTimeSeries. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.BatchIngestion), ex, ex.ToString());
            }
        }
    }
}
