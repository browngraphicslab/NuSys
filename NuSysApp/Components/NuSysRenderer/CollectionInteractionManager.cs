using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NuSysApp.Components.NuSysRenderer;

namespace NuSysApp
{
    public class CollectionInteractionManager
    {
        public delegate void RenderItemSelectedHandler(BaseRenderItem element, PointerDeviceType device);
        public event RenderItemSelectedHandler ItemSelected;

        private enum Mode
        {
            PanZoom,
            MoveNode
        }

        private Mode _mode = Mode.PanZoom;
        private Dictionary<uint, Vector2> PointerPoints = new Dictionary<uint, Vector2>();

        private Dictionary<ElementViewModel, Transformable> _transformables =
            new Dictionary<ElementViewModel, Transformable>();

        private BaseRenderItem _selectedRenderItem;
        private Vector2 _centerPoint;
        private double _twoFingerDist;
        private double _distanceTraveled;
        private DateTime _lastReleased = DateTime.Now;
        private Stopwatch _stopwatch = new Stopwatch();
        private Stopwatch _firstPointerStopWatch = new Stopwatch();
        private CollectionRenderItem _collection;

        public CollectionInteractionManager(CollectionRenderItem collection)
        {
            _collection = collection;
            _collection.ResourceCreator.PointerPressed += OnPointerPressed;
            _collection.ResourceCreator.PointerReleased += OnPointerReleased;
        }

        private void UpdateCenterPoint()
        {
            var points = PointerPoints.Values.ToArray();
            var p0 = points[0];
            var p1 = points[1];
            _centerPoint = new Vector2((float) (p0.X + p1.X)/2f, (float) (p0.Y + p1.Y)/2f);
        }

        private void UpdateDist()
        {
            var points = PointerPoints.Values.ToArray();
            var p0 = new Vector2(points[0].X, points[0].Y);
            var p1 = new Vector2(points[1].X, points[1].Y);
            _twoFingerDist = MathUtil.Dist(p0, p1);
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (PointerPoints.Count == 0)
                _firstPointerStopWatch.Restart();

            if (PointerPoints.Count >= 1)
                _stopwatch.Start();

            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                PointerPoints.Clear();

            var cp = e.GetCurrentPoint(null).Position;
            var p = e.GetCurrentPoint(null).Position;
            PointerPoints.Add(e.Pointer.PointerId, new Vector2((float) p.X, (float) p.Y));
            if (PointerPoints.Count >= 2)
            {
                UpdateCenterPoint();
                UpdateDist();
                _mode = Mode.PanZoom;
                _transformables.Clear();
            }
            else
            {
                var pp = e.GetCurrentPoint(null).Position;
                _centerPoint = new Vector2((float) pp.X, (float) pp.Y);
                _selectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(cp);
                if (_selectedRenderItem == null)
                {
                    _mode = Mode.PanZoom;
                }
                else
                {
                    _mode = Mode.MoveNode;
                }
            }

            _collection.ResourceCreator.PointerMoved += OnPointerMoved;
        }

        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _stopwatch.Stop();

            if (PointerPoints.ContainsKey(e.Pointer.PointerId))
                PointerPoints.Remove(e.Pointer.PointerId);

            if (PointerPoints.Count == 0)
                _firstPointerStopWatch.Stop();

            if (PointerPoints.Count == 1)
            {
                _centerPoint = PointerPoints.Values.First();

                var dt = _stopwatch.ElapsedMilliseconds;
                if (dt < 200)
                {
                    var pp = e.GetCurrentPoint(null).Position;
                    var item = NuSysRenderer.Instance.GetRenderItemAt(pp);
                    ItemSelected?.Invoke(item, e.Pointer.PointerDeviceType);
                }
            }
            else if (PointerPoints.Count == 0)
            {
               
                if (_collection.ViewModel.Selections.Count == 0)
                    _transformables.Clear();

                var dt = (DateTime.Now - _lastReleased).TotalMilliseconds;
                if (dt < 200)
                {
                    _lastReleased = DateTime.Now;
                    _stopwatch.Reset();
                    return;
                }

                _collection.ResourceCreator.PointerMoved -= OnPointerMoved;
                if (_distanceTraveled < 5 && _stopwatch.ElapsedMilliseconds < 150 &&
                    _firstPointerStopWatch.ElapsedMilliseconds < 150)
                {
                    ItemSelected?.Invoke(_selectedRenderItem, e.Pointer.PointerDeviceType);
                }

                _distanceTraveled = 0;
            }
            _lastReleased = DateTime.Now;
            _stopwatch.Reset();
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!PointerPoints.ContainsKey(args.Pointer.PointerId))
                return;

            var p = args.GetCurrentPoint(null).Position;
            PointerPoints[args.Pointer.PointerId] = new Vector2((float) p.X, (float) p.Y);

            if (PointerPoints.Count >= 2)
            {
                if (args.Pointer.PointerId == PointerPoints.Keys.First())
                    return;
                var prevCenterPoint = _centerPoint;
                var prevDist = _twoFingerDist;
                UpdateCenterPoint();
                UpdateDist();
                var dx = _centerPoint.X - prevCenterPoint.X;
                var dy = _centerPoint.Y - prevCenterPoint.Y;
                var ds = (float) (_twoFingerDist/prevDist);


                if (_collection.ViewModel.Selections.Count > 0)
                {
                    foreach (var selection in _collection.ViewModel.Selections)
                    {
                        var elem = (ElementViewModel) selection;
                        var imgCenter = new Vector2((float) (elem.X + elem.Width/2), (float) (elem.Y + elem.Height/2));
                        var newCenter = NuSysRenderer.Instance.ActiveCollection.ObjectPointToScreenPoint(imgCenter);

                        Transformable t;
                        if (_transformables.ContainsKey(elem))
                            t = _transformables[elem];
                        else
                        {
                            t = new Transformable();
                            _transformables.Add(elem, t);
                            t.Position = new Point(elem.X, elem.Y);
                            t.Size = new Size(elem.Width, elem.Height);
                        }


                        PanZoom(t, newCenter, dx, dy, ds);

                        elem.Controller.SetSize(t.Size.Width*t.S.M11, t.Size.Height*t.S.M22);
                        var nx = t.Position.X - (t.Size.Width*t.S.M11 - t.Size.Width)/2;
                        var ny = t.Position.Y - (t.Size.Height*t.S.M22 - t.Size.Height)/2;
                        elem.Controller.SetPosition(nx, ny);
                    }
                }
                else
                {
                    PanZoom(_collection.Camera, _centerPoint, dx, dy, ds);
                }
            }
            else if (PointerPoints.Count == 1)
            {
                if (_mode == Mode.PanZoom)
                {
                    var currPos = args.GetCurrentPoint(null).Position;
                    var deltaX = (float) (currPos.X - _centerPoint.X);
                    var deltaY = (float) (currPos.Y - _centerPoint.Y);
                    _centerPoint = new Vector2((float) currPos.X, (float) currPos.Y);
                    PanZoom(_collection.Camera, _centerPoint, deltaX, deltaY, 1);
                }
                else
                {
                    var currPos = args.GetCurrentPoint(null).Position;
                    var deltaX = currPos.X - _centerPoint.X;
                    var deltaY = currPos.Y - _centerPoint.Y;
                    _distanceTraveled += Math.Abs(deltaX) + Math.Abs(deltaY);
                    _centerPoint = new Vector2((float) currPos.X, (float) currPos.Y);

                    var elem = _selectedRenderItem as ElementRenderItem;
                    if (elem == null)
                        return;

                    var newX = elem.ViewModel.X + deltaX/NuSysRenderer.Instance.ActiveCollection.Camera.S.M11;
                    var newY = elem.ViewModel.Y + deltaY/NuSysRenderer.Instance.ActiveCollection.Camera.S.M22;

                    if (!_collection.ViewModel.Selections.Contains(elem.ViewModel))
                    {
                        elem.ViewModel.Controller.SetPosition(newX, newY);
                    }
                    else
                    {
                        foreach (var selectable in _collection.ViewModel.Selections)
                        {
                            var e = (ElementViewModel) selectable;
                            var newXe = e.X + deltaX/NuSysRenderer.Instance.ActiveCollection.Camera.S.M11;
                            var newYe = e.Y + deltaY/NuSysRenderer.Instance.ActiveCollection.Camera.S.M11;
                            e.Controller.SetPosition(newXe, newYe);
                        }
                    }
                }
            }
        }

        protected void PanZoom(I2dTransformable target, Vector2 centerPoint, float dx, float dy, float ds)
        {
            var tmpTranslate = Matrix3x2.CreateTranslation(target.C.M31, target.C.M32);
            Matrix3x2 tmpTranslateInv;
            Matrix3x2.Invert(tmpTranslate, out tmpTranslateInv);

            Matrix3x2 cInv;
            Matrix3x2.Invert(target.C, out cInv);

            Matrix3x2 inverse;
            Matrix3x2.Invert(cInv*target.S*target.C*target.T, out inverse);

            var center = Vector2.Transform(new Vector2(centerPoint.X, centerPoint.Y), inverse);

            //var center = compositeTransform.Inverse.TransformPoint(centerPoint);

            var localPoint = Vector2.Transform(center, tmpTranslateInv);

            //Now scale the point in local space
            localPoint.X *= target.S.M11;
            localPoint.Y *= target.S.M22;

            //Transform local space into world space again
            var worldPoint = Vector2.Transform(localPoint, tmpTranslate);

            //Take the actual scaling...
            var distance = new Vector2(worldPoint.X - center.X, worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            

            var ntx = target.T.M31 + distance.X + dx;
            var nty = target.T.M32 + distance.Y + dy;

            var nsx = target.S.M11*ds;
            var nsy = target.S.M22*ds;

            var ncx = center.X;
            var ncy = center.Y;

            target.T = Matrix3x2.CreateTranslation((float) ntx, (float) nty);
            target.C = Matrix3x2.CreateTranslation(ncx, ncy);
            target.S = Matrix3x2.CreateScale((float) nsx, (float) nsy);
            target.Update();
        }
    }
}