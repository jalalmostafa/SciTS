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

        public CsvLogger()
        {
            try
            {
                var path = Config.GetMetricsCSVPath();
                bool exists = File.Exists(path);
                _stream = new StreamWriter(path, true, new UTF8Encoding(true));
                _writer = new CsvWriter(_stream, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture));

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