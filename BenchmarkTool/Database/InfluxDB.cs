using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Serilog;
using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Numerics;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;
using System.Collections.Generic;
using BenchmarkTool;

namespace BenchmarkTool.Database
{
    public class InfluxDB : IDatabase
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private InfluxDBClientOptions _options;
        private InfluxDBClient _client;
        private WriteApiAsync _writeApi;
        private IQuery<String> _influxQueries;
        private int _aggInterval;

        public void Cleanup()
        {
            // var org = Config.GetInfluxOrganization();
            // var orgId = _client.GetOrganizationsApi().FindOrganizationsAsync(org: org).GetAwaiter().GetResult().First().Id;
            // _client.GetBucketsApi().DeleteBucketAsync(Config.GetInfluxBucket()).GetAwaiter().GetResult();
            // _client.GetBucketsApi().CreateBucketAsync(Config.GetInfluxBucket(), orgId).GetAwaiter().GetResult();
        }

        public void Close()
        {
            try
            {
                if (_client != null)
                {
                    _client.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to close InfluxDB. Exception: {0}", ex.ToString()));
            }
        }

        public void Init()
        {
            try
            {
                var t = new TimeSpan(0, 0, 10, 0);
                _options = new InfluxDBClientOptions.Builder()
                    .Url(Config.GetInfluxHost())
                    .AuthenticateToken(Config.GetInfluxToken().ToCharArray())
                    .Org(Config.GetInfluxOrganization())
                    .Bucket(Config.GetInfluxBucket())
                    .TimeOut(t)
                    .Build();
                _client = new InfluxDBClient(_options);

                _writeApi = _client.GetWriteApiAsync();

                _influxQueries = new InfluxQuery();
                _aggInterval = Config.GetAggregationInterval();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to initialize InfluxDB. Exception: {0}", ex.ToString()));
            }
        }

        public  async Task<QueryStatusRead>  OutOfRangeQuery(OORangeQuery query)
        {
            try
            {
                var flux = _influxQueries.OutOfRange;
                flux = flux.Replace(QueryParams.StartParam, query.StartDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.EndParam, query.EndDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.SensorIDParam, query.SensorID.ToString());
                flux = flux.Replace(QueryParams.MaxValParam, query.MaxValue.ToString());
                flux = flux.Replace(QueryParams.MinValParam, query.MinValue.ToString());
                // Log.Information(String.Format("Flux query: {0}", flux));

                var queryApi = _client.GetQueryApi();
                Stopwatch sw = Stopwatch.StartNew();
                var results = await queryApi.QueryAsync(flux, Config.GetInfluxOrganization());
                sw.Stop();
                Log.Information(String.Format("Number of point: {0}", results[0].Records.Count.ToString()));
                int count = results != null && results.Count > 0 ? results[0].Records.Count : 0;
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Out of Range Query on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 1, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryAgg(RangeQuery query)
        {
            try
            {
                var flux = _influxQueries.RangeAgg;
                flux = flux.Replace(QueryParams.StartParam, query.StartDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.EndParam, query.EndDate.ToUniversalTime().ToString("o"));
                var sensorIds = query.SensorIDs.Select(x => String.Concat("^", x, "$")).ToList();
                var ids = String.Concat("/", String.Join("|", sensorIds), "/");
                flux = flux.Replace(QueryParams.SensorIDsParam, ids);
                Log.Information(String.Format("Flux query: {0}", flux));

                var queryApi = _client.GetQueryApi();
                Stopwatch sw = Stopwatch.StartNew();
                var results = await queryApi.QueryAsync(flux, Config.GetInfluxOrganization());
                sw.Stop();
                Log.Information(String.Format("Number of point: {0}", results[0].Records.Count.ToString()));
                int count = results != null && results.Count > 0 ? results[0].Records.Count : 0;
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Aggregated Data on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {
            try
            {
                var flux = _influxQueries.RangeRaw.Replace(QueryParams.StartParam, query.StartDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.EndParam, query.EndDate.ToUniversalTime().ToString("o"));
                var sensorIds = query.SensorIDs.Select(x => String.Concat("^", x, "$")).ToList();
                var ids = String.Concat("/", String.Join("|", sensorIds), "/");
                flux = flux.Replace(QueryParams.SensorIDsParam, ids);
                Log.Information("Flux query: " + flux);

                var queryApi = _client.GetQueryApi();
                Stopwatch sw = Stopwatch.StartNew();
                var results = await queryApi.QueryAsync(flux, Config.GetInfluxOrganization());
                sw.Stop();
                int count = results != null && results.Count > 0 ? results[0].Records.Count : 0;
                Log.Information("Number of points: " + count.ToString());
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0,
                                           query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            try
            {
                var flux = _influxQueries.AggDifference.Replace(QueryParams.StartParam, query.StartDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.EndParam, query.EndDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.FirstSensorIDParam, query.FirstSensorID.ToString());
                flux = flux.Replace(QueryParams.SecondSensorIDParam, query.SecondSensorID.ToString());
                Log.Information(String.Format("Flux query: {0}", flux));

                var queryApi = _client.GetQueryApi();
                Stopwatch sw = Stopwatch.StartNew();
                var results = await queryApi.QueryAsync(flux, Config.GetInfluxOrganization());
                sw.Stop();
                Log.Information(String.Format("Number of point: {0}", results[0].Records.Count.ToString()));
                int count = results != null && results.Count > 0 ? results[0].Records.Count : 0;
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Difference beween agg sensor values query on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                var lineData = new List<string>(batch.Size);
                foreach (var item in batch.Records)
                {
                    var timeSpan = item.Time.Subtract(EpochStart);
                    var time = TimeSpanToBigInteger(timeSpan, WritePrecision.Ns);

                    if(Config.GetMultiDimensionStorageType() == "column"){
                        int c = 1 ; StringBuilder builder = new StringBuilder("");
                        while(c < Config.GetDataDimensionsNr()) { builder.Append("value={"+item.ValuesArray[(c)]+"}"); c++; }
                        lineData.Add($"{Constants.TableName},sensor_id={item.SensorID} value={item.ValuesArray[0]} "+ builder + "{time}");
                    }
                    else
                        lineData.Add($"{Constants.TableName},sensor_id={item.SensorID} value={item.ValuesArray} {time}");
                }

                Stopwatch sw = Stopwatch.StartNew();
                await _writeApi.WriteRecordsAsync( lineData,  WritePrecision.Ns );
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            try
            {
                 var lineData = new List<string>();
                var timeSpan = record.Time.Subtract(EpochStart);
                var time = TimeSpanToBigInteger(timeSpan, WritePrecision.Ns);
                  lineData.Add($"{Constants.TableName},sensor_id={record.SensorID} value={record.ValuesArray} {time}");

                Stopwatch sw = Stopwatch.StartNew();
                await _writeApi.WriteRecordsAsync( lineData, WritePrecision.Ns );
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.StreamIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.StreamIngestion), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            try
            {
                var flux = _influxQueries.StdDev.Replace(QueryParams.StartParam, query.StartDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.EndParam, query.EndDate.ToUniversalTime().ToString("o"));
                flux = flux.Replace(QueryParams.SensorIDParam, query.SensorID.ToString());
                Log.Information(String.Format("Flux query: {0}", flux));

                var queryApi = _client.GetQueryApi();
                Stopwatch sw = Stopwatch.StartNew();
                var results = await queryApi.QueryAsync(flux, Config.GetInfluxOrganization());
                sw.Stop();

                int count = results != null && results.Count > 0 ? results[0].Records.Count : 0;
                Log.Information("Number of points: " + count.ToString());
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute STD Dev query on Influxdb. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        private static BigInteger TimeSpanToBigInteger(TimeSpan timestamp, WritePrecision timeUnit)
        {
            BigInteger time;
            switch (timeUnit)
            {
                case WritePrecision.Ns:
                    time = timestamp.Ticks * 100;
                    break;
                case WritePrecision.Us:
                    time = (BigInteger)(timestamp.Ticks * 0.1);
                    break;
                case WritePrecision.Ms:
                    time = (BigInteger)timestamp.TotalMilliseconds;
                    break;
                case WritePrecision.S:
                    time = (BigInteger)timestamp.TotalSeconds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit), timeUnit, "WritePrecision value is not supported");
            }

            return time;
        }
    }
}


