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
apt install -y wget curl gpg sudo htop vim pip git glances dotnet7 net-tools fio sysbench
cd home



curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.3-1_amd64.deb
c
wget https://dl.influxdata.com/influxdb/releases/influxdb2-client-2.7.3-linux-amd64.tar.gz
tar xvzf influxdb2-client-2.7.3-linux-amd64.tar.gz
cp ./influx /usr/local/bin/


# influxd > IF.log  2>&1 &  
 service influxdb start

influx setup \
  --org scits \
  --bucket scitsdb \
  --username scits \
  --password InfluxPW \
  --host http://localhost:8086 \
  --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
 --force  


curl https://clickhouse.com/ | sh
./clickhouse server  > CH.log  2>&1 &

./clickhouse install
clickhouse start

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

# POSTGRES TUNE:: https://pgtune.leopard.in.ua/




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

 
git clone https://github.com/sandrosano/SciTS

passwd postgres
#  hier alle "peer" auf "md5"
vim  /etc/postgresql/14/main/pg_hba.conf  
# vim SciTS/BenchmarkTool/App.config 

# iptables -A INPUT -s 85.215.210.247 -j ACCEPT


# dotnet run --project SciTS/BenchmarkTool consecutive regular InfluxDB
# dotnet run --project SciTS/BenchmarkTool consecutive regular PostgresDB
# dotnet run --project SciTS/BenchmarkTool consecutive regular ClickhouseDB

