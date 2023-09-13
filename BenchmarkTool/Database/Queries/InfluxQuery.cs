using System;

namespace BenchmarkTool.Database.Queries
{
    public class InfluxQuery : IQuery<String>
    {

                private static string _rangeRawAllDims = @"from(bucket: ""{0}"")   
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] =~ {5}) ";
        private static string _rangeRaw = @"from(bucket: ""{0}"") 
                                                        |> keep(columns: [""{4}"", ""{6}"",""{7}""]) 
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] =~ {5})";

        private static string _rangeRawAllDimsLimited = @"from(bucket: ""{0}"")   
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] =~ {5}) 
                                                        |> limit(n:{6})";
        private static string _rangeRawLimited = @"from(bucket: ""{0}"") 
                                                        |> keep(columns: [""{4}"", ""{6}"",""{7}""]) 
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] =~ {5})
                                                        |> limit(n:{8})"; 

        private static string _outOfRange = @"data = from(bucket: ""{0}"")  
                                                        |> range(start: {1}, stop: {2})     
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] == ""{5}"") 
                                                        min = data   
                                                            |> aggregateWindow( column: ""_{6}"", every: {7}h, fn: min   ) 
                                                        max = data   
                                                            |> aggregateWindow( column: ""_{6}"", every: {7}h, fn: max  ) 
                                                        join(tables: {{min: min, max: max}}, on: [""_start"", ""_stop"", ""_time"", ""_field"", ""_measurement"", ""{8}""], method: ""inner"") 
                                                            |> filter(fn: (r) => r[""_{6}_max""] > {9} or r[""_{6}_min""]  < {10})";

        private static string _stdDev = @"from(bucket: ""{0}"")   
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"" and r[""{4}""] == ""{5}"")   
                                                        |> stddev()";

        private static string _aggDifference = @"data = from(bucket: ""{0}"")
                                                        |> range(start: {1}, stop: {2})
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] == ""{5}"" or r[""{4}""] == ""{6}"") 
                                                        sen1 = data   
                                                            |> filter(fn: (r) => r[""{4}""] == ""{5}"")
                                                            |> aggregateWindow(column: ""_{7}"", every: {8}h, fn: mean , createEmpty: false  )
                                                        sen2 = data   
                                                            |> filter(fn: (r) => r[""{4}""] == ""{6}"")
                                                            |> aggregateWindow(column: ""_{7}"", every: {8}h, fn: mean , createEmpty: false  )
                                                        join(tables: {{ sen1: sen1, sen2: sen2}}, on: [""_start"", ""_stop"", ""_time"", ""_field"", ""_measurement""], method: ""inner"") 
                                                            |> map(fn: (r) =>({{
                                                                _time: r._time,
                                                                _value: r._{7}_sen2 - r._{7}_sen1 }}))";

        private static string _rangeAgg = @"from(bucket: ""{0}"")   
                                                        |> range(start: {1}, stop: {2})   
                                                        |> filter(fn: (r) => r[""_measurement""] == ""{3}"")   
                                                        |> filter(fn: (r) => r[""{4}""] =~ {5})   
                                                        |> aggregateWindow(every: {6}h, fn: mean, createEmpty: false)  
                                                        |> yield(name: ""mean"")";
        



        public String RangeRaw =>
            String.Format(_rangeRaw, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDsParam, Constants.Time, Constants.Value + "_1");
        public String RangeRawAllDims =>
            String.Format(_rangeRaw, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDsParam);
        public String RangeRawLimited =>
            String.Format(_rangeRaw, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDsParam, Constants.Time, Constants.Value + "_1", QueryParams.Limit);
        public String RangeRawAllDimsLimited =>
            String.Format(_rangeRaw, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDsParam, QueryParams.Limit);

        public String RangeAgg =>
            String.Format(_rangeAgg, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDsParam,
            Config.GetAggregationInterval());

        public String OutOfRange =>
            String.Format(_outOfRange, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDParam, Constants.Value + "_1",
            Config.GetAggregationInterval(), Constants.SensorID,
            QueryParams.MinValParam, QueryParams.MaxValParam);

        public String StdDev =>
            String.Format(_stdDev, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.SensorIDParam);

        public String AggDifference =>
            String.Format(_aggDifference, Config.GetInfluxBucket(),
            QueryParams.StartParam, QueryParams.EndParam, Config.GetPolyDimTableName(),
            Constants.SensorID, QueryParams.FirstSensorIDParam,
            QueryParams.SecondSensorIDParam, Constants.Value + "_1",
            Config.GetAggregationInterval());
    }
}
