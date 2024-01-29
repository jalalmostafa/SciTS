using System;

namespace BenchmarkTool.Database.Queries
{
    public class VictoriametricsQuery : IQuery<String>
    {
//    https://prometheus.io/docs/prometheus/latest/querying/basics/
        // private static string _rangeRawAllDims = @"{2} @{0} {{{3}=~{4}}} @{1}";

// https://docs.victoriametrics.com/keyConcepts.html#range-query                
        private static string _rangeRawAllDims = @"query={2}{3}{{{4}=~'{5}'}}&start={0}&end={1}&step={6}";

            // 1) Select all the raw values for the given sensor ids on the given time range: 

            // value{sensor_id=~"id1|...|idN"}

            // This query must be passed to e.g. export API via `match[]` query arg alongside the time range via `start` and `end` query args - see these docs.
         
        private static string _rangeRaw = @"query={2}{3}{{{4}=~'{5}'}}&start={0}&end={1}&step={6}";

        private static string _rangeRawAllDimsLimited = @"query={2}{3}{{{4}=~'{5}'}}&start={0}&end={1}&step={6}&limit={7}";
        private static string _rangeRawLimited =@"query={2}{3}{{{4}=~'{5}'}}&start={0}&end={1}&step={6}&limit={7}";

        private static string _outOfRange =@"query=((min_over_time({2}{3}{{{4}=~'{5}'}}[6]))<{7}) or ((max_over_time({2}{3}{{{4}=~'{5}'}}[6]))>{8})&start={0}&end={1}";
       // 2) Out of range query:

            // (min((min_over_time(value{sensor_id="id"})) < ?) or (max((max_over_time(value{sensor_id="id"})) > ?)

            // This query must be passed to e.g. range query API via `query` query arg alongside the interval (aka `step`) and the time range via `start` and `end` query args - see these docs.

        private static string _stdDev = @"query=stddev_over_time({2}{3}{{{4}=~'{5}'}}[{6}])&start={0}&end={1}";

            // 4) Data down-sampling:

            // aggr_over_time_func(value{sensor_id=~"id1|...|idN"}[{6}])

            // See the supported `aggr_over_time_func()` options here. The query must be passed to e.g. range query API in the same way as the query 2 above.
        private static string _aggDifference = @"query=sum(avg_over_time({2}{3}{{{4}=~'{7}'}}[{6}])),(avg_over_time({2}{3}{{{4}=~'{8}'}}[{6}]))&start={0}&end={1}";

            // 5) Comparing two down-sampled values:

            // aggr_func1(aggr_over_time_func1(value{sensor_id=~"id1|...|idN"}))
            //   -
            // aggr_func2(aggr_over_time_func2(value{sensor_id=~"id1|...|idN"}))

            // The query must be sent to e.g. range query API in the same way as the query 2 above.

        private static string _rangeAgg = @"query=avg_over_time({2}{3}{{{4}=~'{5}'}}[{6}])&start={0}&end={1}"    ;

            // 3) Aggregate query:

            // aggr_func(aggr_over_time_func(value{sensor_id=~"id1|...|idN"}[d]))

            // See the supported `aggr_func()` and `aggr_over_time_func()` options here and here. The `d` must be set to the time selected time range duration. See supported values for d in these docs. The query must be passed to e.g. instant query API via `query` query arg alongside the end of the time range via `time` query arg. See these docs.

        public String RangeRaw =>
            String.Format(_rangeRaw,  
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),Constants.Value + "_1",
            Constants.SensorID, QueryParams.SensorIDsParam,  QueryParams.AggWindow);
        public String RangeRawAllDims =>
            String.Format(_rangeRawAllDims, 
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(), Constants.Value + "_*",
            Constants.SensorID, QueryParams.SensorIDsParam, QueryParams.AggWindow);
        public String RangeRawLimited =>
            String.Format(_rangeRawLimited,  
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),Constants.Value + "_0",
            Constants.SensorID, QueryParams.SensorIDsParam,  QueryParams.AggWindow, QueryParams.Limit);
        public String RangeRawAllDimsLimited =>
            String.Format(_rangeRawAllDimsLimited,  
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(), Constants.Value + "_*",
            Constants.SensorID, QueryParams.SensorIDsParam, QueryParams.AggWindow, QueryParams.Limit);

        public String RangeAgg =>
            String.Format(_rangeAgg, 
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(), Constants.Value + "_0",
            Constants.SensorID, QueryParams.SensorIDsParam, Config.GetAggregationInterval()+"h");

        public String OutOfRange =>
            String.Format(_outOfRange,  
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),Constants.Value + "_0",
            Constants.SensorID, QueryParams.SensorIDsParam, Config.GetAggregationInterval()+"h", QueryParams.MinValParam, QueryParams.MaxValParam );

        public String StdDev =>
            String.Format(_stdDev, 
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(), Constants.Value + "_0",
            Constants.SensorID, QueryParams.SensorIDsParam, Config.GetAggregationInterval()+"h");

        public String AggDifference =>
            String.Format(_aggDifference,  
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(), Constants.Value + "_0",
            Constants.SensorID,"placeholder", Config.GetAggregationInterval()+"h" , QueryParams.FirstSensorIDParam, QueryParams.SecondSensorIDParam);
    }
}
