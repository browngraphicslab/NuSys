using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class Statistics
    {
        public static double EuclideanDistance(IList<double> t0, IList<double> t1)
        {
            if (t0.Count != t1.Count)
                throw new Exception("StockTimeSeries must have the same number of data points.");

            double error = 0;
            for (int i = 0; i < t0.Count; ++i)
            {
                double diff = Math.Abs(t0[i] - t1[i]);
                error += diff*diff;
            }
            return error;
        }

        public static List<int> Bucketize(IEnumerable<decimal> source, int totalBuckets)
        {
            var min = source.Min();
            var max = source.Max();
            var buckets = new List<int>(new int[totalBuckets]);

            var bucketSize = (max - min) / totalBuckets;
            foreach (var value in source)
            {
                int bucketIndex = 0;
                if (bucketSize > (decimal)0.0)
                {
                    bucketIndex = (int)((value - min) / bucketSize);
                    if (bucketIndex == totalBuckets)
                    {
                        bucketIndex--;
                    }
                }
                buckets[bucketIndex]++;
            }

            return buckets;
        }

        public static List<int> Bucketize(IEnumerable<double> source, double min, double max, int totalBuckets)
        {
            var buckets = new List<int>(new int[totalBuckets]);

            var bucketSize = (max - min) / totalBuckets;
            foreach (var value in source)
            {
                int bucketIndex = 0;
                if (bucketSize > 0.0)
                {
                    bucketIndex = (int)((value - min) / bucketSize);
                    if (bucketIndex == totalBuckets)
                    {
                        bucketIndex--;
                    }
                }
                buckets[bucketIndex]++;
            }

            return buckets;
        }
    }
}
