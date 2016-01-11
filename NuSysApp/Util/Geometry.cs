using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    /// <summary>
    ///     This static class performs all visual gemoetry calculations.
    /// </summary>
    public static class Geometry
    {
        /// <summary>
        ///     Checks if two lines intersect.
        /// </summary>
        public static bool LinesIntersect(Line line1, Line line2)
        {
            var AB = new Segment
            {
                Start = new Point(line1.X1, line1.Y1),
                End = new Point(line1.X2, line1.Y2)
            };
            var CD = new Segment
            {
                Start = new Point(line2.X1, line2.Y1),
                End = new Point(line2.X2, line2.Y2)
            };

            var dy_AC = AB.Start.Y - CD.Start.Y;
            var dx_DC = CD.End.X - CD.Start.X;
            var dx_AC = AB.Start.X - CD.Start.X;
            var dy_DC = CD.End.Y - CD.Start.Y;
            var dx_BA = AB.End.X - AB.Start.X;
            var dy_BA = AB.End.Y - AB.Start.Y;

            var denom = (dx_BA*dy_DC) - (dy_BA*dx_DC);
            var num = (dy_AC*dx_DC) - (dx_AC*dy_DC);

            if (denom == 0)
            {
                if (num == 0)
                {
                    //Segments are collinear
                    if (AB.Start.X >= CD.Start.X && AB.Start.X <= CD.End.X)
                    {
                        return true;
                    }
                        //Segments are collinear
                    if (CD.Start.X >= AB.Start.X && CD.Start.X <= AB.End.X)
                    {
                        return true;
                    }
                    return false;
                }
                //Segments are parallel
                return false;
            }

            var r = num/denom;
            if (r < 0 || r > 1)
            {
                return false;
            }

            var s = (dy_AC*dx_BA - dx_AC*dy_BA)/denom;
            if (s < 0 || s > 1)
            {
                return false;
            }

            return true;
        }

        public struct Segment
        {
            public Point End;
            public Point Start;
        }

        public static Line[] NodeToLineSegment(NodeViewModel node)
        {
            var lines = new Line[4];
            var nodeModel = (NodeModel) node.Model;
            var x = nodeModel.X + node.Transform.TranslateX;
            var y = nodeModel.Y + node.Transform.TranslateY;

            //AB line  
            lines[0] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x + node.Width,
                Y2 = y
            };

            //CD line 
            lines[1] = new Line
            {
                X1 = x,
                Y1 = y + node.Height,
                X2 = x + node.Width,
                Y2 = y + node.Height
            };

            //AC line 
            lines[2] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y + node.Height
            };

            //BC line 
            lines[3] = new Line
            {
                X1 = x + node.Width,
                Y1 = y,
                X2 = x + node.Width,
                Y2 = y + node.Height
            };

            return lines;
        }

        public static Rect NodeToBoudingRect(NodeViewModel nodeVm)
        {
            return new Rect()
            {
                Height = ((NodeModel)(nodeVm.Model)).Height,
                Width = ((NodeModel)(nodeVm.Model)).Width,
                X = ((NodeModel)(nodeVm.Model)).X,
                Y = ((NodeModel)(nodeVm.Model)).Y
            };
        }

        public static Rect PointCollecionToBoundingRect(List<Point2d> pc)
        {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            foreach (var point in pc)
            {
                minX = point.X < minX ? point.X : minX;
                minY = point.Y < minY ? point.Y : minY;
                maxX = point.X > maxX ? point.X : maxX;
                maxY = point.Y > maxY ? point.Y : maxY;
            }
            return new Rect(minX,minY,maxX-minX,maxY-minY);
        }

        public static Rect InqToBoudingRect(InqLineModel inqM)
        {
            var points = inqM.Points;
            var rect = new Rect()
            {
                Height = 0,
                Width = 0,
                X = Double.MaxValue,
                Y = Double.MaxValue
            };
            foreach (Point p in points)
            {
                if (p.X < rect.X)
                {
                    rect.Width = rect.Width + rect.X - p.X;
                    rect.X = p.X;
                }
                if (p.X - rect.X > rect.Width)
                {
                    rect.Width = p.X - rect.X;
                }
                if (p.Y < rect.Y)
                {
                    rect.Height = rect.Height + rect.Y - p.Y;
                    rect.Y = p.Y;
                }
                if (p.Y - rect.Y > rect.Height)
                {
                    rect.Height = p.Y - rect.Y;
                }
            }

            return rect;
        }
    }
}