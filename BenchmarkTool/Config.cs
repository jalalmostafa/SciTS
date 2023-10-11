using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace BenchmarkTool
{
    public class Config
    {
        public static string GetGlancesStorageFileSystem()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesStorageFileSystem];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesStorageFileSystem));
            return val;
        }
        public static bool GetPrintModeEnabled()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.PrintModeEnabled];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.PrintModeEnabled));
            bool.TryParse(val, out bool print);
            return print;
        }
        public static DateTime GetStartTime()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.StartTime];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.StartTime));
            DateTime.TryParse(val, out DateTime date);
            return date;
        }

        public static int GetSensorNumber()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.SensorNumber];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.SensorNumber));
            int.TryParse(val, out int sensors);
            return sensors;
        }

        public static int _actualDataDimensionsNr = 0;
        public static int GetDataDimensionsNr()
        {
            var val = _actualDataDimensionsNr;
            if (val == 0)
                val = GetDataDimensionsNrOptions().First<int>();
            return val;
        }
        public static int[] GetDataDimensionsNrOptions()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DataDimensionsNrOptions];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DataDimensionsNr));
            return Array.ConvertAll(val.Split(","), s => int.TryParse(s, out var x) ? x : -1);
        }

        public static string GetPolyDimTableName()
        {
            var val = Constants.TableName + "_dim_" + Config.GetDataDimensionsNr() + "_" + Config.GetIngestionType();

            return val;
        }
        public static string[] GetAllPolyDimTableNames()
        {
            var list = new List<string>();
            foreach (var dim in GetDataDimensionsNrOptions())
            {
                list.Add(Constants.TableName + "_dim_" + dim + "_" + Config.GetIngestionType());
            }
            var val = list.ToArray<string>();

            return val;
        }
        public static int GetTestRetries()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.TestRetries];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.TestRetries));
            int.TryParse(val, out int loop);
            return loop;
        }
        private static string _DbSetting = "null";
        public static string GetTargetDatabase()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.TargetDatabase];
            if (_DbSetting.Contains("DB"))
            {
                val = _DbSetting;
            }
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.TargetDatabase));
            return val;
        }
        internal static void SetTargetDatabase(string setting)
        {
            _DbSetting = setting;
        }

        public static string GetPostgresConnection()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.PostgresConnection];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.PostgresConnection));
            return val;
        }

        public static string GetDatalayertsConnection()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DatalayertsConnection];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DatalayertsConnection));
            return val;
        }
        public static string GetDatalayertsUser()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DatalayertsUser];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DatalayertsUser));
            return val;
        }
        public static string GetDatalayertsPassword()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DatalayertsPassword];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DatalayertsPassword));
            return val;
        }
        public static int GetRegularTsScaleMilliseconds()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.RegularTsScaleMilliseconds];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DatalayertsPassword));
            int.TryParse(val, out int intVal);
            return intVal;
        }
        public static string GetTimescaleConnection()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.TimescaleConnection];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.PostgresConnection));
            return val;
        }

        public static string GetMySQLConnection()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MySQLConnection];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MySQLConnection));
            return val;
        }

        public static string GetMetricsCSVPath()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MetricsCSVPath];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MetricsCSVPath));
            return val;
        }

        public static string GetInfluxHost()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.InfluxDBHost];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.InfluxDBHost));
            return val;
        }

        public static string GetInfluxToken()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.InfluxDBToken];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.InfluxDBToken));
            return val;
        }

        public static string GetInfluxBucket()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.InfluxDBBucket];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.InfluxDBBucket));
            return val;
        }

        public static string GetInfluxOrganization()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.InfluxDBOrganization];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.InfluxDBOrganization));
            return val;
        }
        public static string GetVictoriametricsHost()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.VictoriametricsDBHost];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.VictoriametricsDBHost));
            return val;
        }

        public static string GetVictoriametricsToken()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.VictoriametricsDBToken];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.VictoriametricsDBToken));
            return val;
        }

        public static string GetVictoriametricsBucket()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.VictoriametricsDBBucket];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.VictoriametricsDBBucket));
            return val;
        }

        public static string GetVictoriametricsOrganization()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.VictoriametricsDBOrganization];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.VictoriametricsDBOrganization));
            return val;
        }

        public static string GetClickhouseHost()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.ClickhouseHost];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.ClickhouseHost));
            return val;
        }

        public static string GetClickhouseUser()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.ClickhouseUser];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.ClickhouseUser));
            return val;
        }

        public static int GetClickhousePort()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.ClickhousePort];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.ClickhousePort));
            int.TryParse(val, out int intVal);
            return intVal;
        }

        public static string GetClickhouseDatabase()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.ClickhouseDatabase];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.ClickhouseDatabase));
            return val;
        }

        public static string QueryTypeOnRunTime;



        public static string[] _QueryArray;

        public static string GetQueryType()
        {

            var val = ConfigurationManager.AppSettings[ConfigurationKeys.QueryType];
            if (String.IsNullOrEmpty(QueryTypeOnRunTime)) //INIT
            {
                if (val == "All") //TODO move logic to Config.GetQueryOptions
                    Config._QueryArray = new string[] { "RangeQueryRawData", "RangeQueryRawAllDimsData", "RangeQueryAggData", "OutOfRangeQuery", "DifferenceAggQuery", "STDDevQuery" };
                else if (val == "Agg" | Program.Mode.Contains("Agg"))
                    Config._QueryArray = new string[] { "RangeQueryAggData", "DifferenceAggQuery", "STDDevQuery" };
                else
                {   
                    List<string> valA = val.Split(',').ToList();
                    foreach( var x in valA){
                      x.ToEnum<Operation>();
                    } 
                    // TODO insert check method assert correct parsing
                    
                    Config._QueryArray = valA.ToArray();
                }

                QueryTypeOnRunTime = _QueryArray.First();
            }

            val = QueryTypeOnRunTime;

            return val;
        }

        private static string _ingType = "null";
        internal static void SetIngestionType(string ingType)
        {
            _ingType = ingType;
        }
        public static string GetIngestionType()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.IngestionType];
            if (_ingType.Contains("reg"))
                val = _ingType;
            if (String.IsNullOrEmpty(val) | (val != "regular" & val != "irregular"))
                throw new Exception(String.Format("Invalid or Null or empty app settings val for key={0}", ConfigurationKeys.IngestionType));
            return val;
        }
        public static string GetMultiDimensionStorageType()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MultiDimensionStorageType];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MultiDimensionStorageType));
            return val;
        }


        public static int GetAggregationInterval()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.AggregationInterval];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.AggregationInterval));
            int.TryParse(val, out int intVal);
            return intVal;
        }

        public static long GetDurationMinutes()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DurationMinutes];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DurationMinutes));
            long.TryParse(val, out long duration);
            return duration;
        }
        public static bool _sensorFilterAll = false;
        public static string GetSensorsFilterString()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.SensorsFilter];
            if (val == "All")
                _sensorFilterAll = true;
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.SensorsFilter));
            return val;
        }

        public static int[] GetSensorsFilter()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.SensorsFilter];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.SensorsFilter));
            int[] aa;
            if (_sensorFilterAll == true)
            {
                aa = new int[Config.GetSensorNumber()];
                for (var i = 0; i < aa.Length; i += 1)
                {
                    aa[i] = i;
                }
                return aa;
            }
            else
                return Array.ConvertAll(val.Split(","), s => int.TryParse(s, out var x) ? x : -1);
        }


        public static int GetSensorID()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.SensorID];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.SensorID));
            int.TryParse(val, out int sensorId);
            return sensorId;
        }

        public static double GetMaxValue()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MaxValue];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MaxValue));
            double.TryParse(val, out double max);
            return max;
        }

        public static double GetMinValue()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MinValue];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MinValue));
            double.TryParse(val, out double min);
            return min;
        }

        public static int GetFirstSensorID()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.FirstSensorID];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.FirstSensorID));
            int.TryParse(val, out int sensorId);
            return sensorId;
        }

        public static int GetSecondSensorID()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.SecondSensorID];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.SecondSensorID));
            int.TryParse(val, out int sensorId);
            return sensorId;
        }


        public static int[] GetClientNumberOptions()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.ClientNumberOptions];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.ClientNumberOptions));
            return Array.ConvertAll(val.Split(","), s => int.TryParse(s, out var x) ? x : -1);
        }

        public static int[] GetBatchSizeOptions()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.BatchSizeOptions];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.BatchSizeOptions));
            return Array.ConvertAll(val.Split(","), s => int.TryParse(s, out var x) ? x : -1);
        }

        public static int GetDaySpan()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.DaySpan];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.DaySpan));
            int.TryParse(val, out int days);
            return days;
        }

        public static string GetGlancesUrl()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesUrl];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesUrl));
            return val;
        }

        public static int GetGlancesDatabasePid()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesDatabasePid];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesDatabasePid));
            int.TryParse(val, out int pid);
            return pid;
        }

        public static int GetGlancesPeriod()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesPeriod];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesPeriod));
            int.TryParse(val, out int period);
            return period;
        }

        public static string GetGlancesOutput()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesOutput];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesOutput));
            return val;
        }

        public static string GetGlancesDisk()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesDisk];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesDisk));
            return val;
        }

        public static string GetGlancesNIC()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.GlancesNIC];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.GlancesNIC));
            return val;
        }


        public static int _actualMixedWLPercentage;
        public static int[] GetMixedWLPercentageOptions()
        {
            var val = ConfigurationManager.AppSettings[ConfigurationKeys.MixedWLPercentageOptions];
            if (String.IsNullOrEmpty(val))
                throw new Exception(String.Format("Null or empty app settings val for key={0}", ConfigurationKeys.MixedWLPercentageOptions));
            return Array.ConvertAll(val.Split(","), s => int.TryParse(s, out var x) ? x : -1); ;
        }
    }
}
