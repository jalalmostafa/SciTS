using Serilog;
using System;
using System.Threading.Tasks;
using BenchmarkTool.Queries;
using BenchmarkTool.Database;
using System.Collections.Generic;

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

        public ClientRead()
        {
            try
            {
                _date = Config.GetStartTime();
                _daySpan = Config.GetDaySpan();
                _aggInterval = Config.GetAggregationInterval();
                _operation = Config.GetQueryType().ToEnum<Operation>();
                _minutes = Config.GetDurationMinutes();

                var dbFactory = new DatabaseFactory();
                _targetDb = dbFactory.Create(1, 0, 0);
                _targetDb.Init();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public Task<List<QueryStatusRead>> RunQuery()
        {
            int loop = Config.GetTestRetries();
            var sensorsFilter = Config.GetSensorsFilter();
            var sensorsFilterString = Config.GetSensorsFilterString();
            var sensorId = Config.GetSensorID();
            var maxValue = Config.GetMaxValue();
            var minValue = Config.GetMinValue();
            var firstSensorId = Config.GetFirstSensorID();
            var secondSensorId = Config.GetSecondSensorID();
            var statuses = new List<QueryStatusRead>();

            switch (_operation)
            {

                case Operation.RangeQueryRawData:
                    Log.Information("Executing Range Raw Query");
                    for (var i = 0; i < loop; i++)
                    {
                        var startDate = GetRandomDateTime();
                        var query = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = _targetDb.RangeQueryRaw(query);
                        status.Iteration = i;
                        statuses.Add(status);
                    }
                    break;
                case Operation.RangeQueryAggData:
                    Log.Information("Executing Range Aggregared Query");
                    for (int i = 0; i < loop; i++)
                    {
                        var startDate = GetRandomDateTime();
                        var aggQuery = new RangeQuery(startDate, _minutes, sensorsFilter, sensorsFilterString);
                        var status = _targetDb.RangeQueryAgg(aggQuery);
                        status.Iteration = i;
                        statuses.Add(status);
                    }
                    break;
                case Operation.OutOfRangeQuery:
                    Log.Information("Executing Out of Range Query");
                    for (int i = 0; i < loop; i++)
                    {
                        var startDate = GetRandomDateTime();
                        var oorangeQuery = new OORangeQuery(startDate, _minutes, sensorId, maxValue, minValue);
                        var status = _targetDb.OutOfRangeQuery(oorangeQuery);
                        status.Iteration = i;
                        statuses.Add(status);
                    }
                    break;
                case Operation.DifferenceAggQuery:
                    Log.Information("Executing Agg Difference Query");
                    for (int i = 0; i < loop; i++)
                    {
                        var startDate = GetRandomDateTime();
                        var comparisonQuery = new ComparisonQuery(startDate, _minutes, firstSensorId, secondSensorId);
                        var status = _targetDb.AggregatedDifferenceQuery(comparisonQuery);
                        status.Iteration = i;
                        statuses.Add(status);
                    }
                    break;
                case Operation.STDDevQuery:
                    Log.Information("Executing Standard Deviation Query");
                    for (int i = 0; i < loop; i++)
                    {
                        var startDate = GetRandomDateTime();
                        var status = _targetDb.StandardDevQuery(new SpecificQuery(startDate, _minutes, secondSensorId));
                        status.Iteration = i;
                        statuses.Add(status);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            _targetDb.Cleanup();
            _targetDb.Close();
            return Task.FromResult(statuses);
        }

        private DateTime GetRandomDateTime()
        {
            return _date.AddDays(_rnd.Next(_daySpan)).AddHours(_rnd.Next(24));
        }
    }
}
