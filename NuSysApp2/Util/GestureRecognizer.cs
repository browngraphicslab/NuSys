using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class GestureRecognizer
    {
        public enum GestureType
        {
            None, Scribble, SELECTION
        }
        public static GestureType Classify(InqLineModel currentStroke)
        {
            var outerRect = Geometry.PointCollecionToBoundingRect(currentStroke.Points.ToList());
            var area = outerRect.Width * SessionController.Instance.SessionView.ActualWidth * outerRect.Height*SessionController.Instance.SessionView.ActualHeight;
            if (area < 1.0)
                return  GestureType.None;

            var first = currentStroke.Points.First();
            var last = currentStroke.Points.Last();
            if ((Math.Abs(first.X - last.X) < 0.0005 && Math.Abs(first.Y - last.Y) < 0.0005))
            {
                return GestureType.SELECTION;
            }



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
                new Point(points.Sum(p => p.X - mid.X), points.Sum(p => p.Y - mid.Y)));//NOT ACTUAL NAME, I need to find/come up with one
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
            var avgAng = nangles.Sum(x => x) / npoints.Count;
            //
            //WriteData(LSE + "," + entropy + "," + linearity + "," + avgAng);

            if (0.6 * avgAng - 0.6 - entropy < 0)
            {
                return GestureType.Scribble;
            }
            return GestureType.None;
        }
        private static double LineLength(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
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
            Windows.Storage.StorageFolder storageFolder =
                  Windows.Storage.ApplicationData.Current.LocalFolder;

            Windows.Storage.StorageFile sampleF =
                await storageFolder.GetFileAsync("datacwla.txt");
            await Windows.Storage.FileIO.AppendTextAsync(sampleF, data + "\n");
        }
    }
}