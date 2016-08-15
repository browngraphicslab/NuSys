using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Threading.Tasks;
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageDetailHomeTabView : UserControl
    {
        public ImageDetailHomeTabView(ImageDetailHomeTabViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
            
            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            xClippingWrapper.Controller = vm.LibraryElementController;
            xClippingWrapper.ProcessLibraryElementController();

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose();
        }

        private void Dispose()
        {
            xClippingWrapper.Dispose();
        }
        
        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (ImageDetailHomeTabViewModel) DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;
            xClippingWrapper.Dispose();
            DataContext = null;
        }

        private void Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
             
        }
    }
}
