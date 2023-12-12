## command
## command
## command
## command

## command mixed-AggQueries, mixed-11%LimitedQueries

dotnet run --project SciTS/BenchmarkTool read regular ClickhouseDB &&
dotnet run --project SciTS/BenchmarkTool write regular ClickhouseDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular ClickhouseDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular ClickhouseDB && 
dotnet run --project SciTS/BenchmarkTool read irregular ClickhouseDB &&
dotnet run --project SciTS/BenchmarkTool write irregular ClickhouseDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular ClickhouseDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular ClickhouseDB && echo end



dotnet run --project SciTS/BenchmarkTool read regular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool read irregular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write irregular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular InfluxDB && echo end



dotnet run --project SciTS/BenchmarkTool read regular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool read irregular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular PostgresDB && echo end




dotnet run --project SciTS/BenchmarkTool read regular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool read irregular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular DatalayertsDB && echo end-