CREATE DATABASE katrindb2;
CREATE TABLE IF NOT EXISTS public.sensor_data (
    "time" timestamp(6) with time zone NOT NULL,
    sensor_id integer,
    value double precision
);
CREATE INDEX ON sensor_data(time DESC);
CREATE INDEX ON sensor_data(sensor_id, time DESC); --UNIQUE
