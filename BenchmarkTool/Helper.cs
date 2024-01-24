using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BenchmarkTool
{
    public static class Helper
    {
        private static DateTime Epoch = new DateTime(1970, 1, 1);

        public static T ToEnum<T>(this string value, bool ignoreCase = true)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase);
        }


        /// <summary>
        /// Return a CSV string of the values in the list
        /// </summary>
        /// <param name="intValues"></param>
        /// <param name="separator"></param>
        /// <param name="encloser"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string ToCSV<T>(this IEnumerable<T> intValues, string separator = ",", string encloser = "")
        {
            string result = String.Empty;
            foreach (T value in intValues)
            {
                result = String.IsNullOrEmpty(result)
                    ? string.Format("{1}{0}{1}", value, encloser)
                    : String.Format("{0}{1}{3}{2}{3}", result, separator, value, encloser);
            }
            return result;
        }

        public static async Task<List<QueryStatusWrite>> ParallelForEachAsync(
            this IEnumerable<ClientWrite> source,
            Func<ClientWrite, Task<List<QueryStatusWrite>>> func,
            int maxDegreeOfParallelism)
        {
            async Task<List<QueryStatusWrite>> AwaitPartition(IEnumerator<ClientWrite> partition)
            {
                var results = new List<QueryStatusWrite>();
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await Task.Yield(); // prevents a sync/hot thread hangup
                        var partitionResults = await func(partition.Current);
                        results.AddRange(partitionResults);
                    }
                }
                return results;
            }

            var results = await Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDegreeOfParallelism)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
            return results.SelectMany(r => r).ToList();
        }


        public static async Task<List<QueryStatusRead>> ParallelForEachAsync(
       this IEnumerable<ClientRead> source,
       Func<ClientRead, Task<List<QueryStatusRead>>> func,
       int maxDegreeOfParallelism)
        {
            async Task<List<QueryStatusRead>> AwaitPartition(IEnumerator<ClientRead> partition)
            {
                var results = new List<QueryStatusRead>();
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await Task.Yield(); // prevents a sync/hot thread hangup
                        var partitionResults = await func(partition.Current);
                        results.AddRange(partitionResults);
                    }
                }
                return results;
            }

            var results = await Task.WhenAll(
                Partitioner
                    .Create(source)
                    .GetPartitions(maxDegreeOfParallelism)
                    .AsParallel()
                    .Select(p => AwaitPartition(p)));
            return results.SelectMany(r => r).ToList();
        }


        public static long GetMilliEpoch()
        {
           TimeSpan t = DateTime.Now - Epoch;
            return  (long) t.TotalMilliseconds;
        }

        public static long GetNanoEpoch()
        { 
            TimeSpan t = DateTime.Now - Epoch;
            return t.Ticks * 100;
        }
    }
}
