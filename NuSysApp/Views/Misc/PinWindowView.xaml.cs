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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PinWindowView : UserControl
    {
        private FloatingMenuView _floatingMenu;

        public PinWindowView()
        {
            this.InitializeComponent();
          //  Border.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 98, 189, 197));           
        }


        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((TextBlock)sender).DataContext as PinViewModel;
            var pinModel = (PinModel) pinvm.Model;

            var vm = (WorkspaceViewModel)this.DataContext;

            _floatingMenu.WorkspaceView.TranslateXAnimation.To = -pinModel.X + Window.Current.Bounds.Width/2;
            _floatingMenu.WorkspaceView.TranslateYAnimation.To = -pinModel.Y + Window.Current.Bounds.Height/2;
            _floatingMenu.WorkspaceView.ScaleXAnimation.To = 1;
            _floatingMenu.WorkspaceView.ScaleYAnimation.To = 1;
            _floatingMenu.WorkspaceView.PinAnimationStoryboard.Completed += delegate(object o, object o1)
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
            _floatingMenu.WorkspaceView.PinAnimationStoryboard.Begin();

            e.Handled = true;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((Button)sender).DataContext as PinViewModel;
            var pinModel = (PinModel)pinvm.Model;
            NetworkConnector.Instance.RequestDeleteSendable(pinModel.ID);
            e.Handled = true;
            /*
            var pinvm = ((Button)sender).DataContext as PinViewModel;
            var pinModel = (PinModel) pinvm.Model;

            var vm = (WorkspaceViewModel)this.DataContext;

            vm.AtomViewList.Remove(pinvm.View);
            vm.PinViewModelList.Remove(pinvm);*/
        }

        public void setFloatingMenu(FloatingMenuView floatingMenu)
        {
            _floatingMenu = floatingMenu;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            _floatingMenu.CloseAllSubMenus();
        }

        private void PinWindow_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var vm = _floatingMenu.WorkspaceView.DataContext as WorkspaceViewModel;
            vm.Model.OnPinCreation -= vm.CreatePinHandler;
            e.Handled = true;
        }

        private void PinWindow_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var vm = _floatingMenu.WorkspaceView.DataContext as WorkspaceViewModel;
            vm.Model.OnPinCreation += vm.CreatePinHandler;
            e.Handled = true;
        }

        private void PinWindow_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //var transMat = ((MatrixTransform) this.RenderTransform).Matrix;
            //transMat.OffsetX += e.Delta.Translation.X;
            //transMat.OffsetY += e.Delta.Translation.Y;
            //var transform = new MatrixTransform();
            //transform.Matrix = transMat;
            //this.RenderTransform = transform;
            //e.Handled = true;
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
