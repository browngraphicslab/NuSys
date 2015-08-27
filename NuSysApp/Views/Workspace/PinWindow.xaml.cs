using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public sealed partial class PinWindow : UserControl
    {
        public PinWindow()
        {
            this.InitializeComponent();
            Border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 81, 220, 231));
        }

        public static readonly DependencyProperty pinList = DependencyProperty.Register(
  "PinList",
  typeof(ObservableCollection<PinViewModel>),
  typeof(MinimapView),
  new PropertyMetadata(new ObservableCollection<PinViewModel>())
);
        public ObservableCollection<PinViewModel> PinList
        {
            get { return (ObservableCollection<PinViewModel>)GetValue(pinList); }
            set
            {
                SetValue(pinList, value);
                //         ((MinimapViewModel)DataContext).PinList = value;
            }
        }
        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((TextBlock)sender).DataContext as PinViewModel;
            var vm = (WorkspaceViewModel)this.DataContext;

            var c = new CompositeTransform
            {
                ScaleX = 1,
                ScaleY = 1,
                TranslateX = -pinvm.Transform.Matrix.OffsetX + ((Canvas)this.Parent).ActualWidth / 2,
                TranslateY = -pinvm.Transform.Matrix.OffsetY + ((Canvas)this.Parent).ActualHeight / 2
            };
            vm.CompositeTransform = c;
        }

        //private void ShowPins(object sender, TappedRoutedEventArgs e)
        //{
        //    if (IC2.Visibility == Visibility.Collapsed)
        //    {
        //        IC2.Visibility = Visibility.Visible;
        //        PinButton.Content = "pins -";
        //    }
        //    else
        //    {
        //        IC2.Visibility = Visibility.Collapsed;
        //        PinButton.Content = "pins +";
        //    }

        //}
    }
}
