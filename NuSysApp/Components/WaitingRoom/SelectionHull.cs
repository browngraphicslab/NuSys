using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class SelectionHull : IDisposable
    {
        private List<Point> _points;
        private int _min;
        private Point _rootPoint;
        private SortedDictionary<double, Point> _sortedPoints;
        private Stack<Point> _hullPoints;
        private Polygon _hull;
        private Polygon _visualHull;
        private Canvas _mainCanvas;

        /// <summary>
        /// Makes a selection hull by checking for bad input, doing housekeeping tasks, and 
        /// finally creating the hull & selecting what's inside
        /// 
        /// NOTE!!!!!!!!!!!!
        /// If you want to visually confirm that addSelectionHull lasso works, remove the comments on the
        /// last 2 lines of the addSelectionHull() method
        /// 
        /// UPDATE: Whoever was the last person to work on this, please chime and and tell everyone
        ///  what this class is being used for now! Not sure why the constructor was removed and what
        ///  the compute method is doing. Thanks. -Z
        /// </summary>
        public SelectionHull()
        {
        }

        /// <summary>
        /// This will always return 0. Not sure what exactly is going on here.
        /// </summary>
        /// <param name="lasso"></param>
        /// <param name="mainCanvas"></param>
        /// <returns></returns>
        public int Compute(Polyline lasso, Canvas mainCanvas)
        {
            // handles bad point input
            if (lasso.Points.Count < 5)
                return 0;

            ExecuteHousekeepingTasks(lasso, mainCanvas);
            return ExecuteHullTasks();
        }

        /// <summary>
        /// "Housekeeping tasks" - i.e. defining instance variables, deselecting all other nodes on the canvas, clearing artifact 
        /// UI elements from the workspace
        /// </summary>
        private void ExecuteHousekeepingTasks(Polyline lasso, Canvas mainCanvas)
        {
            _mainCanvas = mainCanvas;
            SessionController.Instance.ActiveFreeFormViewer.DeselectAll();
            _points = new List<Point>(lasso.Points);
            CleanMainCanvas();
        }

        /// <summary>
        /// Executes hull tasks, i.e. sorting points and figuring out the hull. 
        /// </summary>
        private int ExecuteHullTasks()
        {
            FindBottomLeftMostPoint();
            PlaceBottomLeftMostPointAtFirstPosition();
            SortPoints();
            FigureOutConvexHull();

            // only make a hull if we have some points
            if (_hullPoints != null)
            {
                AddSelectionHull();
                return SelectContainedNodes(SessionController.Instance.ActiveFreeFormViewer.AllContent);
            }

            return 0;
        }

        // Cleans artifact ui elements from the main canvas (i.e. the lasso polyline, any extraneous leftover shapes, etc.)
        private void CleanMainCanvas()
        {
            List<UIElement> toRemove = new List<UIElement>();

            // flags polygons and polylines as artifacts to remove
            foreach (var content in _mainCanvas.Children)
            {
                if (content.GetType().ToString().Equals("Windows.UI.Xaml.Shapes.Polygon"))
                {
                    toRemove.Add(content);
                }
                if (content.GetType().ToString().Equals("Windows.UI.Xaml.Shapes.Polyline"))
                {
                    toRemove.Add(content);
                }
            }

            // removes the artifacts
            foreach (var content in toRemove)
            {
                _mainCanvas.Children.Remove(content);
            }
        }

        // finds the bottom-left most point by looping thru all of the points
        private void FindBottomLeftMostPoint()
        {
            double yMin = _points[0].Y;
            _min = 0;

            // loop thru all points
            for (int i = 1; i < _points.Count(); i++)
            {
                double y = _points[i].Y;
                // pick the bottom-most or choose the left mostt point in case of a tie
                if ((y > yMin) || (y == yMin && _points[i].X < _points[_min].X))
                {
                    yMin = _points[i].Y;
                    _min = i;
                }
            }
        }

        // swaps the bottom left point to be the first position
        private void PlaceBottomLeftMostPointAtFirstPosition()
        {
            this.Swap(_points[0], _points[_min]);
            _rootPoint = _points[0];
        }

        // swaps two points in the points list
        private void Swap(Point one, Point two)
        {
            int indexOne = _points.IndexOf(one);
            int indexTwo = _points.IndexOf(two);

            var temp = one;
            _points[indexOne] = _points[indexTwo];
            _points[indexTwo] = temp;

        }

        // sorts all of the points by polar angle in counterclockwise order around the bottom-left-most point
        private void SortPoints()
        {
            _sortedPoints = new SortedDictionary<double, Point>();
            for (int i = 0; i < _points.Count(); i++)
            {
                var point = _points[i];
                double quant = QuantifyAngle(point);

                // If the list doesn't have a pointat that angle yet, then add it
                if (!_sortedPoints.ContainsKey(quant))
                {
                    _sortedPoints.Add(quant, point);
                }


                // if there are multiple points with the same angle, keep the one in that is furthest away from the root point (but you shouldn't delete the root point)
                else if (_sortedPoints.ContainsKey(quant) && _sortedPoints[quant] != _rootPoint)
                {
                    // distance of point already in the dictionary
                    double d1 = DistanceToRootPoint(_sortedPoints[quant]);
                    // distance of the new point to the root point
                    double d2 = DistanceToRootPoint(point);

                    // if the new point is farther away than the point already in the dictionary
                    if (d2 > d1)
                    {
                        _sortedPoints.Remove(quant);
                        _sortedPoints.Add(quant, point);
                    }
                }
            }
        }

        // quantifies the polar angle based on the slope. For example, a quanitification of negative infinity would be an angle of 0, wherease a quantification of positive infinity would be an angle of 180 degrees
        private double QuantifyAngle(Point point)
        {
            double quant = -(point.X - _rootPoint.X) / (_rootPoint.Y - point.Y);
            return quant;
        }

        // returns square of distance to the root point
        private double DistanceToRootPoint(Point point)
        {
            return (point.X - _rootPoint.X) * (point.X - _rootPoint.X) + (point.Y - _rootPoint.Y) * (point.Y - _rootPoint.Y);
        }

        // figures out the points in the convex hull and stores it as a list
        private void FigureOutConvexHull()
        {
            List<Point> sortedPoints = new List<Point>(_sortedPoints.Values);
            if (sortedPoints.Count < 5)
                return;
            _hullPoints = new Stack<Point>();
            for (int i = 0; i < 3; i++)
            {
                _hullPoints.Push(sortedPoints[i]);
            }

            // process remaining points
            for (int i = 3; i < sortedPoints.Count(); i++)
            {
                // keep removing the top while the angle formed by points next to top, top, and the point at index i makes a non-left turn
                while (Orientation(NextToTop(), _hullPoints.Peek(), sortedPoints[i]) != 2)
                {
                    if (_hullPoints.Count() < 3) return;
                    _hullPoints.Pop();

                }
                _hullPoints.Push(sortedPoints[i]);
            }
        }

        // returns the point right below the top-most point in the hull stack
        private Point NextToTop()
        {
            var top = _hullPoints.Pop();
            var nextToTop = _hullPoints.Peek();
            _hullPoints.Push(top);
            return nextToTop;
        }

        // finds the orientation of the triplet (p,q,r)
        // returns 0 if collinear, 1 of clockwise, 2 if counterclockwise
        private int Orientation(Point r, Point q, Point p)
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
                return 2; //counter clockwise
            }
        }

        /// <summary>
        /// Note: The _hull is represents the selection hull in global space (absolute coordinates, i.e. x=50,000, y = 50,000).
        /// The _visualHull represents the selection hull in local space, and you can add the visual hull to the main canvas to actually
        /// see the hull you have drawn.
        /// </summary>
        private void AddSelectionHull()
        {
            _hull = new Polygon();
            _visualHull = new Polygon();

            // give both hulls the proper points
            while (_hullPoints.Count() > 0)
            {
                var point = _hullPoints.Pop();
                _visualHull.Points.Add(point);
                _hull.Points.Add(point);
            }

            // format visual hull
            _visualHull.Fill = new SolidColorBrush(Colors.Green);
            _visualHull.Opacity = .1;
            _visualHull.Stroke = new SolidColorBrush(Colors.Black);
            _visualHull.StrokeThickness = 1;

            // transform hull points to global space
            for (int i = 0; i < _hull.Points.Count(); i++)
            {
              //  _hull.Points[i] = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(_hull.Points[i]);
            }

            // REMOVE COMMENTS FROM LINES BELOW TO SEE THE VISUAL HULL
          //   _mainCanvas.Children.Add(_visualHull);
          //  Canvas.SetZIndex(_visualHull, -1);
        }

        // selects contained atoms by figuring out the atoms in the selection hull
        private int SelectContainedNodes(List<ElementViewModel> atoms)
        {
            var count = 0;
            foreach (var atom in atoms)
            {
                foreach (var refPoint in atom.ReferencePoints)
                {
                    if (this.IsPointInHull(refPoint))
                    {
                        SessionController.Instance.ActiveFreeFormViewer.AddSelection(atom);
                        count++;
                        break;
                    }
                }
            }

            return count;
        }

        private bool IsPointInHull(Point testPoint)
        {
            bool result = false;
            var polygon = _hull.Points;

            int j = polygon.Count() - 1;
            for (int i = 0; i < polygon.Count(); i++) //loop thru all points in the convex hull
            {

                //if the test point is below the polygon point and above the  last polygon point OR if the testpoin is below the previous polygon point and above the current polygon pt.
                if (polygon[i].Y < testPoint.Y && polygon[j].Y >= testPoint.Y || polygon[j].Y < testPoint.Y && polygon[i].Y >= testPoint.Y)
                {
                    if (polygon[i].X + (testPoint.Y - polygon[i].Y) / (polygon[j].Y - polygon[i].Y) * (polygon[j].X - polygon[i].X) < testPoint.X)
                    {
                        result = !result;
                    }
                }
                j = i;
            }
            return result;
        }

        /// <summary>
        /// Removes all previously attached event listeners and object references, frees previously allocated memory
        /// </summary>
        public void Dispose()
        {
            _points = null;
            _sortedPoints = null;
            _hullPoints = null;
            _hull = null;
            _visualHull = null;
            _mainCanvas = null;

        }
    }
}