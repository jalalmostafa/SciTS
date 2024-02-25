using Serilog;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;
using BenchmarkTool.Queries;
using BenchmarkTool.Database;
using System.Collections.Generic;
using System.Diagnostics;

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

                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

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

        public async Task<QueryStatusRead> RunQuery(int TestRetryReadIteration, int chosenClientIndex, int limit)
        {
            
            var sensorsFilter = Config.GetSensorsFilter();
            var sensorId = Config.GetSensorID();
            var maxValue = Config.GetMaxValue();
            var minValue = Config.GetMinValue();
            var firstSensorId = Config.GetFirstSensorID();
            var secondSensorId = Config.GetSecondSensorID();
            QueryStatusRead status;
            _chosenClientIndex = chosenClientIndex;
            Stopwatch swC = Stopwatch.StartNew();

            switch (_operation)
            {
                case Operation.RangeQueryRawData:
                    {
                        Log.Information("Executing Range Raw Query");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes,  sensorsFilter);
                        status = await _targetDb.RangeQueryRaw(query);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryRawAllDimsData:
                    {
                        Log.Information("Executing Range Raw Query");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes,  sensorsFilter);
                        status = await _targetDb.RangeQueryRawAllDims(query);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;

                case Operation.RangeQueryRawLimitedData:
                    {
                        Log.Information("Executing Range Raw Query Limited");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes,  sensorsFilter);
                        status = await _targetDb.RangeQueryRawLimited(query, limit);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Limit:{limit}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryRawAllDimsLimitedData:
                    {
                        Log.Information("Executing Range Raw Query Limited");
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes,  sensorsFilter);
                        status = await _targetDb.RangeQueryRawAllDimsLimited(query, limit);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Limit{limit}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.RangeQueryAggData:
                    {
                        Log.Information("Executing Range Aggregared Query");
                        var startDate = GetRandomDateTime();
                        var aggQuery = new RangeQuery(startDate, _minutes,  sensorsFilter);
                        status = await _targetDb.RangeQueryAgg(aggQuery);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.OutOfRangeQuery:
                    {
                        Log.Information("Executing Out of Range Query");
                        var startDate = GetRandomDateTime();
                        var oorangeQuery = new OORangeQuery(startDate, _minutes, sensorId, maxValue, minValue);
                        status = await _targetDb.OutOfRangeQuery(oorangeQuery);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()} [ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.DifferenceAggQuery:
                    {
                        Log.Information("Executing Agg Difference Query");
                        var startDate = GetRandomDateTime();
                        var comparisonQuery = new ComparisonQuery(startDate, _minutes, firstSensorId, secondSensorId);
                        status = await _targetDb.AggregatedDifferenceQuery(comparisonQuery);
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;
                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()}[ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                case Operation.STDDevQuery:
                    {
                        Log.Information("Executing Standard Deviation Query");
                        var startDate = GetRandomDateTime();
                        status = await _targetDb.StandardDevQuery(new SpecificQuery(startDate, _minutes, secondSensorId));
                        status.Iteration = TestRetryReadIteration;
                        status.Client = _chosenClientIndex;
                        status.StartDate = startDate;

                        swC.Stop();
                        status.PerformanceMetric.ClientLatency = swC.ElapsedMilliseconds;
                        Console.WriteLine($"[Succeded:{status.Succeeded}-Iteration:{TestRetryReadIteration}-date: {startDate} ,min: {_minutes} ] {BenchmarkTool.Program.Mode}-{Config._actualMixedWLPercentage}% - {Config.GetTargetDatabase()}[ ClientsNR:{BenchmarkTool.Program._currentReadClientsNR} -  {_operation.ToString()} -  with Dimensions:{status.PerformanceMetric.DimensionsNb}] Latency:{status.PerformanceMetric.Latency}");

                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            _targetDb.Cleanup();
            _targetDb.Close();

            return status;
        }

        private DateTime GetRandomDateTime()
        {
            return _date.AddDays(_rnd.Next(_daySpan - 1)).AddHours(_rnd.Next(24));
        }
    }
}
