using System;
using System.Collections.Generic;
using System.Linq;
using DataLayerTS;
using DataLayerTS.Models;

using BenchmarkTool.Queries;

namespace BenchmarkTool.Database.Queries
{
    public class DatalayertsQuery : IQuery<ContainerRequest>
    {

        public ContainerRequest RangeRaw => new ContainerRequest()
        {
            Selection = new Dictionary<string, string[]>(),
        };

        public ContainerRequest RangeAgg => new ContainerRequest()
        {
            Selection = new Dictionary<string, string[]>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE,
                    Aggregations = new AggregationType[] { AggregationType.MEAN },
                    IntervalTicks = Config.GetAggregationInterval()* 36000000000 ,
                    },
                  }
        };

        public ContainerRequest OutOfRange => new ContainerRequest()
        {
            Selection = new Dictionary<string, string[]>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.FILTER, },
                },


        };

        public ContainerRequest StdDev => new ContainerRequest()
        {
            Selection = new Dictionary<string, string[]>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE,
                    Aggregations = new AggregationType[] { AggregationType.STD },
                    }
                },
        };

        public ContainerRequest AggDifference => new ContainerRequest()
        {
            Selection = new Dictionary<string, string[]>(),
            Transformations = new TransformationRequest[] {
                new TransformationRequest(){
                    Function = FunctionType.RESAMPLE, 
                    Aggregations = new AggregationType[] { AggregationType.DIF },
                    IntervalTicks = Config.GetAggregationInterval()* 36000000000 ,

                    }
                },
        };

    }

}