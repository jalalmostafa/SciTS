using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkTool.System.Metrics;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Newtonsoft.Json;

namespace BenchmarkTool.System
{
    public class GlancesMonitor
    {
        private IRestClient _client;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;


        public GlancesMonitor(string baseUrl)
        {
            _client = new RestClient( new RestClientOptions( baseUrl) , configureSerialization: s => s.UseNewtonsoftJson()  );
            
            //  , new ConfigureSerialization(    s => s.UseSerializer(() =>  new UseNewtonsoftJson()    ) )   )  ;  // nwetonsoftJson? TODO
            //         //   .UseNewtonsoftJson();

        }

        public async Task<Cpu> GetCpuAsync()
        {
            var request = new RestRequest("/api/3/cpu");
            return await _client.GetAsync<Cpu>(request,  _cancellationTokenSource.Token);
        }

        public async Task<List<DiskIO>> GetDiskIOAsync() // TODO returns empty list without names
        {
            var request = new RestRequest("/api/3/diskio");
            return await _client.GetAsync<List<DiskIO>>(request, _cancellationTokenSource.Token); 
        } 

        public async Task<Memory> GetMemoryAsync()
        {
            var request = new RestRequest("/api/3/mem");
            return await _client.GetAsync<Memory>(request, _cancellationTokenSource.Token);
        }

        public async Task<Swap> GetSwapAsync()
        {
            var request = new RestRequest("/api/3/memswap");
            return await _client.GetAsync<Swap>(request, _cancellationTokenSource.Token);
        }

        public async Task<List<Network>> GetNetworkAsync() 
        {
            var request = new RestRequest("/api/3/network");
            return await _client.GetAsync<List<Network>>(request, _cancellationTokenSource.Token);
        }
        
        public async Task<List<FS>> GetFSAsync()  
        {
            var request = new RestRequest("/api/3/fs");
            return await _client.GetAsync<List<FS>>(request, _cancellationTokenSource.Token);
        }

        public async Task<DatabaseProcess> GetDatabaseProcessAsync(int pid)
        {
            var request = new RestRequest($"/api/3/processlist/pid/{pid}");
            return await _client.GetAsync<DatabaseProcess>(request, _cancellationTokenSource.Token);
        }
         public async Task<AllMetrics> GetAllAsync(int pid, string nic, string disk, string fs)
        {
            var cpuAsync = GetCpuAsync();
            var processAsync = GetDatabaseProcessAsync(pid);
            var diskIOAsync = GetDiskIOAsync();
            var memoryAsync = GetMemoryAsync();
            var networkAsync = GetNetworkAsync();
            var swapAsync = GetSwapAsync();
            var fsAsync = GetFSAsync();

            var metrics = new AllMetrics() //TODO How can i continue executing the task although some tasks are cancelled?
            {  
                Cpu = await cpuAsync,
                DatabaseProcess = await processAsync,
                DiskIO = (await diskIOAsync).Find(d => d.DiskName == disk),
                Memory = await memoryAsync,
                Network = (await networkAsync).Find(n => n.InterfaceName == nic),
                FS = (await fsAsync).Find(f => f.DeviceName == "/dev/"+ disk && f.Mnt_Point == fs), 
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