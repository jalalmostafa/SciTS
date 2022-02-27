using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record DiskIO
    {
        [JsonProperty("disk_name")]
        public string DiskName { get; init; }

        [JsonProperty("read_bytes")]
        public double ReadBytes { get; init; }

        [JsonProperty("read_count")]
        public long ReadCount { get; init; }

        [JsonProperty("write_bytes")]
        public double WriteBytes { get; init; }

        [JsonProperty("write_count")]
        public long WriteCount { get; init; }
    }
}