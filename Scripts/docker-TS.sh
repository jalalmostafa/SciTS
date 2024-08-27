cd ..
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir ts_data ts_log ts_config 

docker run -d   -p 6432:5432 \
         \
        -v $(realpath ./ts_data):/home/postgres/pgdata/data/ \
        -v $(realpath ./ts_config):/var/lib/postgresql/data/  \
        -v $(realpath ./ts_log):/var/log/postrgresql/               \
        -e POSTGRES_PASSWORD=TimescalePW     \
        timescale/timescaledb:latest-pg14