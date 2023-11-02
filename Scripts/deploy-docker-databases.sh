cd ..
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir if_data if_config if_log ts_data ts_log ts_config pg_config pg_log pg_data ch_log ch_config ch_data

docker run -d   -p 8087:8086 \
      -v $(realpath ./if_data):/var/lib/influxdb2/ \
      -v $(realpath ./if_config):/etc/influxdb2/ \
        -v $(realpath ./if_log):/var/log/influxdb2/  \
        -e DOCKER_INFLUXDB_INIT_MODE=setup \
      -e DOCKER_INFLUXDB_INIT_USERNAME=katrin \
      -e DOCKER_INFLUXDB_INIT_PASSWORD=InfluxPW \
      -e DOCKER_INFLUXDB_INIT_ORG=katrin \
      -e DOCKER_INFLUXDB_INIT_BUCKET=katrindb \
      -e DOCKER_INFLUXDB_INIT_ADMIN_TOKEN=u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
      influxdb:2.0

docker run -d   -p 6432:5432 \
         \
        -v $(realpath ./ts_data):/home/postgres/pgdata/data/ \
        -v $(realpath ./ts_config):/var/lib/postgresql/data/  \
        -v $(realpath ./ts_log):/var/log/postrgresql/               \
        -e POSTGRES_PASSWORD=P@ssw0rd     \
        timescale/timescaledb:latest-pg14


 docker run -d   -p 5433:5432 \
        \
        -v $(realpath ./pg_config):/etc/postgresql/ \
        -v $(realpath ./pg_log):/var/log/postgresql/ \
        -v $(realpath ./pg_data):/var/lib/postgresql/data/ \
        -e POSTGRES_PASSWORD=PostgresPW \
         postgres -c shared_buffers=256MB -c max_connections=200


 docker run -d -p 8123:8123 -p 9000:9000 -p 9009:9009   \
    -v $(realpath ./ch_data):/var/lib/clickhouse/ \
        -v $(realpath ./ch_log):/var/log/clickhouse-server/ \
        -v $(realpath ./ch_config):/etc/clickhouse-server/ \
         --ulimit nofile=262144:262144 \
        clickhouse/clickhouse-server
 
       # --name clickhouse-server     --name postgresdb          --name timescaledb   --network=host                              



docker run -it --rm -v /path/to/victoria-metrics-data:/victoria-metrics-data -p 8428:8428 victoriametrics/victoria-metrics
# retention period!!
