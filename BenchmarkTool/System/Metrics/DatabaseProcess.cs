using System.Collections.Generic;
using BenchmarkTool.System.Converters;
using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    [JsonConverter(typeof(DatabaseProcessConverter))]
    public record DatabaseProcess
    {
        [JsonProperty("cmdline")]
        public List<string> CmdLine { get; set; }

        [JsonProperty("cpu_percent")]
        public double CpuPercent { get; set; }

        [JsonProperty("cpu_times")]
        public List<double> CpuTimes { get; set; }

        [JsonProperty("gids")]
        public List<int> Gids { get; set; }

        [JsonProperty("io_counters")]
        public List<long> IOCounters { get; set; }

        [JsonProperty("memory_info")]
        public List<long> MemoryInfo { get; set; }

        [JsonProperty("memory_percent")]
        public double MemoryPercent { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("nice")]
        public int Nice { get; set; }

        [JsonProperty("num_threads")]
        public int ThreadsNumber { get; set; }

        [JsonProperty("pid")]
        public int Pid { get; set; }

        [JsonProperty("ppid")]
        public int PPid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }
    }
}