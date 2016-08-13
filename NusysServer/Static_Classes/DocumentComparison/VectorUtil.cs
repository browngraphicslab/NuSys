using System.Collections.Generic;
using System.Diagnostics;

namespace NusysServer.DocumentComparison
{
    public class VectorUtil
    {
        public static double tfidfDictionaryDotProduct(Dictionary<int, double> d1, Dictionary<int, double> d2)
        {
            double sum = 0.0;
            foreach (var key in d1.Keys)
            {
                if (d2.ContainsKey(key))
                {
                    sum += d1[key] * d2[key];
                }
            }

            return sum;
        }

        public static double CosineDistance(Dictionary<int, double> d1, Dictionary<int, double> d2)
        {
            //Debug.WriteLine(1 - tfidfDictionaryDotProduct(d1, d2));
            return 1 - tfidfDictionaryDotProduct(d1, d2);
        }
    }
}