using Newtonsoft.Json;

namespace BenchmarkTool.System.Metrics
{
    public record Cpu
    {
        [JsonProperty("cpucore")]
        public int CpuCore { get; set; }

        [JsonProperty("ctx_switches")]
        public long ContextSwitches { get; set; }

        [JsonProperty("guest")]
        public double Guest { get; set; }

        [JsonProperty("guest_nice")]
        public double GuestNice { get; set; }

        [JsonProperty("idle")]
        public double Idle { get; set; }

        [JsonProperty("interrupts")]
        public long Interrupts { get; set; }

        [JsonProperty("iowait")]
        public double IOWait { get; set; }

        [JsonProperty("irq")]
        public double Irq { get; set; }

        [JsonProperty("nice")]
        public double Nice { get; set; }

        [JsonProperty("soft_interrupts")]
        public long SoftInterrupts { get; set; }

        [JsonProperty("softirq")]
        public double SoftIrq { get; set; }

        [JsonProperty("steal")]
        public double Steal { get; set; }

        [JsonProperty("syscalls")]
        public long SysCalls { get; set; }

        [JsonProperty("system")]
        public double System { get; set; }

        [JsonProperty("total")]
        public double Total { get; set; }

        [JsonProperty("user")]
        public double User { get; set; }
    }
}