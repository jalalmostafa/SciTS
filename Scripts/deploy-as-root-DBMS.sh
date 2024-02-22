# sudo apt update
# sudo apt install -y fio
# TEST_DIR=/mnt/disks/mnt_dir/fiotest
# sudo mkdir -p $TEST_DIR
# mount TEST DIR to DOCKER
# https://cloud.google.com/compute/docs/disks/benchmarking-pd-performance?hl=de

# docker run -it --cap-add SYS_ADMIN --cap-add=SYS_PTRACE -p 61209:61208 -p 8087:8086  -p 6432:5432 -p 5433:5432 -p 8124:8123 -p 9001:9000 -p 9010:9009    -p 8428:8428  ubuntu bash 
# psql -U postgres -W   ( to check pw access)
# set postgres conf: https://hassanannajjar.medium.com/how-to-fix-error-password-authentication-failed-for-the-user-in-postgresql-896e1fd880dc

## NOT WORKING IN ROOT
apt update -y
apt install -y tzdata
apt upgrade -y
apt install -y wget curl gpg sudo htop vim pip git glances dotnet7 net-tools fio sysbench glances apt-transport-https ca-certificates dirmngr  
# cd home

mkdir /mnt/vdb1/
#evtl fdisk /dev/vdb -> n -> w  -> mkfs -t xfs /dev/vdb1   
mount /dev/vdb1 /mnt/vdb1
# add to fstab UUID=25e0fc93-8bc4-43bc-b5eb-cc0e860570a2 /mnt/vdb1 xfs  defaults  0      0
#  change all data dirs in POSTGRES CLICKHOUSE INFLUX to vdb1
#  assure listen to * /correctn networkinterface

 iptables -t nat -I PREROUTING -p tcp --dport 8086 -j DNAT --to 127.0.0.1:8086
 iptables -t nat -I PREROUTING -p tcp --dport 5432 -j DNAT --to 127.0.0.1:5432
 iptables -t nat -I PREROUTING -p tcp --dport 9000 -j DNAT --to 127.0.0.1:9000
 iptables -t nat -I PREROUTING -p tcp --dport 61209 -j DNAT --to 127.0.0.1:61209

# curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.3-1_amd64.deb
# c
wget https://dl.influxdata.com/influxdb/releases/influxdb2-client-2.7.3-linux-amd64.tar.gz
tar xvzf influxdb2-client-2.7.3-linux-amd64.tar.gz
cp ./influx /usr/local/bin/

 curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.4-1_amd64.deb
sudo dpkg -i influxdb2_2.7.4-1_amd64.deb

# influxd > IF.log  2>&1 &  
 service influxdb start

# or edit gui on 18086
# sandro@sandro-opi:~$ sudo ssh -L 18086:localhost:8086 root@85.215.243.202 -i ./KIT/sandro_test.openssh 

#  TOKEN IONOS 1-12
#    aOBozbShzfkQo2EOKGAwbi8GDJKXaSiMF5WG9UoH0XIa8oM0EMZ1fesiyzTPvbrIbdJzdH4aKNI3GEKI_odHGA==

influx setup -n scitsconfig\
  --org scits \
  --bucket scitsdb \
  --username scits \
  --password InfluxPW \
  --host http://127.0.0.1:8086 \
  --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
 --force  

 influx config create --active \
  -n scitsconfigclient \
  -u http://127.0.0.1:8086 \
  -t u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
  -o scits     

export INFLUXD_HTTP_BIND_ADDRESS=127.0.0.1:8086
 influxd --http-bind-address=127.0.0.1:8086
# evtl chown -R influxdb /PATH/influxdb
#  VIM /ETC/INFLUX/CONFIG

# vlickhouse

sudo apt-get install -y apt-transport-https ca-certificates dirmngr
GNUPGHOME=$(mktemp -d)
sudo GNUPGHOME="$GNUPGHOME" gpg --no-default-keyring --keyring /usr/share/keyrings/clickhouse-keyring.gpg --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 8919F6BD2B48D754
sudo rm -r "$GNUPGHOME"
sudo chmod +r /usr/share/keyrings/clickhouse-keyring.gpg

echo "deb [signed-by=/usr/share/keyrings/clickhouse-keyring.gpg] https://packages.clickhouse.com/deb stable main" | sudo tee \
    /etc/apt/sources.list.d/clickhouse.list
sudo apt-get update

sudo apt-get install -y clickhouse-server clickhouse-client

sudo service clickhouse-server start

# Insert here_: vim /etc/clickhouse-server/users.d/default-password.xml 
#         <password></password>

# /etc/clickhouse-server/config.xml   change:  ::  to
#  <clickhouse>
#     <listen_host>0.0.0.0</listen_host> -> DATA PATH!!!!

# postgres
# Create the file repository configuration:
sh -c 'echo "deb https://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'

# Import the repository signing key:
wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -

# Update the package lists:
apt-get update -y

# Install the latest version of PostgreSQL.
# If you want a specific version, use 'postgresql-12' or similar instead of 'postgresql':
apt-get -y install postgresql
chown -R postgres /var/lib/postgresql/
#  postgres.CONF::::    HOST -> * ,  PATH -> /mnt...
# HBA file:::: host    all             all             10.7.222.11  255.255.255.0 trust 
#  schnelltest-von clientmacshein:  psql -h 10.7.222.12 -p 5432 -U postgres
# local   all             all                                     trust
# host    all             all             all                     trust
# 

# POSTGRES TUNE:: https://pgtune.leopard.in.ua/

#  postgres.config  : listen : localhost uncomment

# TUNE:  (insert in postgres.conf)
# DB Version: 16
# OS Type: linux
# DB Type: mixed
# Total Memory (RAM): 64 GB
# CPUs num: 8
# Connections num: 50
# Data Storage: ssd

# max_connections = 50
# shared_buffers = 16GB
# effective_cache_size = 48GB
# maintenance_work_mem = 2GB
# checkpoint_completion_target = 0.9
# wal_buffers = 16MB
# default_statistics_target = 100
# random_page_cost = 1.1
# effective_io_concurrency = 200
# work_mem = 41943kB
# huge_pages = tryAnerkennung
# min_wal_size = 1GB
# max_wal_size = 4GB
# max_worker_processes = 8
# max_parallel_workers_per_gather = 4
# max_parallel_workers = 8
# max_parallel_maintenance_workers = 4

# # timescale
apt install -y gnupg postgresql postgresql-common apt-transport-https lsb-release wget
sh /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh  

# # echo "deb https://packagecloud.io/timescale/timescaledb/ubuntu/ $(lsb_release -c -s) main" | tee /etc/apt/sources.list.d/timescaledb.list  
# # upper link substituted by:
curl -s https://packagecloud.io/install/repositories/timescale/timescaledb/script.deb.sh | bash

wget --quiet -O - https://packagecloud.io/timescale/timescaledb/gpgkey | gpg --dearmor -o /etc/apt/trusted.gpg.d/timescaledb.gpg
apt install -y timescaledb-2-postgresql-14
apt-get install -y postgresql-client

timescaledb-tune
service postgresql restart

# # TODO PG / TS TUNE

 # sudo -u postgres psql
#  nano /var/log/postgresql/postgresql....
# https://stackoverflow.com/questions/31645550/postgresql-why-psql-cant-connect-to-server

curl https://github.com/VictoriaMetrics/VictoriaMetrics/releases/download/v1.94.0/victoria-metrics-darwin-amd64-v1.94.0.tar.gz
tar -x victoria-metrics-darwin-amd64-v1.94.0.tar.gz
victoria-metrics-prod  -retentionPeriod=9y > VM.log 2>&1 &

 # pip install --user 'glances[all]'
glances -w --disable-webui &
#  MODUIFY SERIVICE FILE!!!
 
git clone https://github.com/sandrosano/SciTS

passwd postgres
#  hier alle "peer" auf "md5"
vim  /etc/postgresql/14/main/pg_hba.conf  
# vim SciTS/BenchmarkTool/App.config 

# iptables -A INPUT -s 85.215.210.247 -j ACCEPT
#  iptables -A INPUT -s 10.7.222.11 -j ACCEPT
 
# AmbientCapabilities=CAP_SYS_NICE CAP_NET_BIND_SERVICE
#  vim /etc/systemd/system/influxd.service
#  :w /etc/systemd/system/postgresql.service

#  vim /etc/systemd/system/glances.service

mkdir /mnt/vdb1/clickhouse /mnt/vdb1/influxdb /mnt/vdb1/postgresql /mnt/vdb1/postgresql/16/ /mnt/vdb1/postgresql/16/main

chown -R clickhouse:clickhouse /mnt/vdb1/clickhouse/
chown -R influxdb:influxdb /mnt/vdb1/influxdb/
chown -R postgres:postgres /mnt/vdb1/postgresql/
chown -R postgres:postgres /var/log/postgresql/

sudo -u postgres /usr/lib/postgresql/16/bin/initdb -D /mnt/vdb1/postgresql/16/main
sudo -u postgres /usr/lib/postgresql/16/bin/pg_ctl -D /mnt/vdb1/postgresql/16/main -l /var/log/postgresql/16/main/my-postgres.log start

systemctl daemon-reload
service clickhouse-server restart
service influxdb restart
service postgresql restart

