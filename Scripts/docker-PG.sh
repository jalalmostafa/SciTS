cd ..
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir pg_config pg_log pg_data

docker run -d   -p 5433:5432 \
        \
        -v $(realpath ./pg_config):/etc/postgresql/ \
        -v $(realpath ./pg_log):/var/log/postgresql/ \
        -v $(realpath ./pg_data):/var/lib/postgresql/data/ \
        -e POSTGRES_PASSWORD=PostgresPW \
         postgres -c shared_buffers=256MB -c max_connections=200
