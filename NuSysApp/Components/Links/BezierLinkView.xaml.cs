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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : AnimatableUserControl
    {
        public BezierLinkView(LinkViewModel vm, bool isBiDirectional)
        {
            InitializeComponent();
            if (!isBiDirectional)
            {
                arrow.Visibility = Visibility.Visible;
            }
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;
           
          //  vm.Controller.LibraryElementModel.OnTitleChanged+= ControllerOnTitleChanged;
            vm.Controller.Disposed += OnDisposed;

             Title.SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                Rect.Width = args.NewSize.Width;
                Rect.Height = args.NewSize.Height;
            };

            //    Annotation.Text = vm.Annotation;

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
                Title.TextChanged += TitleOnTextChanged;
            };

        }

        private void LinkControllerOnAnnotationChanged(string text)
        {
            var vm = (LinkViewModel)DataContext;
             Title.Text = text;
            if (text != "")//TODO put visibility settings back in
            {
                 Title.Visibility = Visibility.Visible;
            }
            else
            {
                 Title.Visibility = Visibility.Collapsed;
            }
        }

        private void TitleOnTextChanged(object sender, TextChangedEventArgs e)
        {

            var vm = DataContext as LinkViewModel;
            Debug.Assert(vm != null);
            vm?.UpdateTitle(Title.Text);
        }

        private void OnDisposed(object source, object nothing = null)
        {
            var vm = (LinkViewModel)DataContext;
            vm.PropertyChanged -= OnPropertyChanged;
            vm.Controller.Disposed -= OnDisposed;
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
            DataContext = null;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            
            this.UpdateControlPoints();

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
            this.UpdateArrow();
            var vm = (LinkViewModel)this.DataContext;

            var controller = (LinkController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Anchor.X, controller.InElement.Anchor.Y);
            var anchor2 = new Point(controller.OutElement.Anchor.X, controller.OutElement.Anchor.Y);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            curveInner.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curveInner.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);

            Canvas.SetLeft(TitleContainer, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(TitleContainer, anchor1.Y - distanceY / 2 - Rect.ActualHeight * 1.5);

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
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Anchor.X, controller.InElement.Anchor.Y);
            var anchor2 = new Point(controller.OutElement.Anchor.X, controller.OutElement.Anchor.Y);

            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;

            pathfigureInner.StartPoint = anchor1;
            curveInner.Point3 = anchor2;
        }

        private void BezierLink_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = DataContext as LinkViewModel;
            Debug.Assert(vm != null);
            var linkController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(vm?.Controller.ContentId);
            Debug.Assert(linkController != null);
            SessionController.Instance.SessionView.ShowDetailView(linkController);
        }

        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            Debug.Assert(tb != null);
            tb.IsReadOnly = false;
        }

        private void Title_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                var tb = sender as TextBox;
                Debug.Assert(tb != null);
                tb.IsReadOnly = true;
            }
        }

    }
}