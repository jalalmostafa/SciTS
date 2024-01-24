
glances -w --disable-webui & \
echo "are all dbms deamons running?" & \
\
dotnet run --project BenchmarkTool populate regular PostgresDB > P-r-pop.log 2>&1 && echo "P-reg-pop done" & \
\
dotnet run --project BenchmarkTool populate regular ClickhouseDB - && echo "C-reg-pop done" & \
\
dotnet run --project BenchmarkTool populate regular TimescaleDB > T-r-pop.log 2>&1 && echo "T-reg-pop done"   & \
\
dotnet run --project BenchmarkTool populate regular InfluxDB > I-r-pop.log 2>&1 && echo "I-reg-pop done"  & \
\
dotnet run --project BenchmarkTool populate regular DatalayertsDB > D-r-pop.log 2>&1 &&  echo "D-reg-pop done"  & \
\
\
dotnet run --project BenchmarkTool populate irregular PostgresDB > P-ir-pop.log 2>&1 && echo "P-ir-pop done"  & \
\
dotnet run --project BenchmarkTool populate irregular ClickhouseDB > C-ir-pop.log 2>&1 && echo "C-ir-pop done"  & \
\
dotnet run --project BenchmarkTool populate irregular TimescaleDB > T-ir-pop.log 2>&1 && echo "T-ir-pop done"  & \
\
dotnet run --project BenchmarkTool populate irregular InfluxDB > I-ir-pop.log 2>&1 && echo "I-ir-pop done" & \
\
dotnet run --project BenchmarkTool populate irregular DatalayertsDB > D-ir-pop.log 2>&1 &&   echo "D-ir-pop done"   & \

watch tail -n2 *.log *.csv && sleep 15600 && \

dotnet run --project BenchmarkTool consecutive regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-100%Q regular PostgresDB > mixedR-P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular ClickhouseDB > mixedR-C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-50%Q regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-150%Q regular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q regular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q regular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q regular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q regular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool consecutive irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool consecutive irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-100%Q irregular PostgresDB > mixedR-P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular ClickhouseDB > mixedR-C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-100%Q irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-50%Q irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-50%Q irregular DatalayertsDB > D.log 2>&1 && \
\
dotnet run --project BenchmarkTool mixed-150%Q irregular PostgresDB > P.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q irregular ClickhouseDB > C.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q irregular TimescaleDB > T.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q irregular InfluxDB > I.log 2>&1 && \
dotnet run --project BenchmarkTool mixed-150%Q irregular DatalayertsDB > D.log 2>&1 && \
echo "end"


 