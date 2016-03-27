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
using NuSysApp.Components.Nodes;
using NuSysApp.Controller;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _stopped;
        private bool _loaded;
        private List<LinkedTimeBlockViewModel> _timeBlocks;


        public AudioNodeView(AudioNodeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _stopped = true;
            vm.PropertyChanged += Node_SelectionChanged;
            _loaded = false;
            (DataContext as AudioNodeViewModel).addTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            playbackElement.MediaFailed += PlaybackElement_MediaFailed;
            ((AudioNodeModel)(vm.Model)).Controller = new MediaController(playbackElement);
            ((AudioNodeModel)(vm.Model)).Controller.OnPlay += Controller_OnPlay;
            ((AudioNodeModel)(vm.Model)).Controller.OnPause += Controller_OnPause;
            ((AudioNodeModel)(vm.Model)).Controller.OnStop += Controller_OnStop;

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void PlaybackElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (AudioNodeViewModel)DataContext;
            vm.PropertyChanged -= Node_SelectionChanged;
            (DataContext as AudioNodeViewModel).removeTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = null;
            
            ((AudioNodeModel)(vm.Model)).Controller.OnPlay -= Controller_OnPlay;
            ((AudioNodeModel)(vm.Model)).Controller.OnPause -= Controller_OnPause;
            ((AudioNodeModel)(vm.Model)).Controller.OnStop -= Controller_OnStop;
            ((AudioNodeModel)(vm.Model)).Controller = null;
            playbackElement.MediaEnded -= PlaybackElementOnMediaEnded;
            (DataContext as AudioNodeViewModel).OnVisualizationLoaded -= LoadPlaybackElement;
            nodeTpl.Dispose();

            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        private void Controller_OnStop(MediaElement playbackElement)
        {
            _stopped = true;
            play.Opacity = 1;
            pause.Opacity = 1;
        }

        private void Controller_OnPause(MediaElement playbackElement)
        {
            play.Opacity = 1;
            pause.Opacity = 0.3;
        }

        private void Controller_OnPlay(MediaElement playbackElement)
        {
            play.Opacity = .3;
            pause.Opacity = 1;
            playbackElement.MediaEnded += PlaybackElementOnMediaEnded;
        }

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as AudioNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);

            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
        }

        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            _stopped = true;
            play.Opacity = 1;
            pause.Opacity = 1;
            e.Handled = true;
            playbackElement.Position = new TimeSpan(0);
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Stop();

        }

        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {

            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            play.Opacity = .3;
            pause.Opacity = 1;
            playbackElement.MediaEnded += PlaybackElementOnMediaEnded;
            
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Play();
            

        }

        private void PlaybackElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            play.Opacity = 1;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) DataContext;
            vm.Controller.RequestDelete();
        }

        private async void RenderImageSource(Grid RenderedGrid)
        {

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            double x = grid.Height;
            grid.Width = RenderedGrid.Width * 2;
            await renderTargetBitmap.RenderAsync(RenderedGrid, 1000, 100);
            grid.Width = x;
            VisualizationImage.Source = renderTargetBitmap;
            grid.Children.Remove(RenderedGrid);

        }


        private async void OnPause_Click(object sender, RoutedEventArgs e)
        {

            play.Opacity = 1;
            pause.Opacity = 0.3;
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Pause();



        }



        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (NodeViewModel)this.DataContext;

                if (vm.IsSelected)
                {
                    //var slideout = (Storyboard)Application.Current.Resources["slideout"];
                    slideout.Begin();
                }
                else
                {
                    //var slidein = (Storyboard)Application.Current.Resources["slidein"];
                    slidein.Begin();
                }
            }*/
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(grid, width, height);
            return r;
        }

        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.ScrubJump(time);
            

        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                double milliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)milliseconds);
                ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.ScrubJump(time);

            }
            e.Handled = true;
        }

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
            if ((DataContext as AudioNodeViewModel).VisualGrid != null)
            {
                LoadPlaybackElement();
            }
            else
            {
                (DataContext as AudioNodeViewModel).OnVisualizationLoaded += LoadPlaybackElement;
            }
        }
        private void LoadPlaybackElement()
        {
            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            playbackElement.MediaEnded += PlaybackElementOnMediaEnded;
        }

        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var element in _timeBlocks)
            {
                element.ResizeLine1();
            }
        }

        private void ScrubBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Scrub();
        }
    }
}
