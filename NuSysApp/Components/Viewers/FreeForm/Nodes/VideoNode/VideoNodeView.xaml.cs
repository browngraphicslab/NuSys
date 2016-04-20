using System;
using System.Collections.Generic;
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


        public VideoNodeView(VideoNodeViewModel vm)
        {
            _addTimeBlockMode = false;
            this.InitializeComponent();
            this.DataContext = vm;
            //playbackElement.AutoPlay = false;
            if (SessionController.Instance.ContentController.ContainsAndLoaded(vm.Model.LibraryId))
            {
                LoadVideo();
            }
            else
            {
                vm.Controller.LibraryElementModel.OnLoaded += LoadVideo;
            }
            

            _isRecording = false;
            vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            //  playbackElement.Play();
            playbackElement.Position = new TimeSpan(0);
            //playbackElement.Stop();

            vm.Controller.Disposed += ControllerOnDisposed;

            ((VideoNodeModel)vm.Model).OnJump += VideoNodeView_OnJump;

        }

        private void VideoNodeView_OnJump(TimeSpan time)
        {
            playbackElement.Position = time;
        }

        private void ControllerOnDisposed(object source)
        {
            playbackElement.Stop();
            var vm = (VideoNodeViewModel) DataContext;
            vm.Controller.LibraryElementModel.OnLoaded -= LoadVideo;
            vm.LinkedTimeModels.CollectionChanged -= LinkedTimeBlocks_CollectionChanged;
            vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }

        private void LoadVideo()
        {
            var content = (DataContext as VideoNodeViewModel).Controller.LibraryElementModel;
            if (content != null)
            {
                InMemoryRandomAccessStream memoryStream;
                var stream = content.ViewUtilBucket.ContainsKey("videoStream")
                    ? (InMemoryRandomAccessStream) content.ViewUtilBucket["videoStream"]
                    : null;

                if (stream == null)
                {
                    memoryStream = new InMemoryRandomAccessStream();
                    var nodeModel = (((VideoNodeViewModel) DataContext).Model as VideoNodeModel);
                    var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get(nodeModel.LibraryId).Data);
                    memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
                    memoryStream.Seek(0);
                    content.ViewUtilBucket["videoStream"] = stream;
                }
                else
                {
                    memoryStream = stream;
                }
                playbackElement.SetSource(memoryStream, "video/mp4");
                
                ((VideoNodeViewModel) DataContext).Controller.LibraryElementModel.OnLoaded-= LoadVideo;
            }
            playbackElement.Position = new TimeSpan(0);


        }

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as VideoNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);
            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            Grid.SetRow(line, 1);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
            (DataContext as ElementViewModel).Controller.SaveTimeBlock();
        }
        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
            double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

            TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
            playbackElement.Position = time;
        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                if (_addTimeBlockMode == false)
                {
                    double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                    double seconds = playbackElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                    TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                    playbackElement.Position = time;
                }
                else if (_addTimeBlockMode == true)
                {
                    ((UIElement)sender).CapturePointer(e.Pointer);

                    if (grid.Children.Contains(_temporaryLinkVisual))
                    {
                        _temporaryLinkVisual.X2 = e.GetCurrentPoint(grid).Position.X;
                    }
                    else
                    {
                        _temporaryLinkVisual.X1 = e.GetCurrentPoint(grid).Position.X;
                        _temporaryLinkVisual.X2 = e.GetCurrentPoint(grid).Position.X;
                        grid.Children.Add(_temporaryLinkVisual);
                    }


                }

            }
            e.Handled = true;
        }

        


        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach (var element in _timeBlocks)
            {
                element.ResizeLine1();
            }
        }


       


        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            /*     if (_recording)
                 {
                     ToggleRecording(CurrentAudioFile.Name);
                 }*/
            playbackElement.Stop();
            //scrubBar.Value = 0;
            //playbackElement.Position = new TimeSpan(0,0,0,0,1);
            scrubBar.Value = 0;

            //playbackElement.Stop();


            //playbackElement.Position = new TimeSpan(0,0,0,0,0);

            //       _stopped = true;
            e.Handled = true;
        }


        private async void OnPlay_Click(object sender, RoutedEventArgs e)
        {
            /*   if (_recording)
               {
                  ToggleRecording(CurrentAudioFile.Name);
               }
               else
               {
                  // pause.Opacity = 1;
                   play.Opacity = .3;
                   if (_stopped)
                   {
                       _stopped = false;
                       if (CurrentAudioFile == null) return;
                       var stream = await CurrentAudioFile.OpenAsync(FileAccessMode.Read);
                       playbackElement.SetSource(stream, CurrentAudioFile.FileType);
                   }MediaType.Video
                   playbackElement.MediaEnded += delegate(object o, RoutedEventArgs e2)
                   {
                       play.Opacity = 1;
                   };*/
            if (playbackElement.CurrentState != MediaElementState.Playing)
            {
                Binding b = new Binding();
                b.ElementName = "playbackElement";
                b.Path = new PropertyPath("Position.TotalMilliseconds");
                scrubBar.SetBinding(ProgressBar.ValueProperty, b);

                playbackElement.Play();
            }
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {

            playbackElement.Pause();
            //playbackElement.Position = new TimeSpan(0);
            //    pause.Opacity = .3;
        }
        /*private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }*/
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




        private void PlaybackElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = (VideoNodeViewModel)this.DataContext;
            var model = (VideoNodeModel)vm.Model;
            model.ResolutionX = playbackElement.AspectRatioWidth;
            model.ResolutionY = playbackElement.AspectRatioHeight;

            double width = this.Width;
            double height = this.Height;
            vm.Controller.SetSize(width, height);
            playbackElement.Position = new TimeSpan(0);
            this.AddAllLinksVisually();
            _temporaryLinkVisual = new Line();
            _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
            Grid.SetRow(_temporaryLinkVisual, 1);
            _temporaryLinkVisual.StrokeThickness = scrubBar.ActualHeight;
            _temporaryLinkVisual.Y1 = scrubBar.ActualHeight / 2 + scrubBar.Margin.Top;
            _temporaryLinkVisual.Y2 = scrubBar.ActualHeight / 2 + scrubBar.Margin.Top;
            _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
            _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
            _temporaryLinkVisual.Opacity = 1;

        }
        public int AspectHeight { get { return playbackElement.AspectRatioHeight; } }
        public int AspectWidth { get { return playbackElement.AspectRatioWidth; } }

        private void Region_OnClick(object sender, RoutedEventArgs e)
        {
            _addTimeBlockMode = true;

        }

        private void ScrubBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_addTimeBlockMode == true)
            {
                if (grid.Children.Contains(_temporaryLinkVisual))
                {
                    int xwithinscrub =
                        (int)(_temporaryLinkVisual.X1 - (Canvas.GetLeft(scrubBar) + scrubBar.Margin.Left));
                    if (xwithinscrub < 0)
                    {
                        xwithinscrub = 0;
                    }
                    else if (xwithinscrub > Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth)
                    {
                        xwithinscrub = (int)(Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth);
                    }
                    int start = (int)((xwithinscrub / (scrubBar.ActualWidth)) * playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    int x2withinscrub = (int)(_temporaryLinkVisual.X2 - (Canvas.GetLeft(scrubBar) + scrubBar.Margin.Left));
                    if (x2withinscrub < 0)
                    {
                        x2withinscrub = 0;
                    }
                    else if (x2withinscrub > Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth)
                    {
                        x2withinscrub = (int)(Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth);
                    }
                    int end = (int)((x2withinscrub / (scrubBar.ActualWidth)) * playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    CreateTimeBlock(new TimeSpan(0, 0, 0, 0, start), new TimeSpan(0, 0, 0, 0, end));
                    grid.Children.Remove(_temporaryLinkVisual);
                    ((UIElement)sender).ReleasePointerCapture(e.Pointer);


                }
            }
        }

        private void AddAllLinksVisually()
        {
            foreach (var element in (DataContext as VideoNodeViewModel).LinkedTimeModels)
            {
                var timeBlockVM = new LinkedTimeBlockViewModel(element, playbackElement.NaturalDuration.TimeSpan, scrubBar);
                LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
                Grid.SetRow(line, 1);
                _timeBlocks.Add(timeBlockVM);
                grid.Children.Add(line);
                timeBlockVM.setUpHandlers(line.getLine());
            }
            scrubBar.SizeChanged += ScrubBar_OnSizeChanged;

        }

        public void CreateTimeBlock(TimeSpan start, TimeSpan end)
        {
            LinkedTimeBlockModel model = new LinkedTimeBlockModel(start, end);
            LinkedTimeBlockViewModel link = new LinkedTimeBlockViewModel(model, playbackElement.NaturalDuration.TimeSpan, scrubBar);

            (DataContext as VideoNodeViewModel).AddLinkTimeModel(model);

        }



    }
}

