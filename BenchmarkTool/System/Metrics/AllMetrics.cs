using System;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using Serilog;
using  BenchmarkTool;
using Microsoft.Net.Http.Headers;

namespace BenchmarkTool.System.Metrics
{
    public record AllMetrics
    {
        public Cpu Cpu { get; init; }

        public DatabaseProcess DatabaseProcess { get; init; }

        public DiskIO DiskIO { get; init; }

        public FS FS { get; init; }

        public Memory Memory { get; init; }

        public Network Network { get; init; }

        public Swap Swap { get; init; }

        private string _database = Config.GetTargetDatabase();
        private int _dimensions = Config.GetDataDimensionsNr();

        public void WriteToCSV(string path, string operation, int clientsNb, int batchSize, int sensorsNb)
        {
            try
            {
                bool exists = File.Exists(path);
                using (StreamWriter sw = new StreamWriter(path, true, new UTF8Encoding(true)))
                {
                    using (CsvWriter cw = new CsvWriter(sw, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)))
                    {
                        if (!exists)
                        {
                            cw.WriteHeader<GlancesRecord>();
                            cw.NextRecord();
                        }

                        var record = ToRecord(operation, clientsNb, batchSize, sensorsNb);
                        cw.WriteRecord<GlancesRecord>(record);
                        cw.NextRecord();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        public GlancesRecord ToRecord(string operation, int clientsNb, int batchSize, int sensorsNb)
        {
            return new GlancesRecord()
            {
                Timestamp = Helper.GetMilliEpoch(),

                Database = _database,
                Operation = operation,
                ClientsNumber = clientsNb,
                BatchSize = batchSize,
                SensorsNumber = sensorsNb,
                Mode = BenchmarkTool.Program.Mode,
                Dimensions = _dimensions,
                Percentage = Config._actualMixedWLPercentage,  
                // Cpu
                CpuTotal = this.Cpu.Total,
                CpuSystem = this.Cpu.System,
                CpuUser = this.Cpu.User,
                CpuIOWait = this.Cpu.IOWait,
                CpuContextSwitches = this.Cpu.ContextSwitches,
                CpuSysCalls = this.Cpu.SysCalls,

                // Mem
                MemActive = this.Memory.Active,
                MemAvailable = this.Memory.Available,
                MemBuffers = this.Memory.Buffers,
                MemCached = this.Memory.Cached,
                MemFree = this.Memory.Free,
                MemInActive = this.Memory.InActive,
                MemShared = this.Memory.Shared,
                MemTotal = this.Memory.Total,
                MemUsed = this.Memory.Used,

                // Swap
                SwapFree = this.Swap.Free,
                SwapSin = this.Swap.Sin,
                SwapSout = this.Swap.Sout,
                SwapTotal = this.Swap.Total,
                SwapUsed = this.Swap.Used,

                // DiskIO
                DiskReadBytes = this.DiskIO.ReadBytes,
                DiskReadCount = this.DiskIO.ReadCount,
                DiskWriteBytes = this.DiskIO.WriteBytes,
                DiskWriteCount = this.DiskIO.WriteCount,

                // FS
                FsDeviceName = this.FS.DeviceName,
                FsFree = this.FS.Free,
                FsFs_Type = this.FS.FS_Type,
                FsKey = this.FS.Key,
                FsMnt_Point = this.FS.Mnt_Point,
                FsPercent = this.FS.Percent,
                FsSize = this.FS.Size,
                FsUsage = this.FS.Usage,

                // Network
                NetworkCumulativeConnections = this.Network.CumulativeConnections,
                NetworkCumulativeReceives = this.Network.CumulativeReceives,
                NetworkCumulativeTransmissions = this.Network.CumulativeTransmissions,
                NetworkConnections = this.Network.Connections,
                NetworkTransmissions = this.Network.Transmissions,
                NetworkReceives = this.Network.Receives,
                NetworkIsUp = this.Network.IsUp,
                NetworkSpeed = this.Network.Speed,

                // Process
                ProcessCpuPercent = this.DatabaseProcess.CpuPercent,
                // user, system, children_user, children_system, iowait (in clock ticks)
                ProcessCpuTimes = string.Join('|', this.DatabaseProcess.CpuTimes),
                // read_count, write_count, read_bytes, write_bytes
                ProcessIOCounters = string.Join('|', this.DatabaseProcess.IOCounters),
                //rss, vms, shared, text, lib, data, dirty
                ProcessMemoryInfo = string.Join('|', this.DatabaseProcess.MemoryInfo),
                ProcessMemoryPercent = this.DatabaseProcess.MemoryPercent,
                ProcessThreadsNumber = this.DatabaseProcess.ThreadsNumber,
                ProcessStatus = this.DatabaseProcess.Status,
            };
        }

        public class GlancesRecord
        {
            public long Timestamp { get; set; }

            public string Database { get; set; }
            public string Operation { get; set; }
            public int ClientsNumber { get; set; }
            public int BatchSize { get; set; }
                  public int Dimensions { get; set; }
                   public int Percentage { get; set; }
            public int SensorsNumber { get; set; }

            public string Mode {get;set; }

            public double CpuTotal { get; set; }
            public double CpuSystem { get; set; }
            public double CpuUser { get; set; }
            public double CpuIOWait { get; set; }
            public long CpuContextSwitches { get; set; }
            public long CpuSysCalls { get; set; }

            public long MemActive { get; set; }
            public long MemAvailable { get; set; }
            public long MemBuffers { get; set; }
            public long MemCached { get; set; }
            public long MemFree { get; set; }
            public long MemInActive { get; set; }
            public long MemTotal { get; set; }
            public long MemShared { get; set; }
            public long MemUsed { get; set; }

            public long SwapFree { get; set; }
            public long SwapSin { get; set; }
            public long SwapSout { get; set; }
            public long SwapTotal { get; set; }
            public long SwapUsed { get; set; }

            public double DiskReadBytes { get; set; }
            public long DiskReadCount { get; set; }
            public double DiskWriteBytes { get; set; }
            public long DiskWriteCount { get; set; }

            public string FsDeviceName { get; set; }
            public long FsFree { get; set; }
            public string FsFs_Type { get; set; }
            public string FsKey { get; set; }
            public string FsMnt_Point { get; set; }
            public double FsPercent { get; set; }
            public long FsSize { get; set; }
            public long FsUsage { get; set; }

            public long NetworkCumulativeConnections { get; set; }
            public long NetworkCumulativeReceives { get; set; }
            public long NetworkCumulativeTransmissions { get; set; }
            public long NetworkConnections { get; set; }
            public long NetworkTransmissions { get; set; }
            public long NetworkReceives { get; set; }
            public bool NetworkIsUp { get; set; }
            public double NetworkSpeed { get; set; }

            public double ProcessCpuPercent { get; set; }
            public string ProcessCpuTimes { get; set; }
            public string ProcessIOCounters { get; set; }
            public string ProcessMemoryInfo { get; set; }
            public double ProcessMemoryPercent { get; set; }
            public int? ProcessThreadsNumber { get; set; }
            public string ProcessStatus { get; set; }
        }
    }
}