using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Nodes;
using System.Threading.Tasks;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoDetailHomeTabView : UserControl
    {

        public VideoDetailHomeTabView(VideoDetailHomeTabViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;
            SizeChanged += OnSizeChanged;

            if (!vm.LibraryElementController.ContentLoaded)
            {
                UITask.Run(async delegate
                {
                    await vm.LibraryElementController.LoadContentDataModelAsync();
                    LoadVideo(this);
                });
            }
            else
            {
                LoadVideo(this);
            }



            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed; 
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var vm = SessionController.Instance.SessionView.DetailViewerView.DataContext as DetailViewerViewModel;
            var vlem = (vm.CurrentElementController.LibraryElementModel as VideoLibraryElementModel);

            VideoPlayer.SetSize(sizeChangedEventArgs.NewSize.Width, sizeChangedEventArgs.NewSize.Width/vlem.Ratio);
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {

            Dispose();
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            VideoPlayer.SetLibraryElement(vm.LibraryElementController as AudioLibraryElementController, false);
        }

        public void Dispose()
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            Debug.Assert(vm != null);
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;

            SizeChanged -= OnSizeChanged;
            VideoPlayer.Dispose();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            Dispose();
        }
    }
}
