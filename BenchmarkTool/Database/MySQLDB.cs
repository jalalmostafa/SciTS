﻿using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Generators;
using System.Diagnostics;

namespace BenchmarkTool.Database
{
    public class MySQLDB : IDatabase
    {
        private MySqlConnection _connection;
        private static bool _TableCreated;

        public void Cleanup()
        {
            throw new NotImplementedException();
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
                Serilog.Log.Error(String.Format("Failed to close MySQL. Exception: {0}", ex.ToString()));
            }
        }

        public void CheckOrCreateTable()
        {
            try
            {

                var dimNb = 0;
                if (_TableCreated != true)
                {
                    if (Config.GetMultiDimensionStorageType() == "column")
                    {
                        foreach (var tableName in Config.GetAllPolyDimTableNames())
                        {
                            var actualDim = Config.GetDataDimensionsNrOptions()[dimNb];

                            int c = 0; StringBuilder builder = new StringBuilder("");

                            while (c < actualDim) { builder.Append(", value_" + c + " double precision"); c++; }

                            var cmd = _connection.CreateCommand().CommandText = String.Format("CREATE TABLE IF NOT EXISTS " + tableName + " ( time timestamp(6) with time zone NOT NULL, sensor_id integer " + builder + ") ; CREATE INDEX ON " + Config.GetPolyDimTableName() + " ( sensor_id, time DESC); --UNIQUE;  ");

                            _connection.CreateCommand().ExecuteNonQuery();
                            _TableCreated = true;

                            dimNb++;
                        }
                    }
                    else
                        throw new NotImplementedException();

                }

            }
            catch (Exception ex)
            {
                Serilog.Log.Error(String.Format("Failed to create table. Exception: {0}", ex.ToString()));
            }
        }
        public void Init()
        {
            try
            {
                _connection = new MySqlConnection(Config.GetMySQLConnection());
                _connection.Open();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(String.Format("Failed to create table. Exception: {0}", ex.ToString()));
            }
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
        public Task<QueryStatusRead> RangeQueryRawAllDims(RangeQuery rangeQuery)
        {
            throw new NotImplementedException();
        }
        public Task<QueryStatusRead> RangeQueryRawLimited(RangeQuery rangeQuery, int limit)
        {
            throw new NotImplementedException();
        }
        public Task<QueryStatusRead> RangeQueryRawAllDimsLimited(RangeQuery rangeQuery, int limti)
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

        public Task<QueryStatusWrite> WriteBatch(Batch batch)
        {
            try
            {
                StringBuilder sCommand = new StringBuilder("INSERT INTO sensor_data (`time`, sensor_id, `value`) VALUES " + batch.RecordsArray);
                sCommand.Append(";");
                Stopwatch sw = new Stopwatch();
                using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), _connection))
                {
                    myCmd.CommandType = CommandType.Text;
                    sw.Start();
                    myCmd.ExecuteNonQuery();
                    sw.Stop();
                }

                return Task.FromResult(new QueryStatusWrite(true, new PerformanceMetricWrite(sw.Elapsed.TotalMicroseconds, batch.Size, 0, Operation.BatchIngestion)));
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(String.Format("Failed to insert batch into MySQL. Exception: {0}", ex.ToString()));
                return Task.FromResult(new QueryStatusWrite(false, 0, new PerformanceMetricWrite(0, 0, batch.Size, Operation.BatchIngestion), ex, ex.ToString()));
            }
        }

    }
}
