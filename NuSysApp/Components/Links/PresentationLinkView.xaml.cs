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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PresentationLinkView : AnimatableUserControl
    {
        public PresentationLinkView(PresentationLinkViewModel vm)
        {
            InitializeComponent();
            Debug.Assert(vm != null);
            DataContext = vm;
            vm.ControlPointsChanged += ControlPointsChanged;
            vm.Disposed += OnDisposed;
            
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate
            {
                UpdateControlPoints();
            };
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Add(this);
        }

        private void ControlPointsChanged(object sender, EventArgs e)
        {
            UpdateControlPoints();
        }

        private void OnDisposed(object source, object args)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
            DataContext = null;
        }

        /// <summary>
        /// Updates the location of the bezier controlpoints. 
        /// Do not call this method outside of this class.
        /// </summary>
        private void UpdateControlPoints()
        {
            var vm = DataContext as PresentationLinkViewModel;
            Debug.Assert(vm != null);
            Debug.Assert(vm.InAnchor != null);
            Debug.Assert(vm.OutAnchor != null);

            this.UpdateEndPoints();
            this.UpdateArrow();

            var anchor1 = new Point(vm.InAnchor.X, vm.InAnchor.Y);
            var anchor2 = new Point(vm.OutAnchor.X, vm.OutAnchor.Y);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);

            //Update position of delete button. Eventually, will have annotations?
            Canvas.SetLeft(Delete, anchor1.X - distanceX / 2);
            Canvas.SetTop(Delete, anchor1.Y - distanceY / 2);
        }



        private void UpdateArrow()
        {
            var center = new Point((pathfigure.StartPoint.X + curve.Point3.X) / 2.0, (pathfigure.StartPoint.Y + curve.Point3.Y) / 2.0);
            var xDiff = curve.Point3.X - pathfigure.StartPoint.X;
            var yDiff = curve.Point3.Y - pathfigure.StartPoint.Y;
            var angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            var tranformGroup = new TransformGroup();
            tranformGroup.Children.Add(new RotateTransform { Angle = angle, CenterX = 20, CenterY = 20 });
            tranformGroup.Children.Add(new TranslateTransform { X = center.X - 20, Y = center.Y - 20 });

            arrow.RenderTransform = tranformGroup;
        }

        private void UpdateEndPoints()
        {
            var vm = DataContext as PresentationLinkViewModel;
            Debug.Assert(vm != null);
            Debug.Assert(vm.InAnchor != null);
            Debug.Assert(vm.OutAnchor != null);
            var anchor1 = new Point(vm.InAnchor.X, vm.InAnchor.Y);
            var anchor2 = new Point(vm.OutAnchor.X, vm.OutAnchor.Y);
            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PresentationLinkViewModel;
            vm.DeletePresentationLink();
        }
    }
}