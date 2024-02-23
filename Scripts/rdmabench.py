#! /bin/python3
import pandas as pd
import numpy as np

def clean_results(file):
    results = pd.read_csv(file)
    results['Date'] = pd.to_datetime(results['Date'], unit='ns')
    results = results.sort_values(by=['TargetDatabase', 'Date'])
    return results.set_index('Date')


def ingestion_rate(rmetrics='./Metrics-rdma.csv', metrics='./Metrics-sock.csv'):

    def group_ingestion_rate(group):
        dates = group.index
        min_date = dates.min()
        max_date = dates.max()
        all_values = group['SucceededDataPoints'].sum()
        time = (max_date - min_date).total_seconds()
        group['IngestionRatePoint'] = group['SucceededDataPoints'] / (group['Latency'] / 1e3)
        lat_sum = group['Latency'].sum() / 1e3
        return pd.Series({ 'IngestionRateAll': all_values / time, 'TotalTime': time, 'TotalPoints': all_values, 'IngestionRateMean': group['IngestionRatePoint'].mean(), 'IngestionRateBySum': all_values / lat_sum })

    rdma_results = clean_results(rmetrics)
    rdma_results['Type'] = 'RDMA'
    sock_results = clean_results(metrics)
    sock_results['Type'] = 'SOCK'

    results = pd.concat([rdma_results, sock_results])
    return results.groupby('Type').apply(group_ingestion_rate)

if __name__ == '__main__':
    dfs = ingestion_rate()
    print(dfs.to_markdown())
