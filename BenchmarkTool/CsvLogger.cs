using System;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using Serilog;

namespace BenchmarkTool
{
    public class CsvLogger<T> : IDisposable
    {
        
        private StreamWriter _stream;
        private CsvWriter _writer;
        bool exists;
        

        public CsvLogger(string operation)
        {
            try
            { 
                if( operation == "read"){
                    var path = Config.GetMetricsCSVPath()+"Read.csv";
                     exists = File.Exists(path);
                    _stream = new StreamWriter(path, true, new UTF8Encoding(true));
                    _writer = new CsvWriter(_stream, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
                }else{
                    var path = Config.GetMetricsCSVPath()+"Write.csv";
                     exists = File.Exists(path);
                    _stream = new StreamWriter(path, true, new UTF8Encoding(true));
                    _writer = new CsvWriter(_stream, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture));
                 
                }


                if (!exists)
                {
                     _writer.WriteHeader<T>(); 
                    _writer.NextRecord();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public void WriteRecord(T record)
        {
                         _writer.WriteRecord<T>(record);
            _writer.NextRecord();
        }

        public void Flush()
        {
            _stream.Flush();
        }

        public void Dispose()
        {

            if (_writer != null)
            {
                _writer.Dispose();
            }

            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }
        }
    }
}