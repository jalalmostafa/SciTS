using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record Swap
    {
        [JsonProperty("free")]
        public long Free { get; set; }

        [JsonProperty("percent")]
        public double Percent { get; set; }

        [JsonProperty("sin")]
        public long Sin { get; set; }

        [JsonProperty("sout")]
        public long Sout { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("used")]
        public long Used { get; set; }
    }
}