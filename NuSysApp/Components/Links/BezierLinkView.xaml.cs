using System;
using System.ComponentModel;
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
    public sealed partial class BezierLinkView : AnimatableUserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;

            var model = vm.Model;
            this.Annotation.IsActivated = false;
            vm.Controller.TitleChanged += delegate//TODO remove this handler eventually
            {
                Annotation.Text = model.Title;
                //Annotation.Visibility
                AnnotationContainer.Visibility = model.Title == ""
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            };

            Annotation.SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                Rect.Width = args.NewSize.Width;
                Rect.Height = args.NewSize.Height;
            };
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
                AnnotationContainer.Visibility = vm.AnnotationText == "" ? Visibility.Collapsed : Visibility.Visible;
                //       await SessionController.Instance.InitializeRecog();
            };
        }
        private void UpdateText()
        {
            var model = (DataContext as LinkViewModel).Model;
            var vm = DataContext as LinkViewModel;
            vm.AnnotationText = Annotation.Text;
            if (model.Title != Annotation.Text)
            {
                model.Title = Annotation.Text;
                var m = new Message();
                m["id"] = model.Id;
                m["title"] = model.Title;
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SendableUpdateRequest(m), NetworkClient.PacketType.UDP);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.UpdateControlPoints();

            if (propertyChangedEventArgs.PropertyName == "AnnotationText")
            {
                AnnotationContainer.Visibility = Annotation.Text == ""
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            var vm = (LinkViewModel)DataContext;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                if (vm.IsSelected)
                {
                    this.Annotation.Activate();
                    AnnotationContainer.Visibility = Visibility.Visible;
                    Delete.Visibility = Visibility.Visible;
                }
                else
                {
                    if (Annotation.Text == "")
                    {
                        AnnotationContainer.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        UpdateText();
                        Delete.Visibility = Visibility.Collapsed;
                    }
                    this.Annotation.DeActivate();
                }
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
            this.UpdateEndPoints();


            var vm = (LinkViewModel)this.DataContext;

            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);

            Canvas.SetLeft(btnDelete, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(btnDelete, anchor1.Y - distanceY / 2);

            Canvas.SetLeft(AnnotationContainer, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(AnnotationContainer, anchor1.Y - distanceY / 2 - Rect.ActualHeight * 1.5);

        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;
        }

        private async void OnRecordClick(object sender, RoutedEventArgs e)
        {
            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                await session.TranscribeVoice();

                var vm = (LinkViewModel)DataContext;
                //((TextNodeModel)vm.Model).Text = session.SpeechString;
                vm.AnnotationText = session.SpeechString;
            }
            else
            {
                var vm = this.DataContext as LinkViewModel;
            }
        }

        private async void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            await controller.RequestDelete();
        }
    }
}