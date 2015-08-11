using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class PanZoomMode : AbstractWorkspaceViewMode
    {

        public PanZoomMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.ManipulationMode = ManipulationModes.All;
            _view.ManipulationDelta += OnManipulationDelta;
            _view.ManipulationStarting += OnManipulationStarting;
            _view.PointerWheelChanged += OnPointerWheelChanged;
        }

        public override void Deactivate()
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

            var cent = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(_view).Position);

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
        }

        protected void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
           
            e.Container = _view;
            e.Handled = true;
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (((FrameworkElement)e.OriginalSource).DataContext != _view.DataContext) {
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
            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            vm.CompositeTransform = compositeTransform;

            e.Handled = true;
        }  
    }
}
