using BenchmarkTool.Database.Queries;

namespace BenchmarkTool.Database
{
    public class TimescaleDB : PostgresDB
    {
        public TimescaleDB() : base(new TimescaleQuery(),
                                    Config.GetTimescaleConnection())
        { }

        // public override void CheckOrCreateTable()
        // {
        //     try
        //     {
        //         var dimNb = 0;
        //         if (_TableCreated != true)
        //         {
        //             if (Config.GetMultiDimensionStorageType() == "column")
        //             {

                            //                 NpgsqlCommand m_createtbl_cmd_DB = new NpgsqlCommand(
    //                                           String.Format("CREATE DATABASE IF NOT EXISTS katrindb2;  ")
    //                                            , _connection);



    //                 m_createtbl_cmd_DB.ExecuteNonQuery();


    // NpgsqlCommand m_createtbl_cmd_TS = new NpgsqlCommand(
    //                                           String.Format("CREATE EXTENSION IF NOT EXISTS timescaledb;")
    //                                            , _connection);



    //                 m_createtbl_cmd_TS.ExecuteNonQuery();


            //             foreach (var tableName in Config.GetAllPolyDimTableNames())
            //             {
            //                 var actualDim = Config.GetDataDimensionsNrOptions()[dimNb];



            //                 int c = 0; StringBuilder builder = new StringBuilder("");


            //                 while (c < actualDim) { builder.Append(", value_" + c + " double precision"); c++; }


            //                 NpgsqlCommand m_createtbl_cmd = new NpgsqlCommand(
            //                   String.Format("CREATE TABLE IF NOT EXISTS " + tableName + " ( time timestamp(6) with time zone NOT NULL, sensor_id integer " + builder + ") ; CREATE INDEX ON " + Config.GetPolyDimTableName() + " ( sensor_id, time DESC); --UNIQUE;  ")
            //                    , _connection);



            //                 m_createtbl_cmd.ExecuteNonQuery();
            //                 _TableCreated = true;

            //                 dimNb++;
            //             }
            //         }
            //         else
            //             throw new NotImplementedException();


            //     }


            // }
            // catch (Exception ex)
            // {
            //     Log.Error(String.Format("Failed to close. Exception: {0}", ex.ToString()));
            // }
        // }

    }
}
