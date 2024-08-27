import psycopg2
import sys
import random
from datetime import datetime as dt, timedelta

def conn( connString, queryString):
    conn = psycopg2.connect( connString )
        # "host=141.52.65.243 dbname='katrintest' user='postgres' password='P@ssw0rd'")

    # start = dt( sys.argv[2] )
    #     # year=2022, month=1, day=1, hour=0,     minute=0, second=0, microsecond=0)
    # duration = 1440
    # dayspan = 15

    # random_start = start + \
    #     timedelta(days=random.randint(0, 14), hours=random.randint(0, 24),)
    # end = random_start + timedelta(minutes=10)
    # agg_interval = 1
    # max_value = 20000000
    # min_value = 100000

    # query = None
    # query_type = None
    # req = sys.argv[1]

    # if req == 'q1':
    #     query = f"""
    #         SELECT * FROM sensor_data where time >= '{random_start.isoformat()}'
    #         and time <= '{end.isoformat()}' and sensor_id = ANY(ARRAY[1,2,3,4,5,6,7,8,9,10])
    #     """
    #     query_type = 'RangeQueryRawData'
    # elif req == 'q2':
    #     query = f"""
    #        SELECT time_bucket('{agg_interval}h', time) AS time_agg, max(value), min(value) FROM sensor_data
    #         where time >= '{random_start.isoformat()}' and time <= '{end.isoformat()}' and sensor_id = 100
    #         group by time_agg having min(value) < {min_value} OR max(value) > {max_value}
    #     """
    #     query_type = 'OutOfRangeQuery'
    # elif req == 'q3':
    #     query = f"""
    #         SELECT stddev(value) FROM sensor_data where time >= '{random_start.isoformat()}'
    #         and time <= '{end.isoformat()}' and sensor_id = 100
    #     """
    #     query_type = 'STDDevQuery'
    # elif req == 'q4':
    #     query = f"""
    #         SELECT time_bucket('{agg_interval}h', time) AS time_agg, sensor_id, avg(value) FROM sensor_data
    #         where time >= '{random_start.isoformat()}' and time <= '{end.isoformat()}'
    #         and sensor_id = ANY(ARRAY[1,2,3,4,5,6,7,8,9,10]) group by time_agg, sensor_id
    #     """
    #     query_type = 'RangeQueryAggData'
    # elif req == 'q5':
    #     query = f"""
    #         SELECT A1.time_agg, A2.val - A1.val as difference from
    #             (SELECT time_bucket('{agg_interval}h', time) AS time_agg , avg(value) as val FROM sensor_data
    #                 where time >='{random_start.isoformat()}' and time <= '{end.isoformat()}' and sensor_id = 100 group by time_agg)A1
    #         inner join
    #             (SELECT time_bucket('{agg_interval}h', time) AS time_agg , avg(value) as val FROM sensor_data
    #                 where time >='{random_start.isoformat()}' and time <= '{end.isoformat()}' and sensor_id = 200 group by time_agg)A2
    #         On A1.time_agg = A2.time_agg
    #     """
    #     query_type = 'DifferenceAggQuery'

    cur = conn.cursor()
    t0 = dt.now()
    cur.execute( queryString )
    rows = cur.fetchall()
    t1 = dt.now()
    latency = (t1-t0).total_seconds() * 1000 * 1000
    lenR= len(rows)
    return [latency, lenR]

