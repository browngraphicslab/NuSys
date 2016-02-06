using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class FullScreenViewerView : AnimatableUserControl
    {
        public FullScreenViewerView()
        {
            InitializeComponent();

            Opacity = 0;
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
           
            };

          DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
          {
              if (!(DataContext is FullScreenViewerViewModel))
                  return;
              
              var vm = (FullScreenViewerViewModel)DataContext;
              vm.PropertyChanged += OnPropertyChanged;
              Tags.ItemsSource = vm.Tags;
              vm.MakeTagList();
          };
            IsHitTestVisible = false;
            //PointerReleased += OnPointerReleased;
        }

        /*
        private void OnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            if (!((pointerRoutedEventArgs.OriginalSource as FrameworkElement).DataContext is FullScreenViewerViewModel)) { 
                Anim.To(this, "Alpha", 0, 400);
                IsHitTestVisible = false;
                var vm = (FullScreenViewerViewModel)DataContext;
                var textview = (vm.View as TextDetailView);
                textview?.Dispose();
            }
        }
        */

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "View")
            {
                Anim.To(this, "Alpha", 1, 400, null, (a,i) => { IsHitTestVisible = true; });
                Width = SessionController.Instance.SessionView.ActualWidth / 2;
                Properties.Width = SessionController.Instance.SessionView.ActualWidth / 2 - 20;
                TagContainer.Width = SessionController.Instance.SessionView.ActualWidth / 2 - 20;
                propLine.X2 = SessionController.Instance.SessionView.ActualWidth / 2 - 40;
                tagLine.X2 = SessionController.Instance.SessionView.ActualWidth / 2 - 40;
                NewTagBox.Width = SessionController.Instance.SessionView.ActualWidth / 2 - 163;
                Canvas.SetLeft(this, SessionController.Instance.SessionView.ActualWidth / 4);
                Height = SessionController.Instance.SessionView.ActualHeight;
            }
            var vm = (FullScreenViewerViewModel) DataContext;
            Tags.ItemsSource = vm.Tags;
        }

        private void AddTagButton_OnClick(object sender, RoutedEventArgs e)
        {
            tagLine.Opacity = 1;

            var vm = (FullScreenViewerViewModel)DataContext;
            string newTag = NewTagBox.Text.Trim();
            if (newTag != "")
            {
                vm.AddTag(newTag);
                Tags.ItemsSource = vm.Tags;
            }
            NewTagBox.Text = "";
        }

        private void closeDV_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Anim.To(this, "Alpha", 0, 400);
            IsHitTestVisible = false;
            var vm = (FullScreenViewerViewModel)DataContext;
            var textview = (vm.View as TextDetailView);
            textview?.Dispose();
        }
    }
}
