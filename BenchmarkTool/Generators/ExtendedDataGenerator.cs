using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Api.Domain;

namespace BenchmarkTool.Generators
{
    public class ExtendedDataGenerator : IDataGenerator // does regular , random , single and polydimensional Data
    {
        private Random _rnd = new Random();
        private XorShiftRng _rndX = new XorShiftRng();
        private int timeindex;
        static int _scaleMilliseconds = Config.GetRegularTsScaleMilliseconds();
        private bool InTypeReg;

        public Batch GenerateBatch(int batchSize, int sensorStartId, decimal sensorsPerClient, int offset, int clientOffset, DateTime date) // date is here relative to the number of batches which have been written before and th eTestretries
        {
            if (Config.GetIngestionType() == "regular")
                InTypeReg = true;

            var _Timestamp = date;
            int step = 0;
            RecordFactory recordFactory = new RecordFactory();
            Random rndval = new Random();
            Batch batch = new Batch(batchSize);
            var sensorid = (clientOffset + offset) * batchSize + sensorStartId >= sensorsPerClient + sensorStartId ? sensorStartId : (clientOffset + offset) * batchSize + sensorStartId;
            timeindex = 0;
            for (int k = 0; k < batchSize; k++)
            {
                // var recordTimestamp = GetRecordTimestamp(date, timeindex);
                // batch.Records.Add(recordFactory.Create(sensorid, recordTimestamp, rndval.Next()));

                if (sensorid >= sensorStartId + sensorsPerClient)
                {
                    sensorid = sensorStartId;
                    // timeindex++;
                    if (InTypeReg) { step = _scaleMilliseconds; } else { step = _rnd.Next(_scaleMilliseconds); }
                }
                var recordTimestamp = _Timestamp.AddMilliseconds(step);
                // GetRecordTimestamp(_Timestamp, timeindex, step);
                _Timestamp = recordTimestamp;

                batch.Records.Add(recordFactory.Create(sensorid, recordTimestamp, rndval.Next()));
                sensorid++;
            }
            return batch;
        }

        public Batch GenerateBatch(int batchSize, List<int> sensorIdsForThisClientList, DateTime date, int dimensions) // date is here relative to the number of batches which have been written before and th eTestretries
        {
            if (Config.GetIngestionType() == "regular")
                InTypeReg = true;

            RecordFactory recordFactory = new RecordFactory();
            _rnd = new Random(7839);
            _rndX = new XorShiftRng(3345, 4234);
            Batch batch = new Batch(batchSize);
            var _Timestamp = date;
            int step = 0;


// TODO delete old, wrong
            // // int timeindex = 0;
            // for (int dataPointNr = 0; dataPointNr < batchSize; dataPointNr++)
            // {
            //     var chosenSensor = dataPointNr % sensorIdsForThisClientList.Count;
            //     if (chosenSensor == 0 && dataPointNr != 0)
            //     {
            //         // timeindex++;
            //         if (InTypeReg)
            //         {
            //             step = _scaleMilliseconds; _Timestamp = _Timestamp.AddMilliseconds(step);
            //         }
            //         else
            //         {
            //             step = _rnd.Next(_scaleMilliseconds * 2); // *2 so to have a Apprx median of scaleMs
            //             _Timestamp = _Timestamp.AddMilliseconds(step);
            //         }
            //     }

            //     // GetRecordTimestamp(_Timestamp, timeindex, step);

            //     batch.Records.Add(recordFactory.Create(chosenSensor, _Timestamp, GetInput(dimensions)));
            // }

            int dataPointNr = 0;
            while (dataPointNr < batchSize)
            {
                foreach (var chosenSensor in sensorIdsForThisClientList)
                {

                    if (chosenSensor == sensorIdsForThisClientList.First() && dataPointNr != 0)
                    {
                        // timeindex++;
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


                    batch.Records.Add(recordFactory.Create(chosenSensor, _Timestamp, GetInput(dimensions)));
                    dataPointNr++;
                }
            }





            return batch;
        }
        private double[] GetInput(int dimensions)
        {

            List<double> inputList = new List<double>();
            for (int c = 1; c <= dimensions; c++)
            {
                inputList.Add(_rnd.NextSingle());
            }
            return inputList.ToArray();


        }


        private DateTime GetRecordTimestamp(DateTime baseTime, int timeindex) // possibly deprecated TODO delete
        {
            var on = 0;
            if (timeindex > 0) on = 1;
            if (InTypeReg)
                return baseTime.AddMilliseconds(_scaleMilliseconds * timeindex);
            else
                return baseTime.AddMilliseconds(_scaleMilliseconds * timeindex + on * _rnd.Next(_scaleMilliseconds));


        }


    }
}
