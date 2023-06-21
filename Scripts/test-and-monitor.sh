watch tail -n5 *.log *.csv &
#./.dotnet/dotnet7 run --project SciTS/BenchmarkTool consecutive regular PostgresDB > consR-P.log 2>&1 && \
./.dotnet/dotnet7 run --project SciTS/BenchmarkTool consecutive regular ClickhouseDB > consR-C.log 2>&1 && \
#./.dotnet/dotnet7 run --project SciTS/BenchmarkTool mixed regular PostgresDB > mixedR-P.log 2>&1 && \
./.dotnet/dotnet7 run --project SciTS/BenchmarkTool mixed regular ClickhouseDB > mixedR-C.log 2>&1 && \
#./.dotnet/dotnet7 run --project SciTS/BenchmarkTool consecutive irregular PostgresDB > consIR-P.log 2>&1 && \
./.dotnet/dotnet7 run --project SciTS/BenchmarkTool consecutive irregular ClickhouseDB > consIR-C.log 2>&1 && \
#./.dotnet/dotnet7 run --project SciTS/BenchmarkTool mixed irregular PostgresDB > mixedIR-P.log 2>&1 && \
./.dotnet/dotnet7 run --project SciTS/BenchmarkTool mixed irregular ClickhouseDB > mixedIR-C.log 2>&1

                                                                                                        