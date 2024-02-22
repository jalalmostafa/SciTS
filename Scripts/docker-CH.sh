cd ..
mkdir ContainerDB_persistence_files
cd  ContainerDB_persistence_files
mkdir ch_log ch_config ch_data


docker run -d -p 8123:8123 -p 9000:9000 -p 9009:9009   \
    -v $(realpath ./ch_data):/var/lib/clickhouse/ \
        -v $(realpath ./ch_log):/var/log/clickhouse-server/ \
        -v $(realpath ./ch_config):/etc/clickhouse-server/ \
         --ulimit nofile=262144:262144 \
        clickhouse/clickhouse-server
 