using BenchmarkTool.Database.Queries;

using Npgsql;
using PostgreSQLCopyHelper;
using Serilog;
using System;
using System.Text;

using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;

using Python.Runtime;
using System.ComponentModel;
using System.IO;
using System.Configuration;

namespace BenchmarkTool.Database
{
    public class TimescaleDBPy : IDatabase
    {
        private NpgsqlConnection _connection;
        private PostgreSQLCopyHelper<IRecord> _copyHelper;
        private IQuery<String> _query;
        private string _connectionConfig;
        private int _aggInterval;
        private string filePath = "/mnt/unison-box/dropbox/Coop-projekte/kit-ipe-sciTS/gitrepo/my-opi-git/SciTS/BenchmarkTool/Database/timescaleEmbed.py";
        private static bool _TableCreated;

        protected TimescaleDBPy(IQuery<String> query, string connectionConfig)
        {
            _query = query;
            _connectionConfig = connectionConfig;
            _aggInterval = Config.GetAggregationInterval();
        }

        public TimescaleDBPy() : this(new PostgresQuery(), Config.GetTimescaleConnection())
        {
        }

        public void Cleanup()
        {
            // var command = _connection.CreateCommand();
            // command.CommandText = $"DELETE FROM {Config.GetPolyDimTableName()} WHERE {Constants.SensorID} IS NOT NULL";
            // command.ExecuteNonQuery();
        }

        public void CheckOrCreateTable()
        {
            try
            {
                var dimNb = 0;
                if (_TableCreated != true)
                {
                    if (BenchmarkTool.Program.Mode.Contains("populate_Day+0")) // To just create index and hypertable when it does not exist yet, as IF nOT EXISTS did not work
                    {
                        if (Config.GetMultiDimensionStorageType() == "column")
                        {
                            foreach (var tableName in Config.GetAllPolyDimTableNames())
                            {
                                var actualDim = Config.GetDataDimensionsNrOptions()[dimNb];
                                int c = 0; StringBuilder builder = new StringBuilder("");

                                while (c < actualDim) { builder.Append(", " + Constants.Value + "_" + c + " double precision"); c++; }

                                NpgsqlCommand m_createtbl_cmd = new NpgsqlCommand(
                                  String.Format("CREATE TABLE IF NOT EXISTS " + tableName + " ( time timestamp(6) with time zone NOT NULL, sensor_id integer " + builder + ") ; SELECT create_hypertable('" + tableName + "', 'time', if_not_exists => TRUE);    CREATE INDEX IF NOT EXISTS " + tableName + "_myindex" + " ON " + tableName + " ( sensor_id, time DESC); --UNIQUE;  ALTER TABLE " + tableName + " SET  (  timescaledb.compress,  timescaledb.compress_segmentby = 'sensor_id'); SELECT add_compression_policy('" + tableName + "', INTERVAL '7 days'); ")
                                   , _connection);

                                m_createtbl_cmd.ExecuteNonQuery();

                                // NpgsqlCommand m_createHtbl_cmd = new NpgsqlCommand(
                                // String.Format(" ")
                                //  , _connection);

                                // m_createHtbl_cmd.ExecuteNonQuery();

                                _TableCreated = true;
                                dimNb++;
                            }
                        }
                        else
                            throw new NotImplementedException();
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to close. Exception: {0}", ex.ToString()));
            }
        }

        public void Init()
        {

            try
            {
                _connection = new NpgsqlConnection(_connectionConfig);
                _connection.Open();

                if (Config.GetMultiDimensionStorageType() == "column")
                {
                    _copyHelper = new PostgreSQLCopyHelper<IRecord>(Constants.SchemaName, Config.GetPolyDimTableName())
                                .MapTimeStamp(Constants.Time, x => x.Time)
                                .MapInteger(Constants.SensorID, x => x.SensorID);
                    for (var i = 0; i < Config.GetDataDimensionsNr(); i++)
                    {
                        int j = i;
                        _copyHelper = _copyHelper.MapDouble(Constants.Value + "_" + i, x => x.ValuesArray[j]);
                    }

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

                Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";


                var newQueryString = _query.OutOfRange.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorID, "'" + query.SensorID.ToString() + "'")
                                                        .Replace("@" + QueryParams.MaxVal, query.MaxValue.ToString())
                                                        .Replace("@" + QueryParams.MinVal, query.MinValue.ToString());
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;

                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.OutOfRangeQuery));
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

                // using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                // var points = 0;
                // while (reader.Read())
                // {
                //     points++;
                // }
                // sw.Stop();
                // await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                var points = 0;

                Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";


                var newQueryString = _query.AggDifference.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.FirstSensorID, query.FirstSensorID.ToString())
                                                        .Replace("@" + QueryParams.SecondSensorID, query.SecondSensorID.ToString());
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;

                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.DifferenceAggQuery));
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
                Log.Information("Start date: {0}, end date: {1}, sensors: {2}", query.StartDate, query.EndDate, query.SensorIDs);
                using var cmd = new NpgsqlCommand(_query.RangeAgg, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);

                var points = 0;
                // Stopwatch sw = Stopwatch.StartNew();
                // using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                // while (reader.Read())
                // {
                //     points++;
                // }
                // sw.Stop();
                // await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                
                Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";


                var newQueryString = _query.RangeAgg.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorIDs, "'{" + string.Join(",", query.SensorIDs) + "}'");
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;

                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
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
                // cmd.Prepare();
                // Stopwatch sw = Stopwatch.StartNew();
                // using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                // while (reader.Read())
                // {
                //     points++;
                // }
                                Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";
 var newQueryString = _query.RangeRaw.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorIDs, "'{" + string.Join(",", query.SensorIDs) + "}'");

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;

               
                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
      
                 return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
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
                               Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";

                var newQueryString = _query.RangeRawAllDims.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorIDs, "'{" + string.Join(",", query.SensorIDs) + "}'");
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;


                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery query, int limit)
        {
            try
            {
                Log.Information(String.Format("Start Date: {0}", query.StartDate.ToString()));
                Log.Information(String.Format("End Date: {0}", query.EndDate.ToString()));

                using var cmd = new NpgsqlCommand(_query.RangeRawLimited, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);

                cmd.Parameters.AddWithValue(QueryParams.Limit, NpgsqlTypes.NpgsqlDbType.Integer, limit);

                var points = 0;
                               Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";


                var newQueryString = _query.RangeRawAllDims.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorIDs, "'{" + string.Join(",", query.SensorIDs) + "}'")
                                                        .Replace("@" + QueryParams.Limit, "'" + limit + "'");

                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;
                using (Py.GIL())
                {
                    
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }
        public async Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery query, int limit)
        {
            try
            {
                Log.Information(String.Format("Start Date: {0}", query.StartDate.ToString()));
                Log.Information(String.Format("End Date: {0}", query.EndDate.ToString()));

                using var cmd = new NpgsqlCommand(_query.RangeRawAllDimsLimited, _connection);
                cmd.Parameters.AddWithValue(QueryParams.Start, NpgsqlTypes.NpgsqlDbType.Timestamp, query.StartDate);
                cmd.Parameters.AddWithValue(QueryParams.End, NpgsqlTypes.NpgsqlDbType.Timestamp, query.EndDate);
                cmd.Parameters.AddWithValue(QueryParams.SensorIDs, NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, query.SensorIDs);

                cmd.Parameters.AddWithValue(QueryParams.Limit, NpgsqlTypes.NpgsqlDbType.Integer, limit);
                // var points = 0;
                // cmd.Prepare();
                // Stopwatch sw = Stopwatch.StartNew();
                // using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                // while (reader.Read())
                // {
                //     points++;
                // }
                // sw.Stop();
                // await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                 var points = 0;
                               Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";

            

                var newQueryString = _query.RangeRawAllDims.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorIDs, "'{" + string.Join(",", query.SensorIDs) + "}'")
                                                        .Replace("@" + QueryParams.Limit, "'" + limit + "'");
    PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;
                using (Py.GIL())
                {
                    
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
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

                //  using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
                var points = 0;
                // while (reader.Read())
                // {
                //     points++;
                // }
                // sw.Stop();
                // await Print(reader, query.ToString(), Config.GetPrintModeEnabled());
                 Stopwatch sw;
                // python embedding, as NPGSQL does not work as expected
                Runtime.PythonDLL = "/usr/lib/aarch64-linux-gnu/libpython3.10.so";


                var newQueryString = _query.StdDev.Replace("@" + QueryParams.Start, "'" + query.StartDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.End, "'" + query.EndDate.ToString("s") + "'")
                                                        .Replace("@" + QueryParams.SensorID, query.SensorID.ToString()) ;
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                dynamic m_result;

                using (Py.GIL())
                {
                    dynamic os = Py.Import("os");
                    dynamic sys = Py.Import("sys");
                    sys.path.append(os.path.dirname(os.path.expanduser(filePath)));
                    var fromFile = Py.Import(Path.GetFileNameWithoutExtension(filePath));

                    var s = Config.GetTimescaleConnection();
                    int i1 = s.IndexOf("Password=") + 9;
                    int i2 = s.IndexOf(";Command");
                    var pwd = s.Substring(i1, i2 - i1);
                    int j1 = s.IndexOf("Server=") + 7;
                    int j2 = s.IndexOf(";Port");
                    var hst = s.Substring(j1, j2 - j1);
                    int k1 = s.IndexOf("Port=") + 5;
                    int k2 = s.IndexOf(";Database");
                    var prt = s.Substring(k1, k2 - k1);
                    var connString = "host=" + hst + " dbname=" + _connection.Database + " user=" + _connection.UserName + " password=" + pwd + " port=" + prt;
                    sw = Stopwatch.StartNew();
                    m_result = fromFile.InvokeMethod("conn", new PyObject[] { connString.ToPython(), newQueryString.ToPython() }).AsManagedObject(typeof(double[]));
                }
                double PyLatency = m_result[0];
                points = (int)m_result[1];

                sw.Stop();
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.Elapsed.TotalMicroseconds, points, 0, query.StartDate, query.DurationMinutes, 0, Operation.STDDevQuery));
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

                    return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, 1, 0, Operation.StreamIngestion));
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
                await _copyHelper.SaveAllAsync(_connection, batch.RecordsArray);
                sw.Stop();
                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, batch.Size, 0, Operation.BatchIngestion));
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

