using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using MyToolkit.Mathematics;

namespace NuSysApp
{
    public class PanZoomMode : AbstractWorkspaceViewMode
    {
        //private DispatcherTimer _timer;
        private FreeFormViewer _cview;
        private CompositeTransform _tempTransform;
        private static PanZoomMode Instance;

        private List<UIElement> _allElements = new List<UIElement>();

        public PanZoomMode(FrameworkElement view) : base(view)
        {
            Instance = this;
            _cview = view as FreeFormViewer;
       //     _timer = new DispatcherTimer();
        //    _timer.Interval = TimeSpan.FromMilliseconds(5);
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            UpdateTempTransform(vm.CompositeTransform);
        }

        public static void UpdateTempTransform( CompositeTransform compositeTransform )
        {
            Instance._tempTransform = new CompositeTransform
            {
                TranslateX = compositeTransform.TranslateX,
                TranslateY = compositeTransform.TranslateY,
                ScaleX = compositeTransform.ScaleX,
                ScaleY = compositeTransform.ScaleY,
                CenterX = compositeTransform.CenterX,
                CenterY = compositeTransform.CenterY
            };
        }

        private void CompositionTargetOnRendering(object sender, object o)
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            var compositeTransform = vm.CompositeTransform;

            compositeTransform.TranslateX = _tempTransform.TranslateX;
            compositeTransform.TranslateY = _tempTransform.TranslateY;
            compositeTransform.ScaleX = _tempTransform.ScaleX;
            compositeTransform.ScaleY = _tempTransform.ScaleY;
            compositeTransform.CenterX = _tempTransform.CenterX;
            compositeTransform.CenterY = _tempTransform.CenterY;

            CullScreen();
        }


        private void CullScreen()
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            var sv = SessionController.Instance.SessionView.FreeFormViewer;
            var rect = vm.CompositeTransform.Inverse.TransformBounds(new Rect(-200, -200, sv.Width + 400, sv.Height + 400));
            var atoms = vm.AtomViewList.ToArray();
            foreach (var frameworkElement in atoms)
            {
                var dc = (ElementViewModel) frameworkElement.DataContext;
                if (dc == null)
                    continue;
    
                if (dc.ReferencePoints == null)
                {
                    dc.IsVisible = true;
                    return;
                }
                
                bool allIn = false;
                foreach (var referencePoint in dc.ReferencePoints)
                {
                    allIn = rect.Contains(referencePoint);
                    if (allIn)
                        break;
                }
                dc.IsVisible = allIn;
            }
        }


        public override async Task Activate()
        {
            _view.ManipulationMode = ManipulationModes.All;
            _view.ManipulationStarted += OnManipulationStarted;
            _view.PointerWheelChanged += OnPointerWheelChanged;
            _view.ManipulationDelta += OnManipulationDelta;
            _view.ManipulationCompleted += ViewOnManipulationCompleted;
            CompositionTarget.Rendering += CompositionTargetOnRendering;
        }



        public override async Task Deactivate()
        {
            _view.ManipulationMode = ManipulationModes.None;
            _view.ManipulationStarted -= OnManipulationStarted;
            _view.ManipulationDelta -= OnManipulationDelta;
            _view.ManipulationCompleted -= ViewOnManipulationCompleted;
            _view.PointerWheelChanged -= OnPointerWheelChanged;
            CompositionTarget.Rendering -= CompositionTargetOnRendering;
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
         //   _timer.Tick -= OnTick;
         //   _timer.Tick += OnTick;
         //   _timer.Start();
        }

        private void ViewOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
       //     _timer.Stop();
       //     _timer.Tick -= OnTick;
        //    _view.ManipulationCompleted -= ViewOnManipulationCompleted;
            e.Handled = true;
        }


        protected void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var vm = (FreeFormViewerViewModel)_view.DataContext;
            var compositeTransform = _tempTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var mousePoint = e.GetCurrentPoint(_view).Position;

            var cent = compositeTransform.Inverse.TransformPoint(mousePoint);

            var localPoint = tmpTranslate.Inverse.TransformPoint(cent);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - cent.X,
                worldPoint.Y - cent.Y);

            //...amd balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;
            var direction = Math.Sign((double)e.GetCurrentPoint(_view).Properties.MouseWheelDelta);

            var zoomspeed = direction < 0 ? 0.95 : 1.05;//0.08 * direction;
            var translateSpeed = 10;

            compositeTransform.ScaleX *= zoomspeed;
            compositeTransform.ScaleY *= zoomspeed;

            compositeTransform.CenterX = cent.X;
            compositeTransform.CenterY = cent.Y;
            if (_cview?.InqCanvas != null)
            {
                _cview.InqCanvas.Transform = compositeTransform;
                _cview.InqCanvas.Redraw();
            }
            e.Handled = true;
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            if (!(((FrameworkElement)e.OriginalSource).DataContext is FreeFormViewerViewModel))
                return;
            var vm = (FreeFormViewerViewModel)_view.DataContext;

            var compositeTransform = _tempTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(e.Position);

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
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift

            if (((FrameworkElement)e.OriginalSource).DataContext == _view.DataContext)
            {
                compositeTransform.TranslateX += e.Delta.Translation.X;
                compositeTransform.TranslateY += e.Delta.Translation.Y;
            }
            if (_cview?.InqCanvas != null)
            {
                _cview.InqCanvas.Transform = compositeTransform;
                _cview.InqCanvas.Redraw();
            }


            e.Handled = true;

        }
    }
}