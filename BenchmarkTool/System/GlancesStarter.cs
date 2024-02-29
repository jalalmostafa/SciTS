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
        private SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private Operation _operation;
        private int _clientsNb;
        private int _batchSize;
        private int _sensorsNb;

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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
            _monitor = new GlancesMonitor(_baseUrl);
            _thread = Task.Factory.StartNew(async () => await MonitorAsync(_cancellationTokenSource.Token).ConfigureAwait(false), TaskCreationOptions.LongRunning);

        }

        private async Task MonitorAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var metrics = await _monitor.GetAllAsync(_databasePid, _nic, _disk, _fs);

                    await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);

                    try
                    {
                        _metrics.Add(metrics);
                    }
                    finally
                    {
                        _lock.Release();
                    }

                    await Task.Delay(_period * 1000);
                }
                catch (OperationCanceledException e)
                {
                    // do nothing
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }

            }
        }

        public async Task EndMonitorAsync()
        {

            _cancellationTokenSource.Cancel();
            await _thread.ConfigureAwait(false);
            foreach (var item in _metrics)
            {
                item.WriteToCSV(_path, _operation.ToString(), _clientsNb, _batchSize, _sensorsNb);
            }
            _cancellationTokenSource.Dispose();
            _thread.Dispose();

        }
    }
}
