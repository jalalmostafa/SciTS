
# IF WORKLOAD FILE IS CONFIGURED SPECIFICLY for one DB

dotnet run --project SciTS/BenchmarkTool consecutive 2>&1 | tee consecutive.log &&
dotnet run --project SciTS/BenchmarkTool consecutive 2>&1 | tee consecutive.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries 2>&1 | tee  mixed-AggQueries.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries 2>&1 | tee  mixed-LimitedQueries.log && echo "ende"

# ASSERT App.config is configured correctly and glances is working on server.

dotnet run --project SciTS/BenchmarkTool consecutive regular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool read irregular InfluxDB &&
dotnet run --project SciTS/BenchmarkTool write irregular InfluxDB &&  
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular InfluxDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular InfluxDB  &&

# INSERT HERE COMMAND TO STOP INFLUX CONTAINER AND START POSTGRES CONTAINER

dotnet run --project SciTS/BenchmarkTool read regular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool read irregular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool write irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular PostgresDB &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular PostgresDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular PostgresDB && 

# INSERT HERE COMMAND Like "cp App.config.DatalayerTS App.config" TO GET DLTS CLOUD GLANCES DATA

dotnet run --project SciTS/BenchmarkTool read regular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool read irregular DatalayertsDB &&
dotnet run --project SciTS/BenchmarkTool write irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular DatalayertsDB && 
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular DatalayertsDB &&  echo end-


