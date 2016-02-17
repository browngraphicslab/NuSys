using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp.Components
{
    public class SelectionHull
    {
        private List<Point> _points;
        private int _min;
        private Point _rootPoint;
        private SortedDictionary<double, Point> _sortedPoints;
        private Stack<Point> _hullPoints;
        private Polygon _hull;
        private Canvas _mainCanvas;

        public SelectionHull(Polyline lasso, Canvas mainCanvas)
        {
            SessionController.Instance.ActiveWorkspace.DeselectAll();
            _mainCanvas = mainCanvas;
            mainCanvas.Children.Remove(lasso);
            PointCollection pts = lasso.Points;

            _points = new List<Point>(lasso.Points);
            if (_points.Count<5)
                return;
            findBottomLeftMostPoint();
            placeBottomLeftMostPointAtFirstPosition();
            sortPoints();
           
            figureOutConvexHull();
            if (_hullPoints == null)
                return;
            addSelectionHull(mainCanvas);
            selectContainedNodes(SessionController.Instance.ActiveWorkspace.AllContent);
            
        }

        //finds the bottom-left most point by looping thru all of the points
        private void findBottomLeftMostPoint()
        {
            double yMin = _points[0].Y;
            _min = 0;

            //loop thru all points
            for (int i = 1; i < _points.Count(); i++)
            {
                double y = _points[i].Y;
                //pick the bottom-most or choose the left mostt point in case of a tie
                if ((y > yMin) || (y == yMin && _points[i].X < _points[_min].X))
                {
                    yMin = _points[i].Y;
                    _min = i;
                }
            }
        }

        //swaps the bottom left point to be the first position
        private void placeBottomLeftMostPointAtFirstPosition()
        {
            this.swap(_points[0], _points[_min]);
            _rootPoint = _points[0];
        }

        //swaps two points in the points list
        private void swap(Point one, Point two)
        {
            int indexOne = _points.IndexOf(one);
            int indexTwo = _points.IndexOf(two);

            Point temp = one;
            _points[indexOne] = _points[indexTwo];
            _points[indexTwo] = temp;

        }

        //sorts all of the points by polar angle in counterclockwise order around the bottom-left-most point
        private void sortPoints()
        {
            _sortedPoints = new SortedDictionary<double, Point>();
            for (int i = 0; i < _points.Count(); i++)
            {
                Point point = _points[i];
                double quant = quantifyAngle(point);

                //If the list doesn't have a pointat that angle yet, then add it
                if (!_sortedPoints.ContainsKey(quant))
                {
                    _sortedPoints.Add(quant, point);
                }


                //if there are multiple points with the same angle, keep the one in that is furthest away from the root point (but you shouldn't delete the root point)
                else if (_sortedPoints.ContainsKey(quant) && _sortedPoints[quant] != _rootPoint)
                {
                    //distance of point already in the dictionary
                    double d1 = distanceToRootPoint(_sortedPoints[quant]);
                    //distance of the new point to the root point
                    double d2 = distanceToRootPoint(point);

                    //if the new point is farther away than the point already in the dictionary
                    if (d2 > d1)
                    {
                        _sortedPoints.Remove(quant);
                        _sortedPoints.Add(quant, point);
                    }
                }
            }
        }

        //quantifies the polar angle based on the slope. For example, a quanitification of negative infinity would be an angle of 0, wherease a quantification of positive infinity would be an angle of 180 degrees
        private double quantifyAngle(Point point)
        {
            double quant = -(point.X - _rootPoint.X) / (_rootPoint.Y - point.Y);
            return quant;
        }

        //returns square of distance to the root point
        private double distanceToRootPoint(Point point)
        {
            return (point.X - _rootPoint.X) * (point.X - _rootPoint.X) + (point.Y - _rootPoint.Y) * (point.Y - _rootPoint.Y);
        }

        //figures out the points in the convex hull and stores it as a list
        private void figureOutConvexHull()
        {
            List<Point> sortedPoints = new List<Point>(_sortedPoints.Values);
            if (sortedPoints.Count < 5)
                return;
            _hullPoints = new Stack<Point>();
            for (int i = 0; i < 3; i++)
            {
                _hullPoints.Push(sortedPoints[i]);
            }

            //process remaining points
            for (int i = 3; i < sortedPoints.Count(); i++)
            {
                //keep removing the top while the angle formed by points next to top, top, and the point at index i makes a non-left turn
                while (orientation(nextToTop(), _hullPoints.Peek(), sortedPoints[i]) != 2)
                {
                    if (_hullPoints.Count() < 3) return;
                    _hullPoints.Pop();

                }
                _hullPoints.Push(sortedPoints[i]);
            }
        }

        //returns the point right below the top-most point in the hull stack
        private Point nextToTop()
        {
            Point top = _hullPoints.Pop();
            Point nextToTop = _hullPoints.Peek();
            _hullPoints.Push(top);
            return nextToTop;
        }

        //finds the orientation of the triplet (p,q,r)
        //returns 0 if collinear, 1 of clockwise, 2 if counterclockwise
        private int orientation(Point r, Point q, Point p)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) - (q.X - p.X) * (r.Y - q.Y);
            if (val == 0)
            {
                return 0;
            }
            else if (val > 0)
            {
                return 1; //clockwise
            }
            else
            {
                //val<0
                return 2; //counter clockwise
            }
        }

        private void addSelectionHull(Canvas mainCanvas)
        {
            _hull = new Polygon();
           
            //give the polyline the proper points
            while (_hullPoints.Count() > 0)
            {
                Point point = _hullPoints.Pop();
                //mainCanvas.TransformToVisual(null).TransformPoint(new Point(0,0));
                
                _hull.Points.Add(point);
            }
            _hull.Fill = new SolidColorBrush(Colors.Green);
            _hull.Opacity = .1;
            _hull.Stroke = new SolidColorBrush(Colors.Black);
            _hull.StrokeThickness = 1;

            for (int i= 0; i<_hull.Points.Count(); i++)
            {
                _hull.Points[i] = SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(_hull.Points[i]);
            }
            //mainCanvas.Children.Add(_hull);
        }

        private void selectContainedNodes(List<ISelectable> atoms)
        {
            foreach (ISelectable atom in atoms)
            {
                

                //addTestPoly(node.CornerPoints);
                foreach (Point refPoint in atom.ReferencePoints)
                {
                    if (this.isPointInHull(refPoint))
                    {
                        atom.Selected = true;
                        //Debug.WriteLine("A NODE HAS BEEN SELECTED");
                        break;
                    }
                }
            }
        }
        /*
        private void addTestPoly(PointCollection cornerPoints)
        {
            Polygon p = new Polygon();
            p.Points = cornerPoints;
            for (int i = 0; i<p.Points.Count(); i++)
            {
                p.Points[i] = SessionController.Instance.ActiveWorkspace.CompositeTransform.TransformPoint(p.Points[i]);
            }
            p.Stroke = new SolidColorBrush(Colors.Red);
            p.StrokeThickness = 5;

            _mainCanvas.Children.Add(p);
        }
        */

        private bool isPointInHull(Point testPoint)
        {
            //Debug.WriteLine(Canvas.GetLeft(_));
            bool result = false;
            PointCollection polygon = _hull.Points;


            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++) //loop thru all points in the convex hull
            {
                

                //if the test point is below the polygon point and above the  last polygon point OR if the testpoin is below the previous polygon point and above the current polygon pt.
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {

                       // if (i !=0) {
                            result = !result;
                           
                            //Debug.WriteLine("i: " + i);
                       // }
                    }
                }
                j = i;
            }
// Debug.WriteLine(zcount);
            return result;
        }
    }
}
