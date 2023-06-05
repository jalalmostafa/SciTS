using System;

namespace BenchmarkTool.Database.Queries
{
    public class TimescaleQuery : IQuery<String>
    {
        private static string _rangeAgg = @"SELECT time_bucket('{0}h', {1}) AS time_agg, sensor_id, avg({2}) FROM {3}
                                            where {1} >= {4} and {1} <= {5} and {6} = ANY({7}) group by time_agg, sensor_id";

        private static string _rangeRaw = @"SELECT * FROM {0} where {1} >= {2} and {1} <= {3} and {4} = ANY({5})";


        private static string _outOfRange = @"SELECT time_bucket('{0}h', {1}) AS time_agg, max({2}), min({2}) FROM {3}
                                            where {1} >= {4} and {1} <= {5} and {6} = {7}
                                            group by time_agg having min({2}) < {8} OR max({2}) >{9}";

        public static string _stdDev = @"SELECT stddev({0}) FROM {1} where {2} >={3} and {2}<={4} and {5} = {6}";

        private static string _aggDifference = @"SELECT A1.time_agg, A2.val - A1.val as difference from
                                                    (SELECT time_bucket('{0}h', {1}) AS time_agg , avg({2}) as val FROM {3}
                                                        where {1} >={4} and {1} <= {5} and {6} = {7} group by time_agg)A1
                                                inner join
                                                    (SELECT time_bucket('{0}h', {1}) AS time_agg , avg({2}) as val FROM {3}
                                                        where {1} >={4} and {1} <= {5} and {6} = {8} group by time_agg)A2
                                                On A1.time_agg = A2.time_agg";
        public String RangeAgg => String.Format(_rangeAgg, Config.GetAggregationInterval(), Constants.Time, Constants.Value, Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam);

        public String RangeRaw => String.Format(_rangeRaw, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDsParam);

        public String OutOfRange => String.Format(_outOfRange, Config.GetAggregationInterval(), Constants.Time, Constants.Value, Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDParam, QueryParams.MinValParam, QueryParams.MaxValParam);

        public String StdDev => String.Format(_stdDev, Constants.Value, Config.GetPolyDimTableName(), Constants.Time, QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.SensorIDParam);

        public String AggDifference => String.Format(_aggDifference, Config.GetAggregationInterval(), Constants.Time, Constants.Value, Config.GetPolyDimTableName(), QueryParams.StartParam, QueryParams.EndParam, Constants.SensorID, QueryParams.FirstSensorIDParam, QueryParams.SecondSensorIDParam);

    }
}
