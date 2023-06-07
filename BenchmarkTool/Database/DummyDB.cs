﻿using MySqlConnector;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;
using System.Globalization;

namespace BenchmarkTool.Database
{
    public class DummyDB : IDatabase
    {

        public void Cleanup()
        {
        }

        public void Close()
        {

        }

        public void Init()
        {

        }

        public Task<QueryStatusRead> OutOfRangeQuery(OORangeQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> RangeQueryAgg(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> RangeQueryRaw(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> StandardDevQuery(SpecificQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusRead> AggregatedDifferenceQuery(ComparisonQuery query)
        {
            throw new NotImplementedException();
        }

        public Task<QueryStatusWrite> WriteRecord(IRecord record)
        {
            throw new NotImplementedException();
        }


        public async Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                StringBuilder sCommand; // TODO supstitute with Binary insert
                List<string> Rows = new List<string>();
                if (Config.GetMultiDimensionStorageType() == "column")
                {
                    int c = 1; StringBuilder builder = new StringBuilder("");
                    while (c < Config.GetDataDimensionsNr()) { builder.Append(", value_dim" + (c + 1)); c++; }
                    sCommand = new StringBuilder("INSERT INTO sensor_data (`time`, sensor_id, `value_dim1`" + builder + ") VALUES "  );

                    foreach (var record in batch.Records)
                        {
                            Rows.Add(string.Format("('{0}',{1},{2})", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.SensorID, string.Join(",", record.ValuesArray.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray())));
                        }   sCommand.AppendLine(string.Join(",", Rows));
                }
                else{
                    sCommand = new StringBuilder("INSERT INTO sensor_data (`time`, sensor_id, `value`) VALUES "  );
                    foreach (var record in batch.Records)
                        {
                            Rows.Add(string.Format("('{0}',{1},{2})", record.Time.ToString("yyyy-MM-dd HH:mm:ss"), record.SensorID, "{"+string.Join(",", record.ValuesArray.Select(x => x.ToString(CultureInfo.InvariantCulture)).ToArray()) +"}"));
                        }  sCommand.AppendLine(string.Join(",", Rows));
                }     

                sCommand.Append(";");


                Stopwatch sw = new Stopwatch();


                sw.Start();
                await File.AppendAllTextAsync("/tmp/dummy_" + DateTime.Now.Day.ToString() + ".txt", sCommand.ToString());
                sw.Stop();


                return new QueryStatusWrite(true, new PerformanceMetricWrite(sw.ElapsedMilliseconds, batch.Size, 0, Operation.BatchIngestion));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(String.Format("Failed to insert batch into MySQL. Exception: {0}", ex.ToString()));
                return new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString());
            }
        }

    }
}