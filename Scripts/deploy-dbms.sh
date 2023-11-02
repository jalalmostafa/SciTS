wget https://dl.influxdata.com/influxdb/releases/influxdb2-2.7.3-amd64.deb
sudo dpkg -i influxdb2-2.7.3-amd64.deb
sudo service influxdb start
influx config create \
  --config-name get-started \
  --host-url http://localhost:8086 \
  --org katrin \
  --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg==

influx bucket create \
    --name katrindb \
     --org katrin \

influx user create -n katrin -p InfluxPW -o katrin



curl https://clickhouse.com/ | sh
./clickhouse server



sudo apt install -y gnupg postgresql-common apt-transport-https lsb-release wget
sudo sh /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh  
echo "deb https://packagecloud.io/timescale/timescaledb/debian/ $(lsb_release -c -s) main" | sudo tee /etc/apt/sources.list.d/timescaledb.list
wget --quiet -O - https://packagecloud.io/timescale/timescaledb/gpgkey | sudo apt-key add -
sudo apt update
sudo apt install -y timescaledb-2-postgresql-14
sudo apt-get install -y postgresql-client
sudo systemctl restart postgresql

# sudo apt install postgresql



 

curl https://github.com/VictoriaMetrics/VictoriaMetrics/releases/download/v1.94.0/victoria-metrics-darwin-amd64-v1.94.0.tar.gz
tar -x victoria-metrics-darwin-amd64-v1.94.0.tar.gz
victoria-metrics-prod  -retentionPeriod=9y