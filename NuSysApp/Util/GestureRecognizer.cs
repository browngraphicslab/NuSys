using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class GestureRecognizer
    {
        public enum GestureType
        {
            None, Scribble
        }
        public static GestureType testGesture(InqLineModel currentStroke)
        {
            List<double> angles = new List<double>();
            var points = currentStroke.Points;
            for (int i = 0; i < points.Count - 2; i++)
            {

                angles.Add(Math.Acos((Math.Pow(LineLength(points[i + 1], points[i]), 2) +
                    Math.Pow(LineLength(points[i + 1], points[i + 2]), 2) -
                    Math.Pow(LineLength(points[i], points[i + 2]), 2))
                    / (2 * LineLength(points[i + 1], points[i]) * LineLength(points[i + 1], points[i + 2]))));
            }
            angles.RemoveAll(d => Double.IsNaN(d) | d == 0);//this is to clean up the data and prevent errors
            var discreteSet = DiscretizeAngles(angles, 6);
            discreteSet.RemoveAll(d => Double.IsNaN(d) | d == 0);
            var entropy = -discreteSet.Sum(i => i * Math.Log(i));
            var xy = points.Sum(p => p.X) * points.Sum(p => p.Y);
            var m = (points.Count * points.Sum(p => p.X * p.Y) - xy) /
                (points.Count * points.Sum(p => p.X * p.X) - xy);
            var b = (points.Sum(p => p.Y) - m * points.Sum(p => p.X)) / points.Count;
            var LSE = points.Sum(p => Math.Pow(m * p.X + b - p.Y, 2));
            var mid = new Point((points[0].X + points[points.Count - 1].X) / 2, (points[0].Y + points[points.Count - 1].Y) / 2);
            var linearity = LineLength(new Point(),
                new Point(points.Sum(p => p.X - mid.X), points.Sum(p => p.Y - mid.Y))) / StrokeLength(points);//NOT ACTUAL NAME, I need to find/come up with one
            Debug.WriteLine(LSE + " : " + entropy + " : " + linearity);
            // DownScaled Angle averages
            var npoints = new List<Point>();
            for (int i = 0; i < 10; i++)
            {
                npoints.Add(points[i * (int)(points.Count / 10)]);
            }
            List<double> nangles = new List<double>();
            for (int i = 0; i < npoints.Count - 2; i++)
            {
                nangles.Add(Math.Acos((Math.Pow(LineLength(npoints[i + 1], npoints[i]), 2) +
                    Math.Pow(LineLength(npoints[i + 1], npoints[i + 2]), 2) -
                    Math.Pow(LineLength(npoints[i], npoints[i + 2]), 2))
                    / (2 * LineLength(npoints[i + 1], npoints[i]) * LineLength(npoints[i + 1], npoints[i + 2]))));
            }
            nangles.RemoveAll(d => Double.IsNaN(d) | d == 0);
            var avgAng = nangles.Sum(x => x) / nangles.Count;
            var avgAngfull = angles.Sum(x => x) / angles.Count;
            //
            WriteData(LSE + "," + entropy + "," + linearity + "," + avgAng + "," + avgAngfull);

            //if (0.6 * avgAng - 0.6 - entropy < 0)
            if (avgAng < 2)
            {
                return GestureType.Scribble;
            }
            return GestureType.None;
        }
        private static double LineLength(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }

        private static double StrokeLength(ObservableCollection<Point2d> points)
        {
            double result = 0;
           for (var i =0 ; i < points.Count -1; i ++)
           {
               result += LineLength(points[i], points[i + 1]);
           }
            return result;
        }

        private static List<double> DiscretizeAngles(List<double> values, int bins)
        {
            List<double> result = new List<double>();
            values.Sort();
            for (var i = 0; i < bins; i++)
            {
                result.Add(0);
            }
            double step = Math.PI / 2 / bins;
            int stepNumber = 1;
            foreach (double d in values)
            {
                bool isDone = false;
                while (!isDone)
                    if (d <= Math.PI / 2 + step * stepNumber)
                    {
                        result[stepNumber - 1]++;
                        isDone = true;
                    }
                    else
                    {
                        stepNumber++;
                    }
            }
            for (int i = 0; i < bins; i++)
            {
                result[i] /= values.Count;
            }
            return result;
        }


        public async static void WriteData(string data)
        {
            var filename = "dataNSFAFUB.txt";

            Windows.Storage.StorageFolder storageFolder =
                  Windows.Storage.ApplicationData.Current.LocalFolder;

            

            if (await storageFolder.TryGetItemAsync(filename) == null)
            {
                await storageFolder.CreateFileAsync(filename);
            }

            Windows.Storage.StorageFile sampleF =
                await storageFolder.GetFileAsync(filename);
            await Windows.Storage.FileIO.AppendTextAsync(sampleF, data + "\n");
        }
        public Match recognize(ObservableCollection<Point2d> stroke)
        {

            /** Convert the input stroke into a one dimensional time-series */
            stroke = resample(stroke, 32);
            List<Double> candidateSeries = new OneDimensionalRepresentation(stroke).getSeries();

            /** Find the closest matching training template */
            double minMatch = Double.MaxValue;
            LabeledStroke strokeMatch = null;
            for (NNRTemplate trainingTemplate : this.trainingTemplates)
            {
                /** Convert the template into a one dimensional time-series */
                ArrayList<Double> trainSeries = trainingTemplate.getSeries();

                /** Compute the distance between the input stroke and the training template */
                double distance = l2(candidateSeries, trainSeries);

                if (distance < minMatch)
                {
                    minMatch = distance;
                    strokeMatch = trainingTemplate.ls;
                }
            }

            return new Match(strokeMatch, minMatch, strokeMatch.getLabel());
        }


        private double l2(List<Double> s, List<Double> t)
        {
            int N = s.Count() < t.Count() ? s.Count() : t.Count();

            double diff = 0;

            for (int i = 0; i < N; i++)
            {
                diff += Math.Pow(s[i] - t[i], 2);
            }

            return diff;
        }

        private List<Double> cDistance(ObservableCollection<Point2d> stroke)
        {
            List<Double> distances = new List<Double>();

            Point c = centroid(stroke);

            foreach (var p in stroke)
            {
                double distance = LineLength(c, p);
                distances.Add(distance);
            }

            return distances;
        }

        /**
         * Computes the centroid of the points of the stroke, defined as
         * <avg_x, avg_y>
         * @param stroke
         * @return
         */
        public Point centroid(ObservableCollection<Point2d> stroke)
        {

            double sumX = 0, sumY = 0;

            for (int i = 0; i < stroke.Count(); i++)
            {
                Point p = stroke[i];
                sumX += p.X;
                sumY += p.Y;
            }

            double mx = sumX / stroke.Count();
            double my = sumY / stroke.Count();

            return new Point(mx, my);
        }



        private ObservableCollection<Point2d> resample(ObservableCollection<Point2d> s, int n)
        {
            ObservableCollection<Point2d> points = new ObservableCollection<Point2d>();
            for (int i = 0; i < s.Count(); i++)
            {
                points.Add(s[i]);
            }
            var I = 1.0 * StrokeLength(points) / (n - 1);
            var D = 0.0;

            ObservableCollection<Point2d> newPoints = new ObservableCollection<Point2d>();
            newPoints.Add(points[0]);

            for (int i = 1; i < points.Count(); i++)
            {
                var d = LineLength(points[i - 1], points[i]);

                Point pi = points[i];
                Point pim1 = points[i - 1];

                if (D + d >= I)
                {
                    double qx = pim1.X + ((I - D)/d)*(pi.X - pim1.X);
                    double qy = pim1.Y + ((I - D)/d)*(pi.Y - pim1.Y);
                    Point q = new Point(qx, qy);
                    newPoints.Add(new Point2d(q.X,q.Y));
                    points.Insert(i, new Point2d(q.X,q.Y));
                    D = 0;
                }
                else
                {
                    D = D + d;
                }
            }

            return newPoints;
        }


        private List<Double> znormalize(List<Double> numbers)
        {
            List<Double> normalized = new List<Double>();

            double average = avg(numbers);
            double stdev = std(numbers, average);

            foreach (var d in numbers)
            {
                double z = (d - average) / stdev;
                normalized.Add(z);
            }

            return normalized;
        }

        /**
         * Returns the average of the list of numbers.
         * @param numbers
         * @return
         */
        private double avg(List<Double> numbers)
        {
            double sum = 0;

            foreach (var  d in numbers) sum += d;

            return sum / numbers.Count();
        }

        /**
         * Returns the standard deviation of the list of numbers.
         * @param numbers
         * @param avg
         * @return
         */
        private double std(List<Double> numbers, double avg)
        {
            var sum = 0.0;

            foreach (var d in numbers)
            {
                sum += Math.Pow(d - avg, 2);
            }
            return Math.Sqrt(sum / numbers.Count());
        }

        enum StrokeTemplate
        {
            Square, Circle
        }

        class Match
        {

            private StrokeTemplate _st;
            private double _score;
            public Match (StrokeTemplate st, double score)
            {
                _st = st;
                _score = score;
            }
            public StrokeTemplate StrokeTemplate{get { return _st; } }
            public double Score {get { return _score; } }
        }


    }
}