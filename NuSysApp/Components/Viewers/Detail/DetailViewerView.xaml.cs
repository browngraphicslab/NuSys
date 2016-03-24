﻿using System;
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
            Visibility = Visibility.Collapsed;

  
     

            DataContextChanged += delegate(FrameworkElement sender, DataContextChangedEventArgs args)
              {
                  if (!(DataContext is DetailViewerViewModel))
                      return;
              

                  var vm = (DetailViewerViewModel)DataContext;
                  vm.PropertyChanged += OnPropertyChanged;
                  Tags.ItemsSource = vm.Tags;
                  vm.MakeTagList();

                  this.Width = SessionController.Instance.SessionView.ActualWidth / 2;
                  this.Height = SessionController.Instance.SessionView.ActualHeight;
                  this.MaxHeight = SessionController.Instance.SessionView.ActualHeight;
                  this.MaxWidth = SessionController.Instance.SessionView.ActualWidth - resizer.ActualWidth-30;
                  Canvas.SetTop(this, 0);
                  Canvas.SetLeft(this, SessionController.Instance.SessionView.ActualWidth - Width);
                  // Metadata.ItemsSource = vm.Metadata;
              };

            
        }

        public async void ShowElement(ElementController controller)
        {
            var vm = (DetailViewerViewModel)DataContext;
            if (await vm.ShowElement(controller))
                Visibility = Visibility.Visible;

        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {

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
            //tagLine.Opacity = 1;
            var vm = (DetailViewerViewModel)DataContext;
            string newTag = NewTagBox.Text.Trim();
            if (newTag != "")
            {
                vm.AddTag(newTag);
                Tags.ItemsSource = vm.Tags;
            }
            NewTagBox.Text = "";
        }

        private async void NewKey_OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.Enter)
            {
                await AddMetadataKey(e);
            }
        }

        private async Task AddMetadataKey(KeyRoutedEventArgs e)
        {
            var vm = (DetailViewerViewModel)DataContext;
            string newKey = ((TextBox)e.OriginalSource).Text.Trim();
            if (newKey != "")
            {
              vm.AddMetadata(newKey, "", false);
            }
          //  NewMetadataBox.Text = "";
        }

        private void topBar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((e.OriginalSource as UIElement) == (UIElement)exitButton)
            {
                return;
            }
            if ((Canvas.GetLeft(this) + this.ActualWidth < SessionController.Instance.SessionView.ActualWidth || e.Delta.Translation.X < 0)
                && (Canvas.GetLeft(this) > 0 || e.Delta.Translation.X > 0))
            {
                Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            }
            e.Handled = true;
        }

        private async void closeDV_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            var vm = (DetailViewerViewModel)DataContext;
            var textview = (vm.View as TextDetailView);
            textview?.Dispose();
        }

        private void metaData_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var toggle = (TitleEnter.Visibility == Visibility.Collapsed);
            TitleEnter.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
            TagContainer.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
            nodeContent.Visibility = toggle ? Visibility.Visible : Visibility.Collapsed;
           // MetadataContainer.Visibility = toggle ? Visibility.Collapsed : Visibility.Visible;
        }

        private void TitleEnter_OnTextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
      //      ((ElementViewModel) ((DetailViewerViewModel) DataContext).View.DataContext).Model.Title = TitleEnter.Text;
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
      //      if (!_allowResize)
        //        return;

            double rightCoord = Canvas.GetLeft(this) + this.Width;
       
            if ((this.Width > 250 || e.Delta.Translation.X < 0) && (Canvas.GetLeft(this) > 0 || e.Delta.Translation.X > 0) && (Canvas.GetLeft(this) > 30 || e.Delta.Translation.X > 0))
            {
                this.Width -= e.Delta.Translation.X;
               // xContainer.Width = this.Width - 30;

               // exitButtonContainer.Width = xContainer.Width;
               
                Canvas.SetLeft(this, rightCoord - this.Width);

                e.Handled = true;
            }

            if (Canvas.GetLeft(this) <= 30)
            {
                Canvas.SetLeft(this,30);
            }
        }
        
    }
}
