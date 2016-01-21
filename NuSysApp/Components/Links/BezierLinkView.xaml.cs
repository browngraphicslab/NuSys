using System;
using System.ComponentModel;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : UserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;

            vm.Atom1.PropertyChanged += new PropertyChangedEventHandler(OnAtomPropertyChanged);
            vm.Atom2.PropertyChanged += new PropertyChangedEventHandler(OnAtomPropertyChanged);

            var model = vm.Model;
            model.TitleChanged += delegate//TODO remove this handler eventually
            {
                Annotation.Text = model.Title;
                //Annotation.Visibility
                AnnotationContainer.Visibility = model.Title == ""
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }; 

            Annotation.SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                Rect.Width = args.NewSize.Width;
                Rect.Height = args.NewSize.Height;
            };
            Annotation.TextChanged += OnAnnotationTextChanged;
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
                AnnotationContainer.Visibility = vm.AnnotationText == "" ? Visibility.Collapsed : Visibility.Visible;
         //       await SessionController.Instance.InitializeRecog();
            };
        }
        private void OnAnnotationTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            var model = (DataContext as LinkViewModel).Model;
            if (model.Title != Annotation.Text)
            {
                model.Title = Annotation.Text;
                var m = new Message();
                m["id"] = model.Id;
                m["title"] = model.Title;
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SendableUpdateRequest(m),NetworkClient.PacketType.UDP);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "AnnotationText")
            {
                AnnotationContainer.Visibility = (sender as LinkViewModel).AnnotationText == ""
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            }

            var vm = (LinkViewModel)DataContext;

            if (propertyChangedEventArgs.PropertyName == "IsSelected" && vm.AnnotationText == "")
            {
                AnnotationContainer.Visibility = vm.IsSelected ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                Record.Visibility = vm.IsSelected ? Visibility.Visible : Visibility.Collapsed;
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

            var vm = (LinkViewModel) this.DataContext;
            var atom1 = vm.Atom1;
            var atom2 = vm.Atom2;
            var anchor1 = atom1.Anchor;
            var anchor2 = atom2.Anchor;
            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX/2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX/2, anchor1.Y);

            Canvas.SetLeft(btnDelete, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(btnDelete, anchor1.Y - distanceY / 2);

            Canvas.SetLeft(AnnotationContainer, anchor1.X - distanceX/2 - Rect.ActualWidth/2);
            Canvas.SetTop(AnnotationContainer, anchor1.Y - distanceY/2 - Rect.ActualHeight*1.5);
        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel) this.DataContext;
            var atom1 = vm.Atom1;
            var atom2 = vm.Atom2;
            pathfigure.StartPoint = atom1.Anchor;
            curve.Point3 = atom2.Anchor;
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
    }
}