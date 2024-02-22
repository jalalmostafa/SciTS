# usefull for "scaling workloads" benchmark: How does a system react  when it is already quite filled with data
dotnet run --project BenchmarkTool populate regular PostgresDB > P-r-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+1 regular PostgresDB > P-r-pop.log 2>&1
dotnet run --project BenchmarkTool populate+2 regular PostgresDB > P-r-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+3 regular PostgresDB > P-r-pop.log 2>&1 

dotnet run --project BenchmarkTool populate irregular PostgresDB > P-ir-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+1 irregular PostgresDB > P-ir-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+2 irregular PostgresDB > P-ir-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+3 irregular PostgresDB > P-ir-pop.log 2>&1 

dotnet run --project BenchmarkTool populate regular InfluxDB > I-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+1 regular InfluxDB > I-r-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+2 regular InfluxDB > I-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+3 regular InfluxDB > I-r-pop.log 2>&1  

dotnet run --project BenchmarkTool populate irregular InfluxDB > I-ir-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+1 irregular InfluxDB > I-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+2 irregular InfluxDB > I-ir-pop.log 2>&1 
dotnet run --project BenchmarkTool populate+3 irregular InfluxDB > I-ir-pop.log 2>&1 

dotnet run --project BenchmarkTool populate regular ClickhouseDB > C-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+1 regular ClickhouseDB > C-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+2 regular ClickhouseDB > C-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+3 regular ClickhouseDB > C-r-pop.log 2>&1  

dotnet run --project BenchmarkTool populate irregular ClickhouseDB > C-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+1 irregular ClickhouseDB > C-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+2 irregular ClickhouseDB > C-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+3 irregular ClickhouseDB > C-ir-pop.log 2>&1  

dotnet run --project BenchmarkTool populate regular DatalayertsDB > D-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+1 regular DatalayertsDB > D-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+2 regular DatalayertsDB > D-r-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+3 regular DatalayertsDB > D-r-pop.log 2>&1  

dotnet run --project BenchmarkTool populate irregular DatalayertsDB > D-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+1 irregular DatalayertsDB > D-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+2 irregular DatalayertsDB > D-ir-pop.log 2>&1  
dotnet run --project BenchmarkTool populate+3 irregular DatalayertsDB > D-ir-pop.log 2>&1  
