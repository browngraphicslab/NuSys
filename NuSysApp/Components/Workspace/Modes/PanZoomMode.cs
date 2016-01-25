using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class PanZoomMode : AbstractWorkspaceViewMode
    {

        private bool _isPinAnimating;

        public PanZoomMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            _view.ManipulationMode = ManipulationModes.All;
            _view.ManipulationDelta += OnManipulationDelta;
            _view.ManipulationStarting += OnManipulationStarting;
            _view.PointerWheelChanged += OnPointerWheelChanged;
        }

        public override async Task Deactivate()
        {
            _view.ManipulationMode = ManipulationModes.None;
            _view.ManipulationDelta -= OnManipulationDelta;
            _view.ManipulationStarting -= OnManipulationStarting;
            _view.PointerWheelChanged -= OnPointerWheelChanged;
        }

        protected void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            var compositeTransform = vm.CompositeTransform;

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

            var center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(_view).Position);
            compositeTransform.ScaleX *= zoomspeed;
            compositeTransform.ScaleY *= zoomspeed;

            compositeTransform.CenterX = cent.X;
            compositeTransform.CenterY = cent.Y;
            vm.CompositeTransform = compositeTransform;

            var model = (WorkspaceModel)vm.Model;
            model.LocationX = compositeTransform.TranslateX;
            model.LocationY = compositeTransform.TranslateY;
            model.CenterX = compositeTransform.CenterX;
            model.CenterY = compositeTransform.CenterY;
            model.Zoom = compositeTransform.ScaleX;

            _view.InqCanvas.Transform = compositeTransform;

        }

        protected void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
           
            e.Container = _view;
          //  e.Handled = true;
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.IsInertial)
            {
                // TODO: Re-add
                /*
                if (_view.PinAnimationStoryboard.GetCurrentState() == ClockState.Active)
                {
                    _isPinAnimating = true;
                    return;
                }
                */
            }
            else
            {
                _isPinAnimating = false;
            }

            if ( _isPinAnimating) {
                return;
            }

            var vm = (WorkspaceViewModel)_view.DataContext;

            var compositeTransform = vm.CompositeTransform;
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

            if (((FrameworkElement)e.OriginalSource).DataContext == _view.DataContext) { 
                compositeTransform.TranslateX += e.Delta.Translation.X;
                compositeTransform.TranslateY += e.Delta.Translation.Y;
            }

            vm.CompositeTransform = compositeTransform;

            var model = (WorkspaceModel)vm.Model;
            model.LocationX = compositeTransform.TranslateX;
            model.LocationY = compositeTransform.TranslateY;
            model.CenterX = compositeTransform.CenterX;
            model.CenterY = compositeTransform.CenterY;
            model.Zoom = compositeTransform.ScaleX;

            _view.InqCanvas.Transform = compositeTransform;
        }
    }
}
