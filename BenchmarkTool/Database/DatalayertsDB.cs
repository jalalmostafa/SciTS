using DataLayerTS;
using DataLayerTS.Models;
using Serilog;
using System;
using System.Linq;

using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;



namespace BenchmarkTool.Database
{

    public class DatalayertsDB : IDatabase
    {

        private ReusableClient _client;

        public IQuery<ContainerRequest> _iquery;

        private int _aggInterval;



        public void Init()
        {
            _client = new ReusableClient(Config.GetDatalayertsConnection(), Config.GetDatalayertsUser(), Config.GetDatalayertsPassword());
            _iquery = new DatalayertsQuery();
            _aggInterval = Config.GetAggregationInterval();
        }


        public void Cleanup() { }

        public void Close() { }



        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {


            try
            {
                int scale = Config.GetDatalayertsScaleMilliseconds();
                DateTime roundedDate = new DateTime(Config.GetStartTime().Year, Config.GetStartTime().Month, Config.GetStartTime().Day, Config.GetStartTime().Hour, Config.GetStartTime().Minute, Config.GetStartTime().Second, Config.GetStartTime().Millisecond, DateTimeKind.Utc);
                Dictionary<int, List<double>> DictOfSensorIDsToListsFromValueArrays = new Dictionary<int, List<double>>();
                Dictionary<int, int[]> IndexOfSensorIDsDimensions = new Dictionary<int, int[]>();

        
                foreach (var item in batch.Records)
                {
                    double[] ThisRecodsValueArray = new double[item.ValuesArray.Length];

                    for (int i = 0; i < item.ValuesArray.Length; i++)
                    {
                        ThisRecodsValueArray[i] = item.ValuesArray[i];  // no casting needed // maybe no for loop needed TODO
                    }

                    for (int d = 0; d < Config.GetDataDimensionsNr(); d++)
                    {
                        if (!DictOfSensorIDsToListsFromValueArrays.ContainsKey(item.SensorID * Config.GetDataDimensionsNr() + d))
                        {
                            DictOfSensorIDsToListsFromValueArrays[item.SensorID * Config.GetDataDimensionsNr() + d] = new List<double>();
                        }

                        IndexOfSensorIDsDimensions[item.SensorID * Config.GetDataDimensionsNr() + d] = new int[2] { item.SensorID, d };
                        DictOfSensorIDsToListsFromValueArrays[item.SensorID * Config.GetDataDimensionsNr() + d].Add(ThisRecodsValueArray[d]); 

                    }

                }
                Dictionary<int, List<double>>.ValueCollection val_col = DictOfSensorIDsToListsFromValueArrays.Values;

                string[] series = new int[Config.GetSensorNumber()].Select(i => "sensor_id_" + i.ToString()).ToArray();

                VectorContainer<double> vectorContainer = new VectorContainer<double>()
                {
                    FirstTimestamp = roundedDate,
                    IntervalTicks = 10000 * scale, // second = 10mil
                    LastTimestamp = roundedDate.AddMilliseconds((val_col.First().Count-1) * scale )
                };
                vectorContainer.Vectors = new TimeSeriesVector<double>[Config.GetSensorNumber() * Config.GetDataDimensionsNr()].Select(a => new TimeSeriesVector<double>()).ToArray();
                // init
                for (int j = 0; j < Config.GetSensorNumber(); j++)
                {
                    for (int d = 0; d < Config.GetDataDimensionsNr(); d++)
                    {
                        vectorContainer.Vectors[j * Config.GetDataDimensionsNr() + d].Directory = Config.GetPolyDimTableName() + "_in_" + scale + "_ms_steps_with_" + Config.GetDataDimensionsNr() + "_dimensions";
                        vectorContainer.Vectors[j * Config.GetDataDimensionsNr() + d].Series = "sensor_id_" + j + "_dim_" + d;
                        vectorContainer.Vectors[j * Config.GetDataDimensionsNr() + d].Values = new double[val_col.First().Count]; ;
                    }
                }
                // fill
                foreach (var DimSensorNr in DictOfSensorIDsToListsFromValueArrays.Keys)
                {
                    vectorContainer.Vectors[DimSensorNr].Directory = Config.GetPolyDimTableName() + "_in_" + scale + "_ms_steps_with_" + Config.GetDataDimensionsNr() + "_dimensions";
                    vectorContainer.Vectors[DimSensorNr].Series = "sensor_id_" + IndexOfSensorIDsDimensions[DimSensorNr][0] + "_dim_" + IndexOfSensorIDsDimensions[DimSensorNr][1];
                    vectorContainer.Vectors[DimSensorNr].Values = val_col.Select(a => a.ToArray()).ToArray()[DimSensorNr];

                    if (Config.GetPrintModeEnabled() == true) // Todo remove
                        for (int i = 0; i < vectorContainer.Vectors[DimSensorNr].Values.GetLength(0); i++)
                        {
                            if (vectorContainer.Vectors[DimSensorNr].Values[i] > 0) await Console.Out.WriteLineAsync("write  | " + vectorContainer.Vectors[DimSensorNr].Values[i] + " in  " + vectorContainer.Vectors[DimSensorNr].Series + " at TS: " + roundedDate.AddMilliseconds(i * Config.GetDatalayertsScaleMilliseconds()).ToString());
                        }
                    // }
                }

                Stopwatch sw = Stopwatch.StartNew();
                await _client.IngestVectorsAsync<double>(vectorContainer, OverwriteMode.older, TimeSeriesCreationTimestampStorageType.NONE, default);
                sw.Stop();

                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to insert batch into DatalayerTS. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }


        }


        public Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            throw new NotImplementedException();
        }

        public async Task<QueryStatusRead> RangeQueryRaw(RangeQuery query)
        {
            try
            {
                var DltsQuery = _iquery.RangeRaw;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = 0;
                sw.Stop();
                await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> RangeQueryAgg(RangeQuery query)
        {
            try
            {
                var DltsQuery = _iquery.RangeAgg;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);


                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryAggData), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            try  // TODO check if true OUT of RANGE
            {
                var DltsQuery = _iquery.OutOfRange;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorID);


                DltsQuery.Selection.Add(dir, series);

                DltsQuery.Transformations[0].Function = FunctionType.FILTER;
                DltsQuery.Transformations[0].Min = query.MaxValue;
                DltsQuery.Transformations[0].Max = query.MinValue;


                Stopwatch sw = Stopwatch.StartNew();
                var points = 0;
                var readResult = await _client.RetrievePointsAsync<double>(DltsQuery, false, false, default);

                points = readResult.Count(); //TODO assert correcness

                _aggInterval = 0;
                sw.Stop();

                await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.OutOfRangeQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.OutOfRangeQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            try
            {
                var DltsQuery = _iquery.AggDifference;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = new string[] { GetSeriesNames(query.FirstSensorID)[1], GetSeriesNames(query.SecondSensorID)[1] };
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.DifferenceAggQuery), ex, ex.ToString());
            }
        }

        public async Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            try
            {
                var DltsQuery = _iquery.StdDev;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);
                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorID);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, default);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop(); await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.STDDevQuery), ex, ex.ToString());
            }
        }

        public Task Print(object readResult, string type, bool enabled)
        {
            throw new NotImplementedException();
        }
        private async Task Print(VectorContainer<double> readResult, string type, bool enabled)
        {
            if (enabled == true)
            {
                for (int c = 0; c < readResult.Timestamps.Length; c++)
                {
                    for (int d = 0; d < readResult.Vectors.Length; d++)
                    {
                        if (readResult.Vectors[d].Values[c] > 0)
                            await Console.Out.WriteLineAsync(" read | " + readResult.Vectors[d].Values[c].ToString() + " at " + readResult.Timestamps[c].ToString() + "in: " + readResult.Vectors[d].Series + "from Query:| " + type);
                    }
                }
            }
        }


        private string GetDirectoryName()
        {
            if (Config.GetDataDimensionsNr() == 1)
                return Config.GetPolyDimTableName() + "_in_" + Config.GetDatalayertsScaleMilliseconds().ToString() + "_ms_steps";
            else
                return Config.GetPolyDimTableName() + "_in_" + Config.GetDatalayertsScaleMilliseconds().ToString() + "_ms_steps_with_" + Config.GetDataDimensionsNr() + "_dimensions";
        }

        private string[] GetSeriesNames(int SensorID)
        {
            return GetSeriesNames(new int[1] { SensorID });
        }
        private string[] GetSeriesNames(int[] SensorIDs)
        {
            string[] series;
            if (Config.GetDataDimensionsNr() == 1)
                series = SensorIDs.ToString().Select(i => "sensor_id_" + i.ToString()).ToArray();
            else
            {
                series = new String[Config.GetDataDimensionsNr() * SensorIDs.Length];
                for (int c = 0; c < SensorIDs.Length; c++)
                {
                    for (int d = 0; d < Config.GetDataDimensionsNr(); d++)
                        series[c * Config.GetDataDimensionsNr() + d] = "sensor_id_" + c + "_dim_" + d;
                }
            }
            return series;
        }

    }
}