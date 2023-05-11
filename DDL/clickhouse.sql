CREATE DATABASE IF NOT EXISTS katrindb;
CREATE TABLE IF NOT EXISTS sensor_data ( time DateTime64(9) CODEC(Gorilla), sensor_id Int32, value Float32) ENGINE = MergeTree() PARTITION BY toYYYYMMDD(time) ORDER BY (sensor_id, time);
