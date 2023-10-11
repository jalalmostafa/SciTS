glances -w --disable-webui & \
echo "are all dbms deamons running?" & \
xterm -e watch tail -n2 *.log *.csv & \
\
dotnet run --project BenchmarkTool write regular DummyDB > Dummy.log 2>&1 && \
dotnet run --project BenchmarkTool write irregular DummyDB > Dummy.log 2>&1 && \
\
dotnet run --project BenchmarkTool consecutive regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool consecutive irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-33%Q regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-33%Q irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-33%Q irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-100%Q regular PostgresDB > mixedR-P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular ClickhouseDB > mixedR-C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-100%Q irregular PostgresDB > mixedR-P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular ClickhouseDB > mixedR-C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-300%Q regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-300%Q irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-300%Q irregular DatalayertsDB > D.log 2>&1 && \
\
# dotnet run --project BenchmarkTool mixed-50%Q regular PostgresDB > P.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q regular ClickhouseDB > C.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q regular TimescaleDB > T.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q regular InfluxDB > I.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q regular DatalayertsDB > D.log 2>&1 && \
\
# dotnet run --project BenchmarkTool mixed-50%Q irregular PostgresDB > P.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q irregular ClickhouseDB > C.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q irregular TimescaleDB > T.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q irregular InfluxDB > I.log 2>&1 && \
# dotnet run --project BenchmarkTool mixed-50%Q irregular DatalayertsDB > D.log 2>&1 && \
\
xterm -e echo "end"


 