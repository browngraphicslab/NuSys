using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using NuSysApp.Controller;
using NuSysApp.Util;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : AnimatableUserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;
            vm.Controller.Disposed += OnDisposed;
 
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };
        }      

        private void OnDisposed(object source)
        {
            var vm = (ElementViewModel)DataContext;
            vm.PropertyChanged -= OnPropertyChanged;
            vm.Controller.Disposed -= OnDisposed;
            DataContext = null;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            
            this.UpdateControlPoints();

            Canvas.SetZIndex(this, -10);

            var vm = DataContext as LinkViewModel;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                if (vm?.IsSelected ?? false)
                {
                }
            }
            else
            {
                (vm?.Model as LinkModel)?.RectangleModel?.Model.Deselect();
            }
        }
        

        private void OnAtomPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }

        /// <summary>
        /// Updates the location of the bezier controlpoints. 
        /// Do not call this method outside of this class.
        /// </summary>
        private void UpdateControlPoints()
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            this.UpdateEndPoints();

            var vm = (LinkViewModel)this.DataContext;

            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            curveInner.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curveInner.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);

        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;

            pathfigureInner.StartPoint = anchor1;
            curveInner.Point3 = anchor2;
        }

        private void BezierLink_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION)
            {
                // Handles exploration mode
                var vm = DataContext as LinkViewModel;
                Debug.Assert(vm != null);
                Canvas.SetZIndex(this, -10);
                SessionController.Instance.SessionView.Explore(vm);
            }
        }
    }
}