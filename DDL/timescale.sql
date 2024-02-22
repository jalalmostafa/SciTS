CREATE DATABASE katrindb;
CREATE TABLE IF NOT EXISTS public.sensor_data (
    "time" timestamp(6) with time zone NOT NULL,
    sensor_id integer,
    value double precision
);
CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;
CREATE INDEX ON sensor_data(time DESC);
CREATE INDEX ON sensor_data(sensor_id, time DESC); -- UNIQUE
SELECT * FROM create_hypertable('sensor_data', 'time', chunk_time_interval => 43200000); -- chunk size 12 hours
ALTER TABLE sensor_data SET (timescaledb.compress, timescaledb.compress_orderby = 'sensor_id, time DESC');
SELECT add_compression_policy('sensor_data', INTERVAL '7 days');
