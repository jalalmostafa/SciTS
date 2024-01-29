
echo "are all dbms deamons running?" & \
\
watch tail -n3 *.log *.csv & \
\
dotnet run --project BenchmarkTool populate regular PostgresDB > P-r-pop.log 2>&1 && echo "P-reg done" && \
\
dotnet run --project BenchmarkTool populate regular ClickhouseDB > C-r-pop.log 2>&1 && echo "C-reg done" && \
\
dotnet run --project BenchmarkTool populate regular TimescaleDB > T-r-pop.log 2>&1 && echo "T-reg done"   && \
\
dotnet run --project BenchmarkTool populate regular InfluxDB > I-r-pop.log 2>&1 && echo "I-reg done"  && \
\
dotnet run --project BenchmarkTool populate regular DatalayertsDB > D-r-pop.log 2>&1 &&  echo "D-reg done"  && \
\
\
dotnet run --project BenchmarkTool populate irregular PostgresDB > P-ir-pop.log 2>&1 && echo "P-ir done"  && \
\
dotnet run --project BenchmarkTool populate irregular ClickhouseDB > C-ir-pop.log 2>&1 && echo "C-ir done"  && \
\
dotnet run --project BenchmarkTool populate irregular TimescaleDB > T-ir-pop.log 2>&1 && echo "T-ir done"  && \
\
dotnet run --project BenchmarkTool populate irregular InfluxDB > I-ir-pop.log 2>&1 && echo "I-ir done" && \
\
dotnet run --project BenchmarkTool populate irregular DatalayertsDB > D-ir-pop.log 2>&1 &&   echo "D-ir done"  && \
echo "done"

# glances -w --disable-webui & \
# echo "are all dbms deamons running?" & \
# \
# watch tail -n3 *.log *.csv & \
# \
# dotnet run --project BenchmarkTool populate regular PostgresDB > P-r-pop.log 2>&1 && echo "P-reg done" && \
# \
# dotnet run --project BenchmarkTool populate regular ClickhouseDB > C-r-pop.log 2>&1 && echo "C-reg done" && \
# \
# dotnet run --project BenchmarkTool populate regular TimescaleDB > T-r-pop.log 2>&1 && echo "T-reg done"   && \
# \
# dotnet run --project BenchmarkTool populate regular InfluxDB > I-r-pop.log 2>&1 && echo "I-reg done"  && \
# \
# dotnet run --project BenchmarkTool populate regular DatalayertsDB > D-r-pop.log 2>&1 &&  echo "D-reg done"  && \
# \
# \
# dotnet run --project BenchmarkTool populate irregular PostgresDB > P-ir-pop.log 2>&1 && echo "P-ir done"  && \
# \
# dotnet run --project BenchmarkTool populate irregular ClickhouseDB > C-ir-pop.log 2>&1 && echo "C-ir done"  && \
# \
# dotnet run --project BenchmarkTool populate irregular TimescaleDB > T-ir-pop.log 2>&1 && echo "T-ir done"  && \
# \
# dotnet run --project BenchmarkTool populate irregular InfluxDB > I-ir-pop.log 2>&1 && echo "I-ir done" && \
# \
# dotnet run --project BenchmarkTool populate irregular DatalayertsDB > D-ir-pop.log 2>&1 &&   echo "D-ir done"  && \
# echo "done"