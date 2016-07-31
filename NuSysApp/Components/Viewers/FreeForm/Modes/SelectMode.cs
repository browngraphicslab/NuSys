using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.Utilities;
using System.Numerics;
using Windows.Devices.Input;

namespace NuSysApp
{
    public class SelectMode : AbstractWorkspaceViewMode
    {
        private enum Mode { PanZoom, MoveNode}

        private Mode _mode = Mode.PanZoom;
        private Dictionary<uint, Point> _pointerPoints = new Dictionary<uint, Point>(); 
        private BaseRenderItem _selectedRenderItem;
        private Point _startPoint;
        private Point _centerPoint;
        private double _twoFingerDist;
        private double _distanceTraveled;

        public SelectMode(FreeFormViewer view):base(view)
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            var compositeTransform = vm.CompositeTransform;
            NuSysRenderer.T = Matrix3x2.CreateTranslation((float)compositeTransform.TranslateX, (float)compositeTransform.TranslateY);
            NuSysRenderer.C = Matrix3x2.CreateTranslation((float)compositeTransform.CenterX, (float)compositeTransform.CenterY);
            NuSysRenderer.S = Matrix3x2.CreateScale((float)compositeTransform.ScaleX, (float)compositeTransform.ScaleY);
        }

        public SelectMode(AreaNodeView view) : base(view)
        {
        }
        public override async Task Activate()
        {
            var ffview = (FreeFormViewer) _view;
            ffview.RenderCanvas.PointerPressed += OnPointerPressed;
            ffview.RenderCanvas.PointerReleased += OnPointerReleased;
        }



        private void UpdateCenterPoint()
        {
            var points = _pointerPoints.Values.ToArray();
            var p0 = points[0];
            var p1 = points[1];
            _centerPoint = new Point((p0.X + p1.X) / 2.0, (p0.Y + p1.Y) / 2.0);
        }

        private void UpdateDist()
        {
            var points = _pointerPoints.Values.ToArray();
            var p0 = points[0];
            var p1 = points[1];
            _twoFingerDist = MathUtil.Dist(p0, p1);
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                _pointerPoints.Clear();

            var ffView = (FreeFormViewer)_view;
            var cp = e.GetCurrentPoint(null).Position;
            _pointerPoints.Add(e.Pointer.PointerId, e.GetCurrentPoint(null).Position);
            if (_pointerPoints.Count >= 2)
            {
                UpdateCenterPoint();
                UpdateDist();
                _mode = Mode.PanZoom;
            }
            else
            {
                _startPoint = e.GetCurrentPoint(null).Position;
                _selectedRenderItem = ffView.NuSysRenderer.GetRenderItemAt(cp);
                if (_selectedRenderItem == null)
                {
                    _mode = Mode.PanZoom;
                }
                else
                {
                    _mode = Mode.MoveNode;
                }
            }
            ffView.RenderCanvas.PointerMoved += OnPointerMoved;
            /*
            var ffView = (FreeFormViewer)_view;
            _startPoint = e.GetCurrentPoint(ffView).Position;
            _selectedRenderItem = ffView.NuSysRenderer.GetRenderItemAt(_startPoint);

            if (_selectedRenderItem == null)
                return;
            var elem = _selectedRenderItem as ElementRenderItem;
            if (elem == null)
                return;

            var os = ffView.NuSysRenderer.ScreenPointToObjectPoint(new Vector2((float)_startPoint.X, (float)_startPoint.Y));
            if (elem.HitTestTitle(os))
            {
                Debug.WriteLine("Title hit");
                ffView.RenderCanvas.PointerMoved += OnPointerMoved;
            }
            else
            {
                Debug.WriteLine("Activate Element");
            }*/
        }

        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_pointerPoints.ContainsKey(e.Pointer.PointerId))
                _pointerPoints.Remove(e.Pointer.PointerId);

            if (_pointerPoints.Count == 1)
                _startPoint = _pointerPoints.Values.First();

            var ffView = (FreeFormViewer)_view;
            if (_pointerPoints.Count == 0) { 
                ffView.RenderCanvas.PointerMoved -= OnPointerMoved;

                if (_distanceTraveled < 5)
                {
                    Debug.WriteLine("activate!");
                }

                _distanceTraveled = 0;
            }
        }

        private void OnPointerMoved(object sender, PointerRoutedEventArgs args)
        {
            if (!_pointerPoints.ContainsKey(args.Pointer.PointerId))
                return;

            _pointerPoints[args.Pointer.PointerId] = args.GetCurrentPoint(null).Position;

            if (_pointerPoints.Count >= 2)
            {
                var prevCenterPoint = _centerPoint;
                var prevDist = _twoFingerDist;
                UpdateCenterPoint();
                UpdateDist();
                var dx = _centerPoint.X - prevCenterPoint.X;
                var dy = _centerPoint.Y - prevCenterPoint.Y;
                var ds = _twoFingerDist/prevDist;
                PanZoom(_centerPoint, dx,dy, ds);
                
            } else if (_pointerPoints.Count == 1)
            {
                if (_mode == Mode.PanZoom)
                {
                    var currPos = args.GetCurrentPoint(null).Position;
                    var deltaX = currPos.X - _startPoint.X;
                    var deltaY = currPos.Y - _startPoint.Y;
                    _startPoint = currPos;
                    PanZoom(_startPoint, deltaX, deltaY, 1);
                }
                else
                {
                    var currPos = args.GetCurrentPoint(null).Position;
                    var deltaX = currPos.X - _startPoint.X;
                    var deltaY = currPos.Y - _startPoint.Y;
                    _distanceTraveled += Math.Abs(deltaX) + Math.Abs(deltaY);
                    _startPoint = currPos;

                  //  PanZoom(_startPoint, deltaX, deltaY, 1);
                    var elem = _selectedRenderItem as ElementRenderItem;
                    if (elem == null)
                        return;

                    var vm = (FreeFormViewerViewModel)_view.DataContext;
                    var compositeTransform = vm.CompositeTransform;

                    var newX = elem.ViewModel.X + deltaX / compositeTransform.ScaleX;
                    var newY = elem.ViewModel.Y + deltaY / compositeTransform.ScaleX;
                    elem.ViewModel.Controller.SetPosition(newX, newY);
                }
            }
        }

        protected void PanZoom(Point centerPoint, double dx, double dy, double ds)
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            var compositeTransform = vm.CompositeTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(centerPoint);

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= ds;
            compositeTransform.ScaleY *= ds;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift
            compositeTransform.TranslateX += dx;
            compositeTransform.TranslateY += dy;

            NuSysRenderer.T = Matrix3x2.CreateTranslation((float)compositeTransform.TranslateX, (float)compositeTransform.TranslateY);
            NuSysRenderer.C = Matrix3x2.CreateTranslation((float)compositeTransform.CenterX, (float)compositeTransform.CenterY);
            NuSysRenderer.S = Matrix3x2.CreateScale((float)compositeTransform.ScaleX, (float)compositeTransform.ScaleY);
        }

        

        public override async Task Deactivate()
        {
        }

















        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var ffView = (FreeFormViewer)_view;
            var elem = ffView.NuSysRenderer.GetRenderItemAt(e.GetPosition(null));
            if (elem == null)
                return;

            Debug.WriteLine("Showing detail view");

            /*
            var dc = (e.OriginalSource as FrameworkElement)?.DataContext;
            if ((dc is ElementViewModel || dc is LinkViewModel) && !(dc is FreeFormViewerViewModel) )
            {
                if (dc is ElementViewModel)
                {
                    var vm = dc as ElementViewModel;              

                    if (vm.ElementType == ElementType.Powerpoint)
                    {
                        SessionController.Instance.SessionView.OpenFile(vm);
                    }
                    else if (vm.ElementType != ElementType.Link)
                    {

                        if (vm.ElementType == ElementType.PDF)
                        {
                            var pdfVm = (PdfNodeViewModel)vm;
                            PdfDetailHomeTabViewModel.InitialPageNumber = pdfVm.CurrentPageNumber;
                        }

                        SessionController.Instance.SessionView.ShowDetailView(vm.Controller.LibraryElementController);
                    }

                }
            }   
            */
        }
    }
}
