using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record Network
    {
        [JsonProperty("cumulative_cx")]
        public long CumulativeConnections { get; set; }

        [JsonProperty("cumulative_rx")]
        public long CumulativeReceives { get; set; }

        [JsonProperty("cumulative_tx")]
        public long CumulativeTransmissions { get; set; }

        [JsonProperty("cx")]
        public long Connections { get; set; }

        [JsonProperty("interface_name")]
        public string InterfaceName { get; set; }

        [JsonProperty("is_up")]
        public bool IsUp { get; set; }

        [JsonProperty("rx")]
        public long Receives { get; set; }

        [JsonProperty("speed")]
        public double Speed { get; set; }

        [JsonProperty("tx")]
        public long Transmissions { get; set; }

    }
}