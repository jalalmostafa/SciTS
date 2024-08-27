using System;

namespace BenchmarkTool.Database.Queries
{
    public class ClickhouseQuery : IQuery<String>
    {
        private static string _rangeRawAllDims = @"SELECT * from {0} 
                                            where {1} >= toDateTime('{2}') and {1} <= toDateTime('{3}') and {4} IN ({5})";
        private static string _rangeRaw = @"SELECT {6} from {0} 
                                            where {1} >= toDateTime('{2}') and {1} <= toDateTime('{3}') and {4} IN ({5})";

        private static string _rangeRawAllDimsLimited = @"SELECT * from {0} 
                                            where {1} >= toDateTime('{2}') and {1} <= toDateTime('{3}') and {4} IN ({5}) LIMIT {6}";
        private static string _rangeRawLimited = @"SELECT {6} from {0} 
                                            where {1} >= toDateTime('{2}') and {1} <= toDateTime('{3}') and {4} IN ({5}) LIMIT {7}";

        private static string _rangeAgg = @"SELECT toStartOfInterval({0}, INTERVAL {1} hour) as interval, avg({2}), {3} from {4}
                                            where {0} >= toDateTime('{5}') and {0} <= toDateTime('{6}') and {3} IN ({7})
                                            GROUP BY {3}, interval";

        private static string _outOfRange = @"SELECT toStartOfInterval({0} ,INTERVAL {1} hour) as interval, max({2}), min({2}), {3} from {4}
                                             where {0} >= toDateTime('{5}') and {0} <= toDateTime('{6}') and {3} = {7}
                                             GROUP BY {3}, interval
                                             HAVING max({2}) > {8} or min({2}) < {9}";
        private static string _aggDifference = @"SELECT A1.interval, A2.val - A1.val from
                                                    (SELECT toStartOfInterval({0} ,INTERVAL {1} minute) as interval, avg({2}) as val from {3}
                                                        where {0} >=toDateTime('{4}') and {0}<=toDateTime('{5}') and {6} = {7}
                                                        GROUP BY interval)A1
                                                INNER JOIN
                                                    (SELECT toStartOfInterval({0} ,INTERVAL {1} minute) as interval, avg({2}) as val from {3}
                                                        where {0} >=toDateTime('{4}') and {0}<=toDateTime('{5}') and {6} = {8}
                                                        GROUP BY interval)A2
                                                ON A1.interval = A2.interval";
        private static string _stdDev = @"SELECT stddevSamp({0}) from {1}
                                        where {2} >= toDateTime('{3}') and {2} <= toDateTime('{4}') and {5} = {6}";
        public String RangeRawAllDims => String.Format(_rangeRawAllDims, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam);

        public String RangeRaw => String.Format(_rangeRaw, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam, Constants.Value + "_0");
        public String RangeRawAllDimsLimited => String.Format(_rangeRawAllDimsLimited, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam, QueryParams.Limit);

        public String RangeRawLimited => String.Format(_rangeRawLimited, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam, Constants.Value + "_0", QueryParams.Limit);

        public String RangeAgg => String.Format(_rangeAgg, Constants.Time, Config.GetAggregationInterval(), Constants.Value + "_0", Constants.SensorID, Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, QueryParams.SensorIDsParam, QueryParams.Limit);

        public String OutOfRange => String.Format(_outOfRange, Constants.Time, Config.GetAggregationInterval(), Constants.Value + "_0", Constants.SensorID, Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, QueryParams.SensorIDParam, QueryParams.MinValParam, QueryParams.MaxValParam);

        public String StdDev => String.Format(_stdDev, Constants.Value + "_0", Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDParam);

        public String AggDifference => String.Format(_aggDifference, Constants.Time, Config.GetAggregationInterval(), Constants.Value + "_0",
            Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.FirstSensorIDParam, QueryParams.SecondSensorIDParam);
    }
}
