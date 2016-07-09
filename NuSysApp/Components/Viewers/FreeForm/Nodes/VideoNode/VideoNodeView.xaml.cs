using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
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
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _isRecording;
        private List<LinkedTimeBlockViewModel> _timeBlocks;
        private bool _addTimeBlockMode;
        private Line _temporaryLinkVisual;
        private bool _shouldBePlay;


        public VideoNodeView(VideoNodeViewModel vm)
        {
            _shouldBePlay = false;
            _addTimeBlockMode = false;
            vm.PropertyChanged += Node_SelectionChanged;
            this.InitializeComponent();
            this.DataContext = vm;
            //playbackElement.AutoPlay = false;
            if (SessionController.Instance.ContentController.ContainsAndLoaded(vm.Model.LibraryId))
            {
                LoadVideo(this);
            }
            else
            {
                vm.Controller.LibraryElementController.Loaded += LoadVideo;
            }
            

            _isRecording = false;
            vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            //scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            //  playbackElement.Play();
            //playbackElement.Position = new TimeSpan(0);
            //playbackElement.Stop();

            vm.Controller.Disposed += ControllerOnDisposed;

            ((VideoNodeModel)vm.Model).OnJump += VideoNodeView_OnJump;

           // playbackElement.MediaEnded += MediaEnded;
            vm.OnGetMediaPlayerWidth += OnGetMediaPlayerWidth;
            vm.OnGetMediaPlayerHeight += OnGetMediaPlayerHeight;
            VideoMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            VideoMediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;
            vm.OnRegionSeekPassing += VideoMediaPlayer.onSeekedTo;



        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VideoNodeViewModel;
            vm.VideoDuration = VideoMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            vm.UpdateRegions();

        }

        private double OnGetMediaPlayerHeight(object sender)
        {
            return VideoMediaPlayer.MediaPlayer.ActualHeight;
        }

        private double OnGetMediaPlayerWidth(object sender)
        {
            return VideoMediaPlayer.MediaPlayer.ActualWidth;
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            VideoNodeView_OnJump(new TimeSpan(0));
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
            
            if (LinkedTimeBlock._box1 != null && (LinkedTimeBlock._box1.FocusState == FocusState.Unfocused))
            {
                LinkedTimeBlock.removeBox();
            }
        }

        public void VideoNodeView_OnJump(TimeSpan time)
        {
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (VideoNodeViewModel) DataContext;
            vm.Controller.LibraryElementController.Loaded -= LoadVideo;
            vm.PropertyChanged -= Node_SelectionChanged;
            vm.LinkedTimeModels.CollectionChanged -= LinkedTimeBlocks_CollectionChanged;
            vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }

        private void LoadVideo(object sender)
        {
            var vm = DataContext as VideoNodeViewModel;
            VideoMediaPlayer.Source = vm.Controller.LibraryElementController.GetSource();
        }

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }
        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            /*double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = time;

            if (playbackElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "playbackElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                scrubBar.SetBinding(ProgressBar.ValueProperty, b);

                //playbackElement.Play();
            }
*/
        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

                    //double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                  //  double seconds = playbackElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                    //TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                        Binding b = new Binding();
                        b.ElementName = "playbackElement";
                        b.Path = new PropertyPath("Position.TotalMilliseconds");
                        //scrubBar.SetBinding(ProgressBar.ValueProperty, b);

                        //playbackElement.Play();
            e.Handled = true;
        }

        


        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //foreach (var element in _timeBlocks)
           // {
           //     element.ResizeLine1();
           // }
        }


       


        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDelete();
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(grid, width, height);
            return r;
        }





        private void Region_OnClick(object sender, RoutedEventArgs e)
        {
            //if (_addTimeBlockMode == false)
            //{
            //    _addTimeBlockMode = true;
            //}
            //else
            //{
            //    _addTimeBlockMode = false;
            //}



        }

        private void ScrubBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
                _shouldBePlay = false;
                ((UIElement)sender).ReleasePointerCapture(e.Pointer);

        }


        public void ReSaveLinkModels()
        {
            (DataContext as ElementViewModel).Controller.SaveTimeBlock();

        }

        private void VideoMediaPlayer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
        }
    }
}

