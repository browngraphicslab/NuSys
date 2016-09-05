﻿using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp2
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

        public static Line[] NodeToLineSegment(ElementViewModel node)
        {
            var lines = new Line[4];
            var nodeModel = (ElementModel) node.Model;
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
        public static Line[] RectToLineSegment(Rect rect)
        {
            var lines = new Line[4];
            var x = rect.X;
            var y = rect.Y; 

            //AB line  
            lines[0] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x + rect.Width,
                Y2 = y
            };

            //CD line 
            lines[1] = new Line
            {
                X1 = x,
                Y1 = y + rect.Height,
                X2 = x + rect.Width,
                Y2 = y + rect.Height
            };

            //AC line 
            lines[2] = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y + rect.Height
            };

            //BC line 
            lines[3] = new Line
            {
                X1 = x + rect.Width,
                Y1 = y,
                X2 = x + rect.Width,
                Y2 = y + rect.Height
            };

            return lines;
        }

        public static Rect NodeToBoudingRect(ElementViewModel nodeVm)
        {
            return new Rect()
            {
                Height = ((ElementModel)(nodeVm.Model)).Height,
                Width = ((ElementModel)(nodeVm.Model)).Width,
                X = ((ElementModel)(nodeVm.Model)).X,
                Y = ((ElementModel)(nodeVm.Model)).Y
            };
        }

        public static Point2d GetRectCenter(Rect rect)
        {
           
            return new Point2d(rect.X + rect.Width/2, rect.Y + rect.Height/2);
        }

        public static Rect NodesToBoudingRect(List<ElementViewModel> nodeVm)
        {
            var points = new List<Point2d>();
            foreach (var vm in nodeVm)
            {
                points.Add(new Point2d(vm.Transform.TranslateX, vm.Transform.TranslateY));
                points.Add(new Point2d(vm.Transform.TranslateX + vm.Width, vm.Transform.TranslateY));
                points.Add(new Point2d(vm.Transform.TranslateX, vm.Transform.TranslateY + vm.Height));
                points.Add(new Point2d(vm.Transform.TranslateX + vm.Width, vm.Transform.TranslateY + vm.Height));
            }

            return PointCollecionToBoundingRect(points);
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
            var min = new Point(Double.MaxValue, Double.MaxValue);
            var max = new Point(Double.MinValue, Double.MinValue);
            foreach (Point p in points)
            {
                if (p.X > max.X)
                {
                    max.X = p.X;
                }
                if (p.Y > max.Y)
                {
                    max.Y = p.Y;
                }
                if (p.X < min.X)
                {
                    min.X = p.X;
                }
                if (p.Y < min.Y)
                {
                    min.Y = p.Y;
                }
            }

            return new Rect()
            {
                Height = (max.X - min.X) * ((double)Constants.MaxCanvasSize),
                Width = (max.Y - min.Y) * ((double)Constants.MaxCanvasSize),
                X = min.X * ((double)Constants.MaxCanvasSize),
                Y = min.Y *((double)Constants.MaxCanvasSize) 
            };
        }
    }
}