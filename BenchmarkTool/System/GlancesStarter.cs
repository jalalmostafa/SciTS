using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using BenchmarkTool.System.Metrics;

namespace BenchmarkTool.System
{
    public class GlancesStarter
    {
        private Task _thread;
        private string _baseUrl;
        private int _databasePid;
        private int _period;
        private string _nic;
                private string _fs;
        private string _disk;
        private string _path;
        private GlancesMonitor _monitor;
        private List<AllMetrics> _metrics = new List<AllMetrics>();
        private Mutex _mutex = new Mutex();
        private Operation _operation;
        private int _clientsNb;
        private int _batchSize;
        private int _sensorsNb;

        public GlancesStarter(Operation operation, int clientsNb, int batchSize, int sensorsNb)
        {
            _baseUrl = Config.GetGlancesUrl();
            _databasePid = Config.GetGlancesDatabasePid();
            _period = Config.GetGlancesPeriod();
            _nic = Config.GetGlancesNIC();
            _disk = Config.GetGlancesDisk();
            _path = Config.GetGlancesOutput();
            _operation = operation;
            _clientsNb = clientsNb;
            _batchSize = batchSize;
            _sensorsNb = sensorsNb;
            _fs = Config.GetGlancesStorageFileSystem();

            _thread = new Task(Monitor, TaskCreationOptions.LongRunning);
            _monitor = new GlancesMonitor(_baseUrl);
        }

        private async void Monitor()
        {
            while (!_monitor.CancellationToken.IsCancellationRequested)
            {
                try 
                {
                    var metrics = await _monitor.GetAllAsync(_databasePid, _nic, _disk, _fs);
                    _mutex.WaitOne(); // TODO understand why handly geclosed wird
                    _metrics.Add(metrics);
                    _mutex.ReleaseMutex();
                    await Task.Delay(_period * 1000);
                }
                catch (Exception e) 
                {
                    Log.Error(e.ToString());
                }
            }
        }

        public void BeginMonitor()
        {
            _thread.Start();
        }

        public void Commit()
        {
            _mutex.WaitOne();
            foreach (var item in _metrics)
            {
                item.WriteToCSV(_path, _operation.ToString(), _clientsNb, _batchSize, _sensorsNb);
            }
            _metrics.Clear();
            _mutex.ReleaseMutex();
        }

        public void EndMonitor()
        {
            Commit();
            _monitor.Cancel();
            _thread.Wait();
            _mutex.Close();
        }
    }
}
