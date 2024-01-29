
# cp App.config.SpecificConfig App.config &&
dotnet run --project SciTS/BenchmarkTool read regular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write regular InfluxDB && 

dotnet run --project SciTS/BenchmarkTool read regular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool read irregular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool read irregular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write irregular InfluxDB &&  
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular InfluxDB  




dotnet run --project SciTS/BenchmarkTool read regular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool read irregular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular DatalayertsDB && echo end-