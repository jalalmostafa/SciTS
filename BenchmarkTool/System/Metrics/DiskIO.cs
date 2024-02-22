using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record DiskIO
    {
        [JsonProperty("disk_name")]
        public string DiskName { get; set; }

        [JsonProperty("read_bytes")]
        public double ReadBytes { get; set; }

        [JsonProperty("read_count")]
        public long ReadCount { get; set; }

        [JsonProperty("write_bytes")]
        public double WriteBytes { get; set; }

        [JsonProperty("write_count")]
        public long WriteCount { get; set; }
    }
}