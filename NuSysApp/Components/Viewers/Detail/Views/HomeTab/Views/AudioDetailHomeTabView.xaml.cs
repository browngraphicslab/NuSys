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
        private bool _stopped;

        public event ContentLoadedEventHandler ContentLoaded;
        public delegate void ContentLoadedEventHandler(object sender);

        public AudioMediaPlayer AudioMediaPlayer { get { return MediaPlayer; } }

        
        public AudioDetailHomeTabView(AudioDetailHomeTabViewModel vm)
        {
            this.DataContext = vm; // has to be set before initComponent so child xaml elements inherit it
            this.InitializeComponent();

            AudioMediaPlayer.AudioSource = new Uri(vm.LibraryElementController.Data);
            AudioMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;

            vm.LibraryElementController.Disposed += ControllerOnDisposed;
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AudioDetailHomeTabViewModel;
            vm.Duration = AudioMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            ContentLoaded?.Invoke(this);
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (AudioNodeViewModel) DataContext;
            MediaPlayer.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;   
        }


        public void Dispose()
        {
            MediaPlayer.Dispose();
        }
      
    }
}
