using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Numerics;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;
using System.Collections.Generic;
using BenchmarkTool;
using System.Text;
using System.Text.Json;
using PromQL;
using PromQL.Vectors;
using System.Net.Http;
using System.Globalization;



using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using InfluxDB.Client.Api.Client;




namespace BenchmarkTool.Database
{
    public class VictoriametricsDB : IDatabase
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private InfluxDBClientOptions _options;
        private InfluxDBClient _client;
        private WriteApiAsync _writeApi;
        private IQuery<String> _vmQueries;
        private int _aggInterval;

        private static readonly HttpClient _httpclient = new HttpClient();

        public void Cleanup()
        {
            // var org = Config.GetInfluxOrganization();
            // var orgId = _client.GetOrganizationsApi().FindOrganizationsAsync(org: org).GetAwaiter().GetResult().First().Id;
            // _client.GetBucketsApi().DeleteBucketAsync(Config.GetInfluxBucket()).GetAwaiter().GetResult();
            // _client.GetBucketsApi().CreateBucketAsync(Config.GetInfluxBucket(), orgId).GetAwaiter().GetResult();



        }

        public void CheckOrCreateTable()
        {

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
                Log.Error(String.Format("Failed to close VictoriametricsDB. Exception: {0}", ex.ToString()));
            }
        }

        public void Init()
        {
            try
            {

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;


                var t = new TimeSpan(0, 0, 10, 0);
                _options = new InfluxDBClientOptions.Builder()
                    .Url(Config.GetVictoriametricsHost())
                    .AuthenticateToken(Config.GetVictoriametricsToken().ToCharArray())
                    .Org(Config.GetVictoriametricsOrganization())
                    .Bucket(Config.GetVictoriametricsBucket())
                    .TimeOut(t)
                    .Build();
                _client = new InfluxDBClient(_options);

                _writeApi = _client.GetWriteApiAsync();

                _vmQueries = new VictoriametricsQuery();
                _aggInterval = Config.GetAggregationInterval();
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to initialize VictoriametricsDB. Exception: {0}", ex.ToString()));
            }
        }


        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {


            try
            {

                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.EndDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.RangeRaw.Replace(QueryParams.StartParam, startEP.ToString())
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorIDs))
                                                        .Replace(QueryParams.AggWindow, Config.GetRegularTsScaleMilliseconds().ToString()+"ms"); // irreg = 1ms geht nicht, da VM eine querybechraenkung von 30K datapoints hat.


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

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

        public async Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery query)
        {

    
            try
            {

                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.EndDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.RangeRawAllDims.Replace(QueryParams.StartParam, startEP.ToString())
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorIDs))
                                                        .Replace(QueryParams.AggWindow, Config.GetRegularTsScaleMilliseconds().ToString()+"ms"); // irreg = 1ms geht nicht, da VM eine querybechraenkung von 30K datapoints hat.


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;



                Log.Information("Number of points: " + count.ToString());

                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0,
                                           query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery query, int limit)
        {

            try
            {



                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.RangeRawLimited.Replace(QueryParams.StartParam, startEP.ToString())
                .Replace(QueryParams.Limit, limit.ToString())
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorIDs))
                                                        .Replace(QueryParams.AggWindow, Config.GetRegularTsScaleMilliseconds().ToString()+"ms"); // irreg = 1ms geht nicht, da VM eine querybechraenkung von 30K datapoints hat.


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

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

        public async Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery query, int limit)
        {

            try
            {
              

                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.RangeRawAllDimsLimited.Replace(QueryParams.StartParam, startEP.ToString())
                .Replace(QueryParams.Limit, limit.ToString())
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorIDs))
                                                        .Replace(QueryParams.AggWindow, Config.GetRegularTsScaleMilliseconds().ToString()+"ms"); // irreg = 1ms geht nicht, da VM eine querybechraenkung von 30K datapoints hat.


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

                Log.Information("Number of points: " + count.ToString());
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0,
                                           query.StartDate, query.DurationMinutes, 0, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {

            try
            {
              

                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.OutOfRange.Replace(QueryParams.StartParam, startEP.ToString())
                .Replace(QueryParams.MinValParam,  query.MinValue.ToString())
                .Replace(QueryParams.MaxValParam,  query.MaxValue.ToString())
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorID.ToString()))
                                                        .Replace(QueryParams.AggWindow, Config.GetRegularTsScaleMilliseconds().ToString()+"ms"); // irreg = 1ms geht nicht, da VM eine querybechraenkung von 30K datapoints hat.


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

                Log.Information("Number of points: " + count.ToString());
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
                
                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.AggDifference.Replace(QueryParams.StartParam, startEP.ToString()) 
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorIDs.ToString()))
                                                        ;


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

                Log.Information("Number of points: " + count.ToString());
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Aggregated Data on InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {

            try
            {
              
                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.StdDev.Replace(QueryParams.StartParam, startEP.ToString()) 
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SensorIDsParam, String.Join("|", query.SensorID.ToString()))
                                                        ; 


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

                Log.Information("Number of points: " + count.ToString());
                return new QueryStatusRead(true, count, new PerformanceMetricRead(sw.ElapsedMilliseconds, count, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute STD Dev query on Influxdb. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {



            try
            {
              
                var startEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var endEP = (query.StartDate.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds;

                var vmquery = _vmQueries.StdDev.Replace(QueryParams.StartParam, startEP.ToString()) 
                                                        .Replace(QueryParams.EndParam, endEP.ToString())
                                                        .Replace(QueryParams.SecondSensorIDParam,  query.FirstSensorID.ToString())
                                                        .Replace(QueryParams.FirstSensorIDParam, query.SecondSensorID.ToString())
                                                        ; 


                Log.Information("MetricsQL query: " + vmquery);
                using HttpClient client = new();

                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
                    client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");  // await ProcessRepositoriesAsync(client);



                    // Stopwatch sw = Stopwatch.StartNew();
                    // async Task ProcessRepositoriesAsync(HttpClient client)
                    // {

                    //     var jresult = await client.GetStringAsync(@"http://localhost:8428" +
                    //                      "/api/v1/query_range?" + vmquery);


                    //     Result answer = JsonSerializer.Deserialize<Result>(jresult);

                    //     points = answer.data.result.Count();
                    // }

                }

                Stopwatch sw = Stopwatch.StartNew();
                var jresult = await client.GetStringAsync(Config.GetVictoriametricsHost() +
                                     "/api/v1/query_range?" + vmquery);
                sw.Stop();
                Result answer = JsonSerializer.Deserialize<Result>(jresult);
                var results = answer.data.result;
                int count = results.Count;

                // results  != null && results.Count > 0 ? results[0].metric.Count : 0;

                Log.Information("Number of points: " + count.ToString());
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

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

                var lineData = new List<string>(batch.Size);
                foreach (var item in batch.Records)
                {
                    var timeSpan = item.Time.Subtract(EpochStart);
                    var time = TimeSpanToBigInteger(timeSpan, WritePrecision.Ms);

                    if (Config.GetMultiDimensionStorageType() == "column")
                    {
                        int c = 1; StringBuilder builder = new StringBuilder("");
                        while (c < Config.GetDataDimensionsNr()) { builder.Append($",{Constants.Value}_{c}={item.ValuesArray[c]}"); c++; }
                        lineData.Add($"{"test" + Config.GetPolyDimTableName()},sensor_id={item.SensorID} {Constants.Value}_{0}={item.ValuesArray[0]}{builder} {time}");
                    }
                    else
                        lineData.Add($"{Config.GetPolyDimTableName()},sensor_id={item.SensorID} {Constants.Value}={item.ValuesArray} {time}");
                }


                Stopwatch sw = Stopwatch.StartNew();

                await _writeApi.WriteRecordsAsync(lineData, WritePrecision.Ms);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into VictoriamentrcsDB. Exception: {0}", ex.ToString()));
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
                lineData.Add($"{Config.GetPolyDimTableName()},sensor_id={record.SensorID} value={record.ValuesArray} {time}");

                Stopwatch sw = Stopwatch.StartNew();
                await _writeApi.WriteRecordsAsync(lineData, WritePrecision.Ns);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, 1, 0, Operation.StreamIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into InfluxDB. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, 1, Operation.StreamIngestion), ex, ex.ToString());
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
        private string[] GetSeriesNames()
        {
            int[] AllDim = new int[Config.GetDataDimensionsNr()];
            AllDim = Enumerable.Range(0, Config.GetDataDimensionsNr()).ToArray();

            return GetSeriesNames(AllDim);
        }
        private string[] GetSeriesNames(int[] dimensions)
        {
            string[] series;

            series = new String[Config.GetDataDimensionsNr()];

            foreach (int d in dimensions)
                series[d] = Config.GetPolyDimTableName() + "_dim_" + d;


            return series.Where(c => c != null).ToArray();
        }
        public class Result
        {
            public string status { get; set; }
            public Data data { get; set; }
            // public IList<Record>? data { get; set; }
            public Stats stats { get; set; }
        }
        public class Record
        {
            public List<Metric> metric { get; set; }
        }
        public class Metric
        {
            public List<Name> __name__ { get; set; }
            public List<Values> values { get; set; }
        }
        public class Name
        {
            public string __name__ { get; set; }
        }
        public class Values
        {
            public Dictionary<string, string> values { get; set; }
        }
        public class Data
        {
            public string resultType { get; set; }
            public List<Record> result { get; set; }

        }
        public class Stats
        {
            public string seriesFetched { get; set; }

        }

    }


}


