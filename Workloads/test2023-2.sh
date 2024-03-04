dotnet run --project SciTS/BenchmarkTool consecutive 2>&1 | tee consecutive.log
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries 2>&1 | tee  mixed-AggQueries.log
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries 2>&1 | tee  mixed-LimitedQueries.log
