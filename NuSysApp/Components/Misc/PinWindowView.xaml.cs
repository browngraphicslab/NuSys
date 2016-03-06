using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using NuSysApp;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PinWindowView : UserControl
    {
        private FloatingMenuView _floatingMenu;

        public PinWindowView()
        {
            this.InitializeComponent();
        }
        
        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((TextBlock)sender).DataContext as PinViewModel;
            var pinModel = (PinModel) pinvm.Model;

            var vm = (FreeFormViewerViewModel)this.DataContext;

            transXAnimation.To = -pinModel.X + Window.Current.Bounds.Width/2;
            transYAnimation.To = -pinModel.Y + Window.Current.Bounds.Height/2;
            scaleXAnimation.To = 1;
            scaleYAnimation.To = 1;
            canvasStoryboard.Completed += delegate(object o, object o1)

            {
                var c = new CompositeTransform
                {
                    ScaleX = 1,
                    ScaleY = 1,
                    TranslateX = -pinModel.X + Window.Current.Bounds.Width / 2,
                    TranslateY = -pinModel.Y + Window.Current.Bounds.Height / 2,
                };
                vm.CompositeTransform = c;
            };

            canvasStoryboard.Begin();

            e.Handled = true;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinModel = (PinModel)((Button) sender).DataContext;
            var pinWindowViewModel = (PinWindowViewModel) DataContext;
            pinWindowViewModel.Pins.Remove(pinModel);
            
            ////NetworkConnector.Instance.RequestDeleteSendable(pinModel.ID);
            e.Handled = true;
        }

        public void setFloatingMenu(FloatingMenuView floatingMenu)
        {
            _floatingMenu = floatingMenu;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            _floatingMenu.CloseAllSubMenus();
        }


        private void PinWindow_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            CompositeTransform current = (CompositeTransform)(this.RenderTransform);
            CompositeTransform c = new CompositeTransform
            {
                TranslateX = current.TranslateX +  e.Delta.Translation.X,
                TranslateY = current.TranslateY + e.Delta.Translation.Y
            };
            this.RenderTransform = c;
            e.Handled = true;
        }
    }
}
