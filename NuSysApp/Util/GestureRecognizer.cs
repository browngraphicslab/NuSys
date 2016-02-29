using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;

namespace NuSysApp
{
    public class GestureRecognizer
    {
        public enum GestureType
        {
            None, Scribble,Square,Circle
        }
        public static async Task<GestureType> testGesture(InqLineModel currentStroke)
        {
            var points = currentStroke.Points;
            var mid = new Point((points[0].X + points[points.Count - 1].X) / 2, (points[0].Y + points[points.Count - 1].Y) / 2);

            // DownScaled Angle averages
            //var npoints = resample(points, 32);
            //var sampledCentroid = centroid(npoints);
            var Centroid = centroid(points);

            var CentroidtoMidpoint = LineLength(Centroid, mid) / StrokeLength(points) * Constants.MaxCanvasSize; //distance between the centroid and the midpoint
            var FirsttoLastPoint = LineLength(points[0], points[points.Count -1]) / StrokeLength(points) * Constants.MaxCanvasSize;//the distance between the first and last point
            var InkStrokeLength = StrokeLength(points);//the length of the whole stroke
//            Debug.WriteLine("determiner " + ((InkStrokeLength - FirsttoLastPoint)/CentroidtoMidpoint));
//            Debug.WriteLine("StrokeLinetoFirsttoLastLine difference " + (InkStrokeLength - FirsttoLastPoint));
            var match = await recognize(points);//tests the stroke against the set of template strokes that are available
            Debug.WriteLine(match.GestureType.ToString() + ":" + match.Score);
            if (match.Score < 30)
            {
                return match.GestureType;
            }
            var determiner = (InkStrokeLength - FirsttoLastPoint)/CentroidtoMidpoint;//used to determine if a stroke is a scribble or not
            var determinerLowerBounds = -6;
            var determinerUpperBounds = -1.5;
            var lDisUpperBounds = -8e4;
            if (determiner < determinerUpperBounds && determiner > determinerLowerBounds || determiner < determinerLowerBounds && InkStrokeLength - FirsttoLastPoint > lDisUpperBounds)
            {
                Debug.WriteLine("this is a scribble");
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
            for (var i = 0; i < points.Count - 1; i++)
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
        public async static Task<Match> recognize(ObservableCollection<Point2d> stroke)
        {

            stroke = resample(stroke, 32);
            List<Double> candidateSeries = znormalize(cDistance(stroke));

            var filedict = new Dictionary<string,GestureType >()
            {
                { "Circle",GestureType.Circle },
                { "Circle2",GestureType.Circle },
                { "Square",GestureType.Square }
            };

            double minMatch = Double.MaxValue;
            GestureType strokeMatch = GestureType.None;
            foreach (var trainingTemplate in filedict.Keys)
            {
                string filepath = @"Assets\gestureTemplates\" +trainingTemplate + @".stroke";
                StorageFolder InstallationFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;

                StorageFile file = await InstallationFolder.GetFileAsync(filepath);
                var stream = await file.OpenStreamForReadAsync();
                var templatePoints = new ObservableCollection<Point2d>();
                try
                {
                    using (StreamReader sr = new StreamReader(stream))
                    {
                        while(sr.Peek() >= 0)
                        {
                            var input = sr.ReadLine();
                            var splitInput = input.Split(',');
                            templatePoints.Add(new Point2d(Double.Parse(splitInput[0]),Double.Parse(splitInput[1])));
                        }

                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                templatePoints = resample(templatePoints, 32);
                List<Double> trainSeries = znormalize(cDistance(templatePoints));

                double distance = l2(candidateSeries, trainSeries);

                if (distance < minMatch)
                {
                    minMatch = distance;
                    strokeMatch = filedict[trainingTemplate];
                }
            }

            return new Match(strokeMatch, minMatch);
        }


        private static double l2(List<Double> s, List<Double> t)
        {
            int N = s.Count() < t.Count() ? s.Count() : t.Count();

            double diff = 0;

            for (int i = 0; i < N; i++)
            {
                diff += Math.Pow(s[i] - t[i], 2);
            }

            return diff;
        }

        private static List<Double> cDistance(ObservableCollection<Point2d> stroke)
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
        public static Point  centroid(ObservableCollection<Point2d> stroke)
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



        private static ObservableCollection<Point2d> resample(ObservableCollection<Point2d> s, int n)
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
                    double qx = pim1.X + ((I - D) / d) * (pi.X - pim1.X);
                    double qy = pim1.Y + ((I - D) / d) * (pi.Y - pim1.Y);
                    Point q = new Point(qx, qy);
                    newPoints.Add(new Point2d(q.X, q.Y));
                    points.Insert(i, new Point2d(q.X, q.Y));
                    D = 0;
                }
                else
                {
                    D = D + d;
                }
            }

            return newPoints;
        }


        private static List<Double> znormalize(List<Double> numbers)
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
        private static double avg(List<Double> numbers)
        {
            double sum = 0;

            foreach (var d in numbers) sum += d;

            return sum / numbers.Count();
        }

        /**
         * Returns the standard deviation of the list of numbers.
         * @param numbers
         * @param avg
         * @return
         */
        private static double std(List<Double> numbers, double avg)
        {
            var sum = 0.0;

            foreach (var d in numbers)
            {
                sum += Math.Pow(d - avg, 2);
            }
            return Math.Sqrt(sum / numbers.Count());
        }


        public class Match
        {

            private GestureType _st;
            private double _score;
            public Match(GestureType st, double score)
            {
                _st = st;
                _score = score;
            }
            public GestureType GestureType { get { return _st; } }
            public double Score { get { return _score; } }
        }


    }
}