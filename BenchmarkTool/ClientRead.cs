using Serilog;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using BenchmarkTool.Queries;
using BenchmarkTool.Database;
using System.Collections.Generic;
using BenchmarkTool;

namespace BenchmarkTool
{
    public class ClientRead
    {
        private DateTime _date;
        private IDatabase _targetDb;
        private int _aggInterval;
        private Operation _operation;
        private long _minutes;
        private int _daySpan;
        private Random _rnd = new Random();
                public int _chosenClientIndex { get; private set; }


        public ClientRead()
        {
            try
            {

                Thread.CurrentThread.CurrentCulture =  CultureInfo.InvariantCulture;
                
                _date = Config.GetStartTime();
                _daySpan = Config.GetDaySpan();
                _aggInterval = Config.GetAggregationInterval();
                _operation = Config.QueryTypeOnRunTime.ToEnum<Operation>();
                _minutes = Config.GetDurationMinutes();

                var dbFactory = new DatabaseFactory();
                _targetDb = dbFactory.Create();
                _targetDb.Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }


        public async Task<List<QueryStatusRead>> RunQuery(int TestRetryReadIteration,int chosenClientIndex, int limit)
        {
            var sensorsFilter = Config.GetSensorsFilter();
            var sensorsFilterString = Config.GetSensorsFilterString();
            var sensorId = Config.GetSensorID();
            var maxValue = Config.GetMaxValue();
            var minValue = Config.GetMinValue();
            var firstSensorId = Config.GetFirstSensorID();
            var secondSensorId = Config.GetSecondSensorID();
            var statuses = new List<QueryStatusRead>();
                            _chosenClientIndex = chosenClientIndex;


            switch (_operation)
            {
                                case Operation.RangeQueryRawData:
                    {
                        Log.Information("Executing Range Raw Query");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = await _targetDb.RangeQueryRaw(query );
                        status.Iteration = TestRetryReadIteration;
                         status.Client = _chosenClientIndex;
                         status.StartDate=startDate;
                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryRawAllDimsData:
                    {
                        Log.Information("Executing Range Raw Query");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = await _targetDb.RangeQueryRawAllDims(query );
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;

                case Operation.RangeQueryRawLimitedData:
                    {
                        Log.Information("Executing Range Raw Query Limited");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = await _targetDb.RangeQueryRawLimited(query, limit);
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Out-of{limit}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryRawAllDimsLimitedData:
                    {
                        Log.Information("Executing Range Raw Query Limited");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = await _targetDb.RangeQueryRawAllDimsLimited(query, limit);
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Out-of{limit}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryAggData:
                    {
                        Log.Information("Executing Range Aggregared Query");
                        var startDate = GetRandomDateTime();
                        var aggQuery = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = await _targetDb.RangeQueryAgg(aggQuery);
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.OutOfRangeQuery:
                    {
                        Log.Information("Executing Out of Range Query");
                        var startDate = GetRandomDateTime();
                        var oorangeQuery = new OORangeQuery(startDate, _minutes, sensorId, maxValue, minValue);
                        var status = await _targetDb.OutOfRangeQuery(oorangeQuery);
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.DifferenceAggQuery:
                    {
                        Log.Information("Executing Agg Difference Query");
                        var startDate = GetRandomDateTime();
                        var comparisonQuery = new ComparisonQuery(startDate, _minutes, firstSensorId, secondSensorId);
                        var status = await _targetDb.AggregatedDifferenceQuery(comparisonQuery);
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.STDDevQuery:
                    {
                        Log.Information("Executing Standard Deviation Query");
                        var startDate = GetRandomDateTime();
                        var status = await _targetDb.StandardDevQuery(new SpecificQuery(startDate, _minutes, secondSensorId));
                        status.Iteration = TestRetryReadIteration;
                                                 status.Client = _chosenClientIndex;
                         status.StartDate=startDate;

                        statuses.Add(status);
                        Console.WriteLine($"[Succeded:{status.DataPoints}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            _targetDb.Cleanup();
            _targetDb.Close();

            return statuses;
        }

        private DateTime GetRandomDateTime()
        {
            return _date.AddDays(_rnd.Next(_daySpan-1)).AddHours(_rnd.Next(24));
        }
    }
}
