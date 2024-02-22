using System;
using System.Collections.Generic;
using BenchmarkTool.System.Metrics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BenchmarkTool.System.Converters
{
    public class DatabaseProcessConverter : JsonConverter<DatabaseProcess>
    {
        public DatabaseProcessConverter() : base()
        {
        }

        public override DatabaseProcess ReadJson(JsonReader reader,
                                                Type objectType, DatabaseProcess existingValue,
                                                bool hasExistingValue, JsonSerializer serializer)
        {
            var dict = serializer.Deserialize<Dictionary<string, List<dynamic>>>(reader);
            if (dict.Count != 1)
            {
                throw new ArgumentOutOfRangeException("More than 1 process returned");
            }

            foreach (var kv in dict)
            {
                if (kv.Value.Count != 1)
                {
                    throw new ArgumentOutOfRangeException("Less or more than 1 process information?");
                }

                var data = kv.Value[0];
                var cmdline = serializer.Deserialize<List<string>>((data["cmdline"] as JArray).CreateReader());
                var cpuTimes = serializer.Deserialize<List<double>>((data["cpu_times"] as JArray).CreateReader());
                var gids = serializer.Deserialize<List<int?>>((data["gids"] as JArray).CreateReader());
                var memInfo = serializer.Deserialize<List<long>>((data["memory_info"] as JArray).CreateReader());
                var ioCounters = serializer.Deserialize<List<long>>((data["io_counters"] as JArray).CreateReader());

                return new DatabaseProcess()
                {
                    CmdLine = cmdline,
                    CpuPercent = data["cpu_percent"],
                    CpuTimes = cpuTimes,
                    Gids = gids,
                    IOCounters = ioCounters,
                    MemoryInfo = memInfo,
                    MemoryPercent = data["memory_percent"],
                    Name = data["name"],
                    Nice = data["nice"],
                    ThreadsNumber = data["num_threads"],
                    Pid = data["pid"],
                    PPid = data["ppid"],
                    Status = data["status"],
                    Username = data["username"],
                };
            }

            return null;
        }

        public override void WriteJson(JsonWriter writer, DatabaseProcess value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
