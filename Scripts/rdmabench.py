#! /bin/python3
import pandas as pd
import numpy as np
import os
import sys

def clean_results(file):
    if not os.path.exists(file):
        print(f'{file} does not exist!')
        return pd.DataFrame()
    results = pd.read_csv(file)
    results['Date'] = pd.to_datetime(results['Date'], unit='ns')
    results = results.sort_values(by=['TargetDatabase', 'Date'])
    return results.set_index('Date')

def set_type(file, typ):
    results = clean_results(file)
    results['Type'] = typ
    return results

def ingestion_rate(path='.', rmetrics='Metrics-rdma.csv', metrics='Metrics-sock.csv'):

    def group_ingestion_rate(group):
        dates = group.index
        min_date = dates.min()
        max_date = dates.max()
        all_values = group['SucceededDataPoints'].sum()
        time = (max_date - min_date).total_seconds()
        group['IngestionRatePoint'] = group['SucceededDataPoints'] / (group['Latency'] / 1e3)
        lat_sum = group['Latency'].sum() / 1e3
        return pd.Series({ 'IngestionRateAll': all_values / time, 'TotalTime': time, 'TotalPoints': all_values, 'IngestionRateMean': group['IngestionRatePoint'].mean(), 'IngestionRateBySum': all_values / lat_sum })

    rdma_results = set_type(f'{path}/{rmetrics}', 'RDMA')
    sock_results = set_type(f'{path}/{metrics}', 'SOCK')
    results = pd.concat([rdma_results, sock_results])
    if len(results) == 0:
        return pd.Series()
    return results.groupby(['Type', 'ClientsNumber']).apply(group_ingestion_rate)

if __name__ == '__main__':
    dfs = ingestion_rate(path=os.path.abspath(sys.argv[1] if len(sys.argv) > 1 else '.'))
    print(dfs.to_markdown())
