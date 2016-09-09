using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using Path = System.IO.Path;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioDetailHomeTabView : UserControl
    {
        
        public AudioDetailHomeTabView(AudioDetailHomeTabViewModel vm)
        {
            this.DataContext = vm; // has to be set before initComponent so child xaml elements inherit it
            this.InitializeComponent();

            SizeChanged += OnSizeChanged;


            if (!vm.LibraryElementController.ContentLoaded)
            {
                UITask.Run(async delegate
                {
                    await vm.LibraryElementController.LoadContentDataModelAsync();
                    LoadAudio();
                });
            }
            else
            {
                LoadAudio();
            }


            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;

            vm.LibraryElementController.Disposed += ControllerOnDisposed;
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {

            Dispose();
        }

        private void ControllerOnDisposed(object source, object args)
        {

            Dispose();
        }

        private void LoadAudio()
        {
            var vm = DataContext as AudioDetailHomeTabViewModel;
            xAudioPlayer.SetLibraryElement(vm?.LibraryElementController as AudioLibraryElementController, false);
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            xAudioPlayer.SetSize(sizeChangedEventArgs.NewSize.Width, 200);
        }



        public void Dispose()
        {
            var vm = (AudioDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;

            SizeChanged -= OnSizeChanged;
            xAudioPlayer.Dispose();
        }
      
    }
}
