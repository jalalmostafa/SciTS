

## NOT WORKING IN ROOT
apt update -y
apt upgrade -y
apt install -y wget curl gpg sudo htop tzdata 


curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.3-1_amd64.deb
dpkg -i influxdb2_2.7.3-1_amd64.deb
wget https://dl.influxdata.com/influxdb/releases/influxdb2-client-2.7.3-linux-amd64.tar.gz
tar xvzf influxdb2-client-2.7.3-linux-amd64.tar.gz
cp ./influx /usr/local/bin/

service influxdb start

influx setup \
  --org katrin \
  --bucket katrindb \
  --username katrin \
  --password InfluxPW \
  --host http://localhost:8086 \
  --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
 --force  


curl https://clickhouse.com/ | sh
./clickhouse server  > CH.log  2>&1 &


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


# # timescale
# apt install -y gnupg postgresql postgresql-common apt-transport-https lsb-release wget
# sh /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh  


# # echo "deb https://packagecloud.io/timescale/timescaledb/ubuntu/ $(lsb_release -c -s) main" | tee /etc/apt/sources.list.d/timescaledb.list  
# # upper link substituted by:
# curl -s https://packagecloud.io/install/repositories/timescale/timescaledb/script.deb.sh | bash



# wget --quiet -O - https://packagecloud.io/timescale/timescaledb/gpgkey | gpg --dearmor -o /etc/apt/trusted.gpg.d/timescaledb.gpg
# apt install -y timescaledb-2-postgresql-14
# apt-get install -y postgresql-client

# timescaledb-tune
# service postgresql restart
# sudo -u postgres psql
# # TODO PG / TS TUNE

 

curl https://github.com/VictoriaMetrics/VictoriaMetrics/releases/download/v1.94.0/victoria-metrics-darwin-amd64-v1.94.0.tar.gz
tar -x victoria-metrics-darwin-amd64-v1.94.0.tar.gz
victoria-metrics-prod  -retentionPeriod=9y > VM.log 2>&1 &

apt install -y pip
pip install --user 'glances[all]'
apt-get install -y glances 
glances -w --disable-webui &