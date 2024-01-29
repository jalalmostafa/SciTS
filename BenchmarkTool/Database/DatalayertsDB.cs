using DataLayerTS;
using DataLayerTS.Models;
using Serilog;
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using BenchmarkTool.Database.Queries;
using MemoryPack;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace BenchmarkTool.Database
{

    public class DatalayertsDB : IDatabase
    {

        private static ReusableClient _client = new ReusableClient(Config.GetDatalayertsConnection(), Config.GetDatalayertsUser(), Config.GetDatalayertsPassword());

        public IQuery<ContainerRequest> _iquery;

        private int _aggInterval;



        public void Init()
        {
            // _client =  habe ich als statisch deklariert
            _iquery = new DatalayertsQuery();
            _aggInterval = Config.GetAggregationInterval();
        }
        public void CheckOrCreateTable()
        {
            //not needed, DLTS creates automaticly.
        }



        public void Cleanup() { }

        public void Close() { }



        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {

            try
            {
                VectorContainer<double> vectorContainer;
                TimeSeriesPoint<double> timeSeriesPoint;
                IEnumerable<TimeSeriesPoint<double>> pointContainer;

                if (Config.GetIngestionType() == "irregular")
                {

                    pointContainer = new List<TimeSeriesPoint<double>>();

                    foreach (var record in batch.RecordsArray)
                    {
                        var dirName = GetDirectoryName();
                        for (var dim = 0; dim < record.ValuesArray.Length; dim++)
                        {
                            timeSeriesPoint = new TimeSeriesPoint<double>()
                            {
                                Directory = dirName,
                                Series =  "sensor_id_" + record.SensorID + $"_{Constants.Value}_" + dim , // GetSeriesNames(record.SensorID, dim),
                                Value = record.ValuesArray[dim],
                                Timestamp = new DateTime(record.Time.Year,
                                record.Time.Month,
                                record.Time.Day,
                                record.Time.Hour,
                                record.Time.Minute,
                                record.Time.Second, // .Millisecond  unavalible for DLTS-> crashes for this kind of low interval
                                DateTimeKind.Utc)

                            };
                            pointContainer = pointContainer.Append<TimeSeriesPoint<double>>(timeSeriesPoint);
                        }
                    }

                    Stopwatch sw1 = Stopwatch.StartNew();
                    await _client.IngestPointsAsync<double>(pointContainer, OverwriteMode.older, 10000 * Config.GetRegularTsScaleMilliseconds(), TimeSeriesCreationTimestampStorageType.NONE, default).ConfigureAwait(false);

                    sw1.Stop();

                    return new QueryStatusWrite(true, new PerformanceMetricWrite(sw1.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));

                }
                else //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                {
                    var firsttime = batch.RecordsArray.First().Time;
                    DateTime roundedDate = new DateTime(firsttime.Year, firsttime.Month, firsttime.Day, firsttime.Hour, firsttime.Minute, firsttime.Second, firsttime.Millisecond, DateTimeKind.Utc);
                    int dataDims = Config.GetDataDimensionsNr();
                    int timestepIndex = -1;
                    int anzahlSensorenInBatch = 1;
                    int i = 0; bool f = true; Dictionary<int, int> sensIDperClientDict = new Dictionary<int, int>();
                    sensIDperClientDict.Add(batch.RecordsArray.First().SensorID, 0);
                    while (f)
                    {
                        var alt = batch.RecordsArray.ElementAt(i).SensorID;
                        i++;
                        var neu = batch.RecordsArray.ElementAt(i).SensorID;
                        if (neu > alt)
                        {
                            anzahlSensorenInBatch++; sensIDperClientDict.Add(neu, anzahlSensorenInBatch - 1);
                        }
                        if (alt > neu || batch.RecordsArray.ElementAt(i) == batch.RecordsArray.Last()) { f = false; }
                    }
                    int anzahlTimestepsPerDimSensor = batch.RecordsArray.Count() / anzahlSensorenInBatch;
                    ;

                    vectorContainer = new VectorContainer<double>()
                    {
                        FirstTimestamp = roundedDate,
                        IntervalTicks = 10000 * Config.GetRegularTsScaleMilliseconds(), // second = 10mil
                        LastTimestamp = roundedDate.AddMilliseconds(anzahlTimestepsPerDimSensor * Config.GetRegularTsScaleMilliseconds())
                    };
                    // vectorContainer.Vectors = new TimeSeriesVector<double>[anzahlSensorenInBatch * dataDims].Select(a => new TimeSeriesVector<double>()).ToArray();
                    vectorContainer.Vectors = new TimeSeriesVector<double>[anzahlSensorenInBatch * dataDims];

var dirName = GetDirectoryName();
                    foreach (var record in batch.RecordsArray)
                    {
                        int IndexOfSensorID = sensIDperClientDict[record.SensorID];

                        if (IndexOfSensorID == 0) //runs through all sensor IDs of the same time, then steps to the sensors of the next timeslot
                            timestepIndex++;

                        for (int chosenDim = 0; chosenDim < dataDims; chosenDim++)
                        {
                            int vectorIndex = IndexOfSensorID * dataDims + chosenDim;

                            vectorContainer.Vectors[vectorIndex] = new TimeSeriesVector<double>();

                            if (vectorContainer.Vectors[vectorIndex].Values == null)
                            {
                                vectorContainer.Vectors[vectorIndex].Directory = dirName;
                                vectorContainer.Vectors[vectorIndex].Series = "sensor_id_" + record.SensorID + $"_{Constants.Value}_" + chosenDim;
                                vectorContainer.Vectors[vectorIndex].Values = new double[anzahlTimestepsPerDimSensor + 1];
                            }

                            vectorContainer.Vectors[vectorIndex].Values[timestepIndex] = record.ValuesArray[chosenDim];
                        }
                    }
;

                    //                     VectorContainer<double> vectorContainer;
                    //                     //TODO to config
                    //                     string pth = "./DLTS_SensNb" + Config.GetSensorNumber() + "_Dim" + Config.GetDataDimensionsNr() + "_Date" + Config.GetStartTime().ToFileTimeUtc() + "_Scale" + Config.GetRegularTsScaleMilliseconds() + "_batchsize" + batch.Size + ".bin";
                    //                     if (File.Exists(pth) & false)//Debug TTODO
                    //                     {

                    //                         using (Stream stream = File.Open(pth, FileMode.Open))
                    //                         {
                    //                             vectorContainer = await MemoryPackSerializer.DeserializeAsync<VectorContainer<double>>(stream);
                    //                         }
                    //                     }
                    //                     else
                    //                     {

                    //                         int scale = Config.GetRegularTsScaleMilliseconds();
                    //                         DateTime roundedDate = new DateTime(Config.GetStartTime().Year, Config.GetStartTime().Month, Config.GetStartTime().Day, Config.GetStartTime().Hour, Config.GetStartTime().Minute, Config.GetStartTime().Second, Config.GetStartTime().Millisecond, DateTimeKind.Utc);
                    //                         Dictionary<int, List<double>> DictOfDimXSensorIDsToListsFromValueArrays = new Dictionary<int, List<double>>();
                    //                         Dictionary<int, int[]> IndexOfSensorIDsDimensions = new Dictionary<int, int[]>();  

                    // // man kann in Dicts groesse beim erstellen angeben (sind arrays) (mindestgroese mit init), Statt dicts und listen lieber arrays . Linq ist oft langsamer
                    // // 
                    //                         foreach (var item in batch.Records)
                    //                         {


                    //                             for (int d = 0; d < Config.GetDataDimensionsNr(); d++)
                    //                             {


                    //                                 if (!DictOfDimXSensorIDsToListsFromValueArrays.ContainsKey(item.SensorID * Config.GetDataDimensionsNr() + d))
                    //                                 {
                    //                                     DictOfDimXSensorIDsToListsFromValueArrays[item.SensorID * Config.GetDataDimensionsNr() + d] = new List<double>();
                    //                                 }

                    //                                 IndexOfSensorIDsDimensions[item.SensorID * Config.GetDataDimensionsNr() + d] = new int[2] { item.SensorID, d };
                    //                                 DictOfDimXSensorIDsToListsFromValueArrays[item.SensorID * Config.GetDataDimensionsNr() + d].Add(item.ValuesArray[d]);

                    //                             }

                    //                         }

                    //                         Dictionary<int, List<double>>.ValueCollection val_col = DictOfDimXSensorIDsToListsFromValueArrays.Values;

                    //                         string[] series = new int[Config.GetSensorNumber()].Select(i => "sensor_id_" + i.ToString()).ToArray();

                    //                         vectorContainer = new VectorContainer<double>()
                    //                         {
                    //                             FirstTimestamp = roundedDate,
                    //                             IntervalTicks = 10000 * scale, // second = 10mil
                    //                             LastTimestamp = roundedDate.AddMilliseconds((val_col.First().Count - 1) * scale)
                    //                         };
                    //                         vectorContainer.Vectors = new TimeSeriesVector<double>[val_col.Count()].Select(a => new TimeSeriesVector<double>()).ToArray();

                    //                         foreach (var DimSensorNr in DictOfDimXSensorIDsToListsFromValueArrays.Keys)
                    //                         {
                    //                             vectorContainer.Vectors[DimSensorNr].Directory = GetDirectoryName();
                    //                             vectorContainer.Vectors[DimSensorNr].Series = GetSeriesNames(IndexOfSensorIDsDimensions[DimSensorNr][0], IndexOfSensorIDsDimensions[DimSensorNr][1]);

                    //                             // vectorContainer.Vectors[DimSensorNr].Values = new double[val_col.First().Count]; 

                    //                             vectorContainer.Vectors[DimSensorNr].Values = val_col.Select(a => a.ToArray()).ToArray()[DimSensorNr];

                    //                             if (Config.GetPrintModeEnabled() == true) // Todo remove
                    //                                 for (int i = 0; i < vectorContainer.Vectors[DimSensorNr].Values.GetLength(0); i++)
                    //                                 {
                    //                                     if (vectorContainer.Vectors[DimSensorNr].Values[i] > 0) await Console.Out.WriteLineAsync("write  | " + vectorContainer.Vectors[DimSensorNr].Values[i] + " in  " + vectorContainer.Vectors[DimSensorNr].Series + " at TS: " + roundedDate.AddMilliseconds(i * Config.GetRegularTsScaleMilliseconds()).ToString());
                    //                                 }
                    //                             // }
                    //                         }


                    //                         // TODO to config
                    //                         // string pth = "/tmp/DLTS_SensNb" + Config.GetSensorNumber() + "_Dim" + Config.GetDataDimensionsNr() + "_Date" + Config.GetStartTime().ToFileTimeUtc() + "_Scale" + Config.GetRegularTsScaleMilliseconds() + "_lenght" + val_col.First().Count + ".bin";

                    //                         using (Stream stream = File.Open(pth, FileMode.Create))}
                    //                         {
                    //                             var bin = MemoryPackSerializer.Serialize(vectorContainer);
                    //                             var writoter = new BinaryWriter(stream);
                    //                             writer.Write(bin);
                    //                         }
                    //                         // }
                }
                Stopwatch sw2 = Stopwatch.StartNew();
                await _client.IngestVectorsAsync<double>(vectorContainer, OverwriteMode.older, TimeSeriesCreationTimestampStorageType.NONE, default).ConfigureAwait(false);
                sw2.Stop();

                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw2.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));

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
                string[] series = GetSeriesNames(query.SensorIDs, new int[1] { 0 });
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
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
                var DltsQuery = _iquery.RangeRawAllDims;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);



                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.EndDate, DateTimeKind.Utc);





                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
                
                _aggInterval = 0;
                sw.Stop();
                points = readResult.Vectors.Length * readResult.Vectors.First().Values.Length;
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
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

                var DltsQuery = _iquery.RangeRawLimited;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);



                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * limit);

                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs, new int[1] { 0 });
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawData));
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
                var DltsQuery = _iquery.RangeRawAllDimsLimited;

                DltsQuery.FirstTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc);



                DltsQuery.LastTimestamp = DateTime.SpecifyKind(query.StartDate, DateTimeKind.Utc).AddMilliseconds(Config.GetRegularTsScaleMilliseconds() * limit);





                string dir = GetDirectoryName();
                string[] series = GetSeriesNames(query.SensorIDs);
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = 0;
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

                return new QueryStatusRead(true, points, new PerformanceMetricRead(sw.ElapsedMilliseconds, points, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData));
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to execute Range Query Raw Data. Exception: {0}", ex.ToString()));
                return new QueryStatusRead(false, 0, new PerformanceMetricRead(0, 0, 0, query.StartDate, query.DurationMinutes, _aggInterval, Operation.RangeQueryRawAllDimsData), ex, ex.ToString());
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
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
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
            try   
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
                var readResult = await _client.RetrievePointsAsync<double>(DltsQuery, false, false, default).ConfigureAwait(false);

                points = readResult.Count(); //TODO assert correcness

                _aggInterval = 0;
                sw.Stop();

                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

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
                string[] series = new string[] { GetSeriesNames(query.FirstSensorID)[0], GetSeriesNames(query.SecondSensorID)[0] };
                DltsQuery.Selection.Add(dir, series);

                Stopwatch sw = Stopwatch.StartNew();
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());
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
                var readResult = await _client.RetrieveVectorsAsync<double>(DltsQuery, true, default).ConfigureAwait(false);
                var points = 0;
                points = readResult.Vectors.Length;
                _aggInterval = (int)Config.GetAggregationInterval();
                sw.Stop();
                // await Print(readResult, query.ToString(), Config.GetPrintModeEnabled());

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
          
            return Config.GetPolyDimTableName() + "_in_" + Config.GetRegularTsScaleMilliseconds().ToString() + "_ms_steps";

        }

        private string[] GetSeriesNames(int SensorID)
        {
            int[] AllDim = new int[Config.GetDataDimensionsNr()];
            AllDim = Enumerable.Range(0, Config.GetDataDimensionsNr()).ToArray();

            return GetSeriesNames(new int[1] { SensorID }, AllDim);
        }
        private string GetSeriesNames(int SensorID, int dim)
        {
            return GetSeriesNames(new int[1] { SensorID }, new int[1] { dim }).First();
        }
        private string[] GetSeriesNames(int[] SensorIDs)
        {
            int[] AllDim = new int[Config.GetDataDimensionsNr()];
            AllDim = Enumerable.Range(0, Config.GetDataDimensionsNr()).ToArray();

            return GetSeriesNames(SensorIDs, AllDim);
        }
        
        private string[] GetSeriesNames(int[] SensorIDs, int[] dimensions)
        {
            string[] series;

            series = new String[Config.GetDataDimensionsNr() * Config.GetSensorNumber()+1];
            foreach (int c in SensorIDs)
            {
                foreach (int d in dimensions)
                    series[c * Config.GetDataDimensionsNr() + d] = "sensor_id_" + c + $"_{Constants.Value}_" + d;
            }

            return series.Where(c => c != null).ToArray();
        }
        

    }
}