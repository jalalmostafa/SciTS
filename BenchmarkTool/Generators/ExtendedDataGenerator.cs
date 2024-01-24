using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Internal;

namespace BenchmarkTool.Generators
{
    public class ExtendedDataGenerator : IDataGenerator // does regular , random , single and polydimensional Data
    {
        
        private Random _rnd = new Random();
        private XorShiftRng _rndX = new XorShiftRng();
        private int timeindex;
        static int _scaleMilliseconds = Config.GetRegularTsScaleMilliseconds();
        private bool InTypeReg;

        public Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date) // OLD // date is here relative to the number of batches which have been written before and th eTestretries
        {
            if (Config.GetIngestionType() == "regular")
                InTypeReg = true;

            var _Timestamp = date;
            int step = 0;
            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random();
            Batch batch = new Batch(); batch.Size = batchSize;
            var sensorid = (clientOffset + offset) * batchSize + sensorStartId >= sensorsPerClient + sensorStartId ? sensorStartId : (clientOffset + offset) * batchSize + sensorStartId;
            timeindex = 0;
            for (int k = 0; k < batchSize; k++)
            {

                if (sensorid >= sensorStartId + sensorsPerClient)
                {
                    sensorid = sensorStartId;
                    if (InTypeReg) { step = _scaleMilliseconds; } else { step = _rnd.Next(_scaleMilliseconds); }
                }
                var recordTimestamp = _Timestamp.AddMilliseconds(step);
                _Timestamp = recordTimestamp;

                batch.RecordsList.Add(recordFactory.Create(sensorid, recordTimestamp, rndval.Next()));
                sensorid++;
            }
            return batch;
        }

        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions) // CURRENT // date is here relative to the number of batches which have been written before and th eTestretries
        {
            if (Config.GetIngestionType() == "regular")
                InTypeReg = true;

            RecordFactory recordFactory = new RecordFactory();
            _rnd = new Random(7839);
            _rndX = new XorShiftRng(3345, 4234);
            var _Timestamp = date;
            int step = 0;

            Batch batch = new Batch(batchSize);


            int dataPointNr = 0;
            int index = 0;
            while (dataPointNr < batchSize)
            {
                foreach (var chosenSensor in sensorIdsForThisClientList)
                {
                    if (dataPointNr < batchSize)
                    {

                        if (chosenSensor == sensorIdsForThisClientList.First() && dataPointNr != 0)
                        {
                            if (InTypeReg)
                            {
                                step = _scaleMilliseconds; _Timestamp = _Timestamp.AddMilliseconds(step);
                            }
                            else
                            {
                                step = _rnd.Next(_scaleMilliseconds * 2); // *2 so to have a Apprx median of scaleMs
                                _Timestamp = _Timestamp.AddMilliseconds(step);
                            }
                        }

                        batch.RecordsArray[index] = recordFactory.Create(chosenSensor, _Timestamp, GetInput(dimensions));

                        index++;
                        dataPointNr++;
                    }
                }
            }




            return batch;
        }
        private double[] GetInput(int dimensions)
        {

            double[] inputArray = new double[dimensions];



            for (int c = 0; c < dimensions; c++)
            {
                inputArray[c] = _rnd.NextDouble();
            }

            return inputArray;



        }


   


    }
}
