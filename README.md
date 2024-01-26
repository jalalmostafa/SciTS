# SciTS v2, 2023 update

A tool to benchmark Time-series on different databases

- Reworked Architecture
- Mixed, online Workloads
- adds Modes: Regular, Irregular Ingestion.
- Multiple Values per Timeseries ("Dimensions")
- adds Limited Queries
- adds CLI Arguments
  
Requires .NET 7.x cross-platform framework.

## Citation

[![DOI](https://zenodo.org/badge/429005385.svg)](https://zenodo.org/badge/latestdoi/429005385)

Please cite our work:

> Jalal Mostafa, Sara Wehbi, Suren Chilingaryan, and Andreas Kopmann. 2022. SciTS: A Benchmark for Time-Series Databases in Scientific Experiments and Industrial Internet of Things. In 34th International Conference on Scientific and Statistical Database Management (SSDBM 2022). Association for Computing Machinery, New York, NY, USA, Article 12, 1–11. https://doi.org/10.1145/3538712.3538723


### Bibtex

```bibtex
@inproceedings{10.1145/3538712.3538723,
    author = {Mostafa, Jalal and Wehbi, Sara and Chilingaryan, Suren and Kopmann, Andreas},
    title = {SciTS: A Benchmark for Time-Series Databases in Scientific Experiments and Industrial Internet of Things},
    year = {2022},☺
    isbn = {9781450396677},
    publisher = {Association for Computing Machinery},
    address = {New York, NY, USA},
    url = {https://doi.org/10.1145/3538712.3538723},
    doi = {10.1145/3538712.3538723},
    abstract = {Time-series data has an increasingly growing usage in Industrial Internet of Things (IIoT) and large-scale scientific experiments. Managing time-series data needs a storage engine that can keep up with their constantly growing volumes while providing an acceptable query latency. While traditional ACID databases favor consistency over performance, many time-series databases with novel storage engines have been developed to provide better ingestion performance and lower query latency. To understand how the unique design of a time-series database affects its performance, we design SciTS, a highly extensible and parameterizable benchmark for time-series data. The benchmark studies the data ingestion capabilities of time-series databases especially as they grow larger in size. It also studies the latencies of 5 practical queries from the scientific experiments use case. We use SciTS to evaluate the performance of 4 databases of 4 distinct storage engines: ClickHouse, InfluxDB, TimescaleDB, and PostgreSQL.},
    booktitle = {Proceedings of the 34th International Conference on Scientific and Statistical Database Management},
    articleno = {12},
    numpages = {11},
    keywords = {time-series databases, database management systems, industrial internet of things, scientific experiments, sensor data, time-series},
    location = {Copenhagen, Denmark},
    series = {SSDBM '22}
}
```

## How to run

1. Create your workload as `App.config` (case-sensitive) in `BenchmarkTool`.
2. Edit the connection strings to your database servers in the workload file.
3. Choose the target database in the workload file using `TargetDatabase` element.
4. run `dotnet run --project BenchmarkTool write` if it's an ingestion workload,
and `dotnet run --project BenchmarkTool read` if it's a query workload.
x. Use `Scripts/ccache.sh <database-service-name>` to clear the cache between query tests.

## Workloads

You can choose from the available workloads by choosing a `*.config` file from `Workloads` folder.
The file to workload mapping is as follow:

| Workload    | Workload file                      |
| ----------- | ---------------------------------- |
| Q1          | query-q1.config                    |
| Q2          | query-q2.config                    |
| Q3          | query-q3.config                    |
| Q4          | query-q4.config                    |
| Q5          | query-q5.config                    |
| Batching    | ingestion-batching-1client.config  |
| Concurrency | ingestion-batching-nclients.config |
| Scaling     | ingestion-scaling.config           |

## System Metrics using Glances

This tool uses [glances](https://github.com/nicolargo/glances/).
1. Install glances with all plugins on the database server using `pip install glances[all]`
2. Run glances REST API on the database server using `glances -w --disable-webui`

## Workload Definition Files

```xml
<configuration>
    <appSettings>
        <!-- Postgres connection settings -->
        <add key="PostgresConnection" value="Server=192.168.26.140;Port=5432;Database=katrindb2;User Id=postgres;Password=P@ssw0rd;" />

        <!-- Timescale connection settings -->
        <add key="TimescaleConnection" value="Server=192.168.26.140;Port=5432;Database=katrindb;User Id=postgres;Password=P@ssw0rd;CommandTimeout=300" />

        <!-- InfluxDB connection settings -->
        <add key="InfluxDBHost" value="http://192.168.26.140:8086" />
        <add key="InfluxDBToken" value="vUAASWKs-OOFpGq5BQ44Mc-GYfKx5Szda2zQz-o4lXsmPXBBMfGvqkyoDApS8sZxni73cwJ05Mm8cCUGalunKw==" />
        <add key="InfluxDBBucket" value="katrindb" />
        <add key="InfluxDBOrganization" value="katrin" />

        <!-- Clickhouse connection settings -->
        <add key="ClickhouseHost" value="192.168.26.140" />
        <add key="ClickhousePort" value="9000" />
        <add key="ClickhouseUser" value="default" />
        <add key="ClickhouseDatabase" value="katrindb" />

        <!-- General Settings -->
        <!-- How many times to repeat this test -->
        <add key="TestRetries" value="1" />
        <!-- the length of the time-series data in the database (in the database) -->
        <add key="DaySpan" value="15" />
        <!-- Could be: TimescaleDB, InfluxDB, ClickhouseDB, MySQLDB, PostgresDB -->
        <add key="TargetDatabase" value="TimescaleDB" />
        <!-- Initial Timestamp -->
        <add key="StartTime" value="2022-01-01T00:00:00.00" />
        <!-- Where to store metrics file -->
        <add key="MetricsCSVPath" value="Metrics.csv" />
        <!-- System Metrics Options -->
        <add key="GlancesUrl" value="http://192.168.26.140:61208" />
        <add key="GlancesDatabasePid" value="1" />
        <add key="GlancesPeriod" value="1" />
        <add key="GlancesOutput" value="Glances.csv"/>
        <add key="GlancesNIC" value="enp9s0" />
        <add key="GlancesDisk" value="sda1" />

        <!-- Read Query Options -->
        <!-- Could be: Q1-RangeQueryRawData, Q4-RangeQueryAggData, Q2-OutOfRangeQuery, Q5-DifferenceAggQuery, Q3-STDDevQuery -->
        <add key="QueryType" value="RangeQueryRawData" />
        <add key="AggregationIntervalHour" value="1" />
        <add key="DurationMinutes" value="10" />
        <add key="SensorsFilter" value="1,2,3,4,5,6,7,8,9,10" />
        <add key="SensorID" value="100" />
        <add key="MaxValue" value="20000000" />
        <add key="MinValue" value="100000" />
        <add key="FirstSensorID" value="100" />
        <add key="SecondSensorID" value="200" />

        <!-- Ingestion and Population -->
        <add key="BatchSizeOptions" value="20000" />
        <!-- Number of concurrent clients  -->
        <add key="ClientNumberOptions" value="48" />
        <!--Number of sensors-->
        <add key="SensorNumber" value="100000" />

    </appSettings>

</configuration>
```
