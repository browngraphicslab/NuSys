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
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp;
using NuSysApp.Controller;
using NuSysApp.Util;
using WinRTXamlToolkit.Controls.DataVisualization;

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
            vm.Controller.Disposed += OnDisposed;

            var linkLibElemCont = vm.Controller.LibraryElementController as LinkLibraryElementController;
            Debug.Assert(linkLibElemCont != null);

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
        /*
        /// <summary>
        /// Handler for LinkLibraryElementController's LinkDirectionChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnLinkDirectionChanged(object sender, LinkDirectionEnum e)
        {
            if (e.Equals(LinkDirectionEnum.Mono1))
            {
                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/mono2.png"));
                arrow.Visibility = Visibility.Visible;
            }
            else if (e.Equals(LinkDirectionEnum.Mono2))
            {
                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/bi1.png"));
                arrow.Visibility = Visibility.Visible;
            }
            else
            {
                image.Source = new BitmapImage(new Uri("ms-appx:///Assets/mono1.png"));
                arrow.Visibility = Visibility.Collapsed;
            }
        }
        */
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
            var linkLibElemCont = vm.Controller.LibraryElementController as LinkLibraryElementController;
            Debug.Assert(linkLibElemCont != null);
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
            Canvas.SetLeft(DeleteButton, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(DeleteButton, anchor1.Y - distanceY / 2 - Rect.ActualHeight * 1.5);

        }

        private void UpdateArrow()
        {
            var center = new Point((pathfigure.StartPoint.X + curve.Point3.X) / 2.0, (pathfigure.StartPoint.Y + curve.Point3.Y) / 2.0);
            var xDiff = curve.Point3.X - pathfigure.StartPoint.X;
            var yDiff = curve.Point3.Y - pathfigure.StartPoint.Y;
            var angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            var tranformGroup = new TransformGroup();
            if (double.IsNaN(curve.Point1.X) || double.IsNaN(curve.Point1.Y) ||
                double.IsNaN(curve.Point2.X) || double.IsNaN(curve.Point2.Y) ||
                double.IsNaN(curve.Point3.X) || double.IsNaN(curve.Point3.Y))
            {
                Debug.WriteLine("One of the points in the bezier link had a NaN value");
                return;
            }
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
            SessionController.Instance.NuSessionView.ShowDetailView(linkController);
        }

        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            Debug.Assert(tb != null);
            tb.IsReadOnly = false;
        }

        private void Title_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            Debug.Assert(tb != null);
            tb.IsReadOnly = true;
        }
        private void LinkDirectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LinkViewModel;
            vm?.DirectionButtonClicked();
        }

        /// <summary>
        /// Deletes the link
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as LinkViewModel;

            // delete the link using the SessionController
            var success = await SessionController.Instance.LinksController.RemoveLink(vm?.LinkModel.LibraryId);
        }
    }
}