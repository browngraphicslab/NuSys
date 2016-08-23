using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Foundation;


namespace NuSysApp
{
    public class MathUtil
    {
        public static double EPSILON = 0.0000001;

        public static LineEquation ApproximateLine(IList<double> values)
        {
            double sumX = 0;
            for (int i = 0; i < values.Count; ++i)
            {
                sumX += i;
            }

            int n = values.Count;
            double ssXX = 0;
            double ssYY = 0;
            double ssXY = 0;
            double meanX = sumX / n;
            double meanY = values.Sum() / n;
            for (int i = 0; i < values.Count; ++i)
            {
                ssXX += (double)Math.Pow(i - meanX, 2);
                ssYY += (double)Math.Pow(values[i] - meanY, 2);
                ssXY += (i - meanX)*(values[i] - meanY);
            }

            double b = ssXY / ssXX;
            double a = meanY - b * meanX;

            return new LineEquation(a, b);
        }

        public static LineEquation ApproximateLine(IList<Point> values, bool addXEpsilon = false)
        {
            // Adds a small epsilon to each X values so that if all values share the same X value,
            // still a line can be approximated
            if (addXEpsilon)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    values[i] = new Point(values[i].X + 0.0001*i, values[i].Y);
                }
            }

            double sumX = values.Sum(t => t.X);
            double sumY = values.Sum(t => t.Y);

            int n = values.Count;
            double ssXX = 0;
            double ssYY = 0;
            double ssXY = 0;
            double meanX = sumX/n;
            double meanY = sumY/n;

            foreach (Point t in values)
            {
                ssXX += (float) Math.Pow(t.X - meanX, 2);
                ssYY += (float) Math.Pow(t.Y - meanY, 2);
                ssXY += (t.X - meanX)*(t.Y - meanY);
            }

            double b = ssXY/ssXX;
            double a = meanY - b*meanX;

            if (double.IsNaN(b) || double.IsNaN(a))
            {
                Debug.WriteLine("as");
            }

            return new LineEquation((float) a, (float) b);
        }

        public static IList<double> ResampleLinear(IList<double> values, int numSamples)
        {
            IList<double> oldSamples = values;
            double scale = numSamples / (float)oldSamples.Count();
            var newSamples = new List<double>();
            for (int j = 0; j < numSamples; ++j)
            {
                newSamples.Add(0);
            }

            double radius = scale > 1 ? 1 : 1 / scale;
            for (int i = 0; i < numSamples; ++i)
            {
                double center = i / scale + (1 - scale) / 2;
                var left = (int) Math.Ceiling(center - radius);
                var right = (int) Math.Floor(center + radius);

                double sum = 0;
                double sumWeights = 0;
                for (int k = left; k <= right; k++)
                {
                    double weight = (scale >= 1) ? 1 - Math.Abs(k - center) : 1 - Math.Abs((k - center) * scale);
                    int index = Math.Max(0, Math.Min(oldSamples.Count - 1, k));
                    sum += weight*oldSamples[index];
                    sumWeights += weight;
                }
                sum /= sumWeights;
                newSamples[i] = sum;
            }

            return newSamples;
        }

        public static float Mean(IList<float> numbers)
        {
            return numbers.Sum()/numbers.Count;
        }

        public static float Distance(Point p1, Point p2)
        {
            var p = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return (float) Math.Sqrt(p.X*p.X + p.Y*p.Y);
        }

        public static double Clamp(double min, double max, double val)
        {
            return Math.Max(min, Math.Min(max, val));
        }

        public static float[] Haar1D(float[] vecOrg)
        {
            var vec = (float[]) vecOrg.Clone();
            int w = vecOrg.Length;
            var vecp = new float[vecOrg.Length];
            var sqrt2 = (float) Math.Sqrt(2.0);

            while (w > 1)
            {
                w /= 2;
                for (int i = 0; i < w; i++)
                {
                    vecp[i] = (vec[2*i] + vec[2*i + 1])/sqrt2;
                    vecp[i + w] = (vec[2*i] - vec[2*i + 1])/sqrt2;
                }

                for (int i = 0; i < (w*2); i++)
                    vec[i] = vecp[i];
            }
            return vec;
        }

        public static double[] Dwt(double[] data)
        {
            var d = new List<double>(data);
            var r = new List<double>();
            int n = d.Count;
            while (n > 1)
            {
                n /= 2;
                var dNew = new List<double>(n);
                var rNew = new List<double>(n);
                for (int i = 0; i < n*2; i += 2)
                {
                    double avg = (d[i] + d[i + 1]) * 0.5f;
                    dNew.Add(avg);
                    rNew.Add((d[i] - d[i + 1])*0.5f);
                }
                r.InsertRange(0, rNew);
                d = dNew;
            }
            r.Insert(0, d[0]);
            return r.ToArray();
        }
        
        public static IList<double> Normalize(IList<double> data,double min, double max )
        {
            var newData = new List<double>(data.Count);
            foreach (double f in data)
            {
                newData.Add((f - min) / max);
            }
            return newData;
        }

        public static bool IsWithinRange(double min, double max, double val)
        {
            return (val >= min && val <= max);
        }

        public static double Dist(Vector2 p0, Vector2 p1)
        {
            return Math.Sqrt(Math.Pow(p1.X - p0.X, 2) + Math.Pow(p1.Y - p0.Y, 2));
        }

        public static double Dist(Point p0, Point p1)
        {
            return Math.Sqrt(Math.Pow(p1.X - p0.X, 2) + Math.Pow(p1.Y - p0.Y, 2));
        }

        public static Point GetPointOnBezierCurve(Point p0, Point p1, Point p2, Point p3, double t)
        {
            var x = (1 - t) * (1 - t) * (1 - t) * p0.X + 3 * (1 - t) * (1 - t) * t * p1.X + 3 * (1 - t) * t * t * p2.X + t * t * t * p3.X;
            var y = (1 - t) * (1 - t) * (1 - t) * p0.Y + 3 * (1 - t) * (1 - t) * t * p1.Y + 3 * (1 - t) * t * t * p2.Y + t * t * t * p3.Y;
            return new Point(x, y);
        }
    }
}