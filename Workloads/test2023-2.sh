
# IF WORKLOAD FILE IS CONFIGURED SPECIFICLY for one DB and therefore you have several SciTS installations, each one for his TargetDB:
    #  cd "Folder-of-SpecificDB-SciTS"

# ASSUMING you populated all the DBS with at least one full day of data:    
    #  dotnet run --project SciTS/BenchmarkTool populate regular 2>&1 | tee Rpop.log && dotnet run --project SciTS/BenchmarkTool populate irregular 2>&1 | tee IRpop.log && touch populated

# Assert Glances is running propery and sending data. For monitoring in second terminal: 
    # watch tail -4 *.csv

dotnet run --project SciTS/BenchmarkTool populateShort+1 regular 2>&1 | tee RpopS.log &&
dotnet run --project SciTS/BenchmarkTool consecutive regular 2>&1 | tee Rconsecutive.log &&
dotnet run --project SciTS/BenchmarkTool consecutive regular 2>&1 | tee Rconsecutive.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries regular 2>&1 | tee  Rmixed-AggQueries.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries regular 2>&1 | tee  Rmixed-LimitedQueries.log &&

dotnet run --project SciTS/BenchmarkTool populateShort+3 irregular 2>&1 | tee IRpopS.log &&
dotnet run --project SciTS/BenchmarkTool consecutive irregular 2>&1 | tee iRconsecutive.log &&
dotnet run --project SciTS/BenchmarkTool consecutive irregular 2>&1 | tee iRconsecutive.log &&
dotnet run --project SciTS/BenchmarkTool mixed-AggQueries irregular 2>&1 | tee  iRmixed-AggQueries.log &&
dotnet run --project SciTS/BenchmarkTool mixed-LimitedQueries irregular 2>&1 | tee  iRmixed-LimitedQueries.log && echo "end" && touch finished
