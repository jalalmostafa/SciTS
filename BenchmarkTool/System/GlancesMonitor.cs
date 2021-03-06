using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkTool.System.Metrics;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace BenchmarkTool.System
{
    public class GlancesMonitor
    {
        private IRestClient _client;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;


        public GlancesMonitor(string baseUrl)
        {
            _client = new RestClient(baseUrl)
                        .UseNewtonsoftJson();
        }

        public Task<Cpu> GetCpuAsync()
        {
            var request = new RestRequest("/api/3/cpu", DataFormat.Json);
            return _client.GetAsync<Cpu>(request, _cancellationTokenSource.Token);
        }

        public Task<List<DiskIO>> GetDiskIOAsync()
        {
            var request = new RestRequest("/api/3/diskio", DataFormat.Json);
            return _client.GetAsync<List<DiskIO>>(request, _cancellationTokenSource.Token);
        }

        public Task<Memory> GetMemoryAsync()
        {
            var request = new RestRequest("/api/3/mem", DataFormat.Json);
            return _client.GetAsync<Memory>(request, _cancellationTokenSource.Token);
        }

        public Task<Swap> GetSwapAsync()
        {
            var request = new RestRequest("/api/3/memswap", DataFormat.Json);
            return _client.GetAsync<Swap>(request, _cancellationTokenSource.Token);
        }

        public Task<List<Network>> GetNetworkAsync()
        {
            var request = new RestRequest("/api/3/network", DataFormat.Json);
            return _client.GetAsync<List<Network>>(request, _cancellationTokenSource.Token);
        }

        public Task<DatabaseProcess> GetDatabaseProcessAsync(int pid)
        {
            var request = new RestRequest($"/api/3/processlist/pid/{pid}", DataFormat.Json);
            return _client.GetAsync<DatabaseProcess>(request, _cancellationTokenSource.Token);
        }

        public async Task<AllMetrics> GetAllAsync(int pid, string nic, string disk)
        {
            var cpuAsync = GetCpuAsync();
            var processAsync = GetDatabaseProcessAsync(pid);
            var diskIOAsync = GetDiskIOAsync();
            var memoryAsync = GetMemoryAsync();
            var networkAsync = GetNetworkAsync();
            var swapAsync = GetSwapAsync();

            var metrics = new AllMetrics()
            {
                Cpu = await cpuAsync,
                DatabaseProcess = await processAsync,
                DiskIO = (await diskIOAsync).Find(d => d.DiskName == disk),
                Memory = await memoryAsync,
                Network = (await networkAsync).Find(n => n.InterfaceName == nic),
                Swap = await swapAsync,
            };

            return metrics;
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}