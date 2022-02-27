# ts-bench

A tool to benchmark Time-series on different databases

## Benchmarking Scenarios and Configuration

| TSDB | Actual Size   | Compressed Size | Compression Ratio |
| ---- | ------------- | --------------- | ----------------- |
| TimescaleDB v2.5.1   | ||||
| PostgreSQL v13.5     | 314 GB (174 GB Index) | | ||
| InfluxDB v2.1.1      |||||
| ClickHouse v22.1.3.7 |||||

### System Metrics using Glances

This tool uses [glances](https://github.com/nicolargo/glances/).
1. Install glances with all plugins on the database server using `pip install glances[all]`
2. Run glances REST API on the database server using `glances -w --disable-webui`

