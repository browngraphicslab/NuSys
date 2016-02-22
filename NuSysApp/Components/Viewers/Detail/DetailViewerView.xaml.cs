using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class DetailViewerView : AnimatableUserControl
    {
        public DetailViewerView()
        {
            InitializeComponent();

            Opacity = 0;
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
           
            };

          DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
          {
              if (!(DataContext is DetailViewerViewModel))
                  return;
              
              var vm = (DetailViewerViewModel)DataContext;
              vm.PropertyChanged += OnPropertyChanged;
              Tags.ItemsSource = vm.Tags;
              vm.MakeTagList();
          };
            IsHitTestVisible = true;
            //PointerReleased += OnPointerReleased;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "View")
            {
                Anim.To(this, "Alpha", 1, 400, null, (a, i) => { IsHitTestVisible = true; });
                this.Width = SessionController.Instance.SessionView.ActualWidth / 2;
                this.Height = SessionController.Instance.SessionView.ActualHeight;
                xContainer.Height = this.Height;
                xContainer.Width = this.Width - 30;
                nodeContent.Width = xContainer.Width - 75;
                resizer.Height = Height;
                exitButtonContainer.Width = xContainer.Width;
                Properties.Width = xContainer.Width - 15;
                TagContainer.Width = xContainer.Width - 15;
                propLine.X2 = Properties.Width - 15;
                tagLine.X2 = TagContainer.Width - 15;
                NewTagBox.Width = TagContainer.Width - 163;
                Canvas.SetLeft(this, SessionController.Instance.SessionView.ActualWidth - Width);
                Canvas.SetLeft(nodeContent, Canvas.GetLeft(xContainer) + 0.5 * (xContainer.ActualWidth - nodeContent.ActualWidth));
                Canvas.SetTop(resizerImage, (resizer.Height / 2) - (resizerImage.Height / 2));
            }
            if (propertyChangedEventArgs.PropertyName == "Title")
            {
                TitleEnter.Text = ((DetailViewerViewModel)DataContext).Title;
            }
            var vm = (DetailViewerViewModel) DataContext;
            Tags.ItemsSource = vm.Tags;
        }

        private async void NewTagBox_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.Enter)
            {
                await AddTag();
            }
        }

        private async void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            await AddTag();
        }

        private async Task AddTag()
        {
            tagLine.Opacity = 1;
            var vm = (DetailViewerViewModel)DataContext;
            string newTag = NewTagBox.Text.Trim();
            if (newTag != "")
            {
                vm.AddTag(newTag);
                Tags.ItemsSource = vm.Tags;
            }
            NewTagBox.Text = "";
        }

        private void topBar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((e.OriginalSource as UIElement) == (UIElement)exitButton)
            {
                return;
            }
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            //Canvas.SetTop(this, Canvas.GetTop(this) + e.Delta.Translation.Y);

            e.Handled = true;
        }

        private async void closeDV_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Anim.To(this, "Alpha", 0, 400);
            IsHitTestVisible = false;
            var vm = (DetailViewerViewModel)DataContext;
            var textview = (vm.View as TextDetailView);
            textview?.Dispose();
        }

        private void TitleEnter_OnTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            ((ElementInstanceViewModel) ((DetailViewerViewModel) DataContext).View.DataContext).Model.Title = TitleEnter.Text;
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            double rightCoord = Canvas.GetLeft(this) + this.Width;

            if (this.Width > 250 || e.Delta.Translation.X < 0)
            {
                this.Width -= e.Delta.Translation.X;
                xContainer.Width = this.Width - 30;
                exitButtonContainer.Width = xContainer.Width;
                nodeContent.Width = xContainer.Width - 75;
                Properties.Width = xContainer.Width - 15;
                TagContainer.Width = xContainer.Width - 15;
                propLine.X2 = Properties.Width - 15;
                tagLine.X2 = TagContainer.Width - 15;
                NewTagBox.Width = TagContainer.Width - 163;
                Canvas.SetLeft(nodeContent, Canvas.GetLeft(xContainer) + 0.5 * (xContainer.ActualWidth - nodeContent.ActualWidth));
                Canvas.SetLeft(this, rightCoord - this.Width);

                e.Handled = true;
            }
        }
        
    }
}
