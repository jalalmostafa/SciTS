# if permission issues, run : docker run -it ubuntu bash --cap-add SYS_ADMIN --cap-add=SYS_PTRACE

# unminimize
useradd -m -s /usr/bin/bash user 
usermod -aG sudo user
passwd user && \
su user
sudo apt update -y
sudo apt upgrade -y
sudo apt install -y wget curl sudo gpg htop tzdata 



curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.3-1_amd64.deb
sudo dpkg -i influxdb2_2.7.3-1_amd64.deb
wget https://dl.influxdata.com/influxdb/releases/influxdb2-client-2.7.3-linux-amd64.tar.gz
tar xvzf influxdb2-client-2.7.3-linux-amd64.tar.gz
sudo cp ./influx /usr/local/bin/

service influxd start
# oder influxdb
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




# wget -q https://repos.influxdata.com/influxdata-archive_compat.key
# echo '393e8779c89ac8d958f81f942f9ad7fb82a25e133faddaf92e15b16e6ac9ce4c influxdata-archive_compat.key' | sha256sum -c && cat influxdata-archive_compat.key | gpg --dearmor | sudo tee /etc/apt/trusted.gpg.d/influxdata-archive_compat.gpg > /dev/null
# echo 'deb [signed-by=/etc/apt/trusted.gpg.d/influxdata-archive_compat.gpg] https://repos.influxdata.com/debian stable main' | sudo tee /etc/apt/sources.list.d/influxdata.list

# sudo apt-get update -y
# sudo apt-get install influxdb2 -y
# wget https://dl.influxdata.com/influxdb/releases/influxdb2-2.7.3-amd64.deb
# sudo dpkg -i influxdb2-2.7.3-amd64.deb

# curl -O https://dl.influxdata.com/influxdb/releases/influxdb2_2.7.3-1_amd64.deb
# sudo dpkg -i influxdb2_2.7.3-1_amd64.deb
# wget https://dl.influxdata.com/influxdb/releases/influxdb2-client-2.7.3-linux-amd64.tar.gz
# tar xvzf path/to/influxdb2-client-2.7.3-linux-amd64.tar.gz
# sudo cp influxdb2-client-2.7.3-linux-amd64/influx /usr/local/bin/

# sudo service influxdb start
# influx org create -n katrin

# influx config create \
#   --config-name get-started \
#   --host-url http://localhost:8086 \
#   --org katrin \
#   --token u7Ek4P5s0Nle61QQF1nNA3ywL1JYZky6rHRXxkPBX5bY4H3YFJ6T4KApWSRhaKNj_kHgx70ZLBowB6Di4t2YXg== \
#   --active \
#   -p katrin:InfluxPW 

# influx bucket create \
#     --name katrindb \
#      --org katrin \
#  unauthorises
# influx user create -n katrin -p InfluxPW -o katrin
# unauthorises


# curl https://clickhouse.com/ | sh
# ./clickhouse server  > CH.log &



# sudo apt install -y gnupg postgresql postgresql-common apt-transport-https lsb-release wget
# sudo sh /usr/share/postgresql-common/pgdg/apt.postgresql.org.sh  
# echo "deb https://packagecloud.io/timescale/timescaledb/debian/ $(lsb_release -c -s) main" | sudo tee /etc/apt/sources.list.d/timescaledb.list
# wget --quiet -O - https://packagecloud.io/timescale/timescaledb/gpgkey | sudo apt-key add -
# sudo apt install -y timescaledb-2-postgresql-14
# sudo apt-get install -y postgresql-client
# sudo systemctl restart postgresql

# TODO PG / TS TUNE



 

curl https://github.com/VictoriaMetrics/VictoriaMetrics/releases/download/v1.94.0/victoria-metrics-darwin-amd64-v1.94.0.tar.gz
tar -x victoria-metrics-darwin-amd64-v1.94.0.tar.gz
victoria-metrics-prod  -retentionPeriod=9y > VM.log &

sudo apt install -y pip
pip install --user 'glances[all]'
sudo apt-get install glances 
glances -w --disable-webui &