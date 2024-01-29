using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record Memory
    {
        [JsonProperty("active")]
        public long Active { get; set; }

        [JsonProperty("available")]
        public long Available { get; set; }

        [JsonProperty("buffers")]
        public long Buffers { get; set; }

        [JsonProperty("cached")]
        public long Cached { get; set; }

        [JsonProperty("free")]
        public long Free { get; set; }

        [JsonProperty("inactive")]
        public long InActive { get; set; }

        [JsonProperty("percent")]
        public double Percent { get; set; }

        [JsonProperty("shared")]
        public long Shared { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("used")]
        public long Used { get; set; }
    }
}