using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Components.Nodes;
using NuSysApp.Controller;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : AnimatableUserControl, IThumbnailable
    {
        
        public AudioNodeView(AudioNodeViewModel vm)
        {
            this.DataContext = vm; // has to be set before initComponent so child xaml elements inherit it
            InitializeComponent();
            if (!vm.Controller.LibraryElementController.ContentLoaded)
            {
                UITask.Run(async delegate
                {
                    await vm.Controller.LibraryElementController.LoadContentDataModelAsync();
                    LoadAudio();
                });
            }
            else
            {
                LoadAudio();
            }

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void LoadAudio()
        {
            var vm = DataContext as AudioNodeViewModel;
            MediaPlayer.AudioSource = new Uri(vm.Controller.LibraryElementController.Data);
        }

        private void ControllerOnDisposed(object source, object args)
        {
            nodeTpl.Dispose();
            var vm = (AudioNodeViewModel)DataContext;
            if (vm == null)
            {
                return;
            }
            // relic from wave forms still might be added back though
            //(DataContext as AudioNodeViewModel).OnVisualizationLoaded -= LoadPlaybackElement; 

            nodeTpl?.Dispose();
            if (vm?.Controller != null)
            {
                vm.Controller.Disposed -= ControllerOnDisposed;
            }
            DataContext = null;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) DataContext;
            vm.Controller.RequestDelete();
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(MediaPlayer, width, height);
            return r;
        }
    }
}
