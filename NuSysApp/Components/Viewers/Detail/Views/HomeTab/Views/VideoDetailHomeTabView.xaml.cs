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

            //Show/hide region buttons need access to the audiowrapper for event handlers
            xShowHideRegionButtons.Wrapper = VideoMediaPlayer.AudioWrapper;

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

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose();
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            VideoMediaPlayer.Source = new Uri(vm.LibraryElementController.Data);
        }

        public void Dispose()
        {
            VideoMediaPlayer.StopVideo();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (VideoNodeViewModel)DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
        }
    }
}
