using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record FS
    {
        [JsonProperty("device_name")]
        public string DeviceName { get; set; }

        [JsonProperty("free")]
        public long Free { get; set; }

        [JsonProperty("fs_type")]
        public string FS_Type { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("mnt_point")]
        public string Mnt_Point { get; set; }

        [JsonProperty("percent")]
        public double Percent { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("usage")]
        public long Usage { get; set; }
    }
}

