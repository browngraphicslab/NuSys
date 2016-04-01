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
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoDetailView : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private bool _addTimeBlockMode;
        private bool _loaded;
        private Line _temporaryLinkVisual;
        private List<LinkedTimeBlockViewModel> _timeBlocks;



        public VideoDetailView(VideoNodeViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
            var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get((vm.Model as VideoNodeModel).LibraryId).Data);
            memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
            memoryStream.Seek(0);
            playbackElement.SetSource(memoryStream, "video/mp4");
            _isRecording = false;
            vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            
            vm.Controller.Disposed += ControllerOnDisposed;

            //Loaded += delegate (object sender, RoutedEventArgs args)
            //{
            //    var sw = SessionController.Instance.SessionView.ActualWidth / 2;
            //    var sh = SessionController.Instance.SessionView.ActualHeight / 2;

            //    var ratio = playbackElement.ActualWidth > playbackElement.ActualHeight ? playbackElement.ActualWidth / sw : playbackElement.ActualHeight / sh;
            //    playbackElement.Width = playbackElement.ActualWidth / ratio;
            //    playbackElement.Height = playbackElement.ActualHeight / ratio;


            //};
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (VideoNodeViewModel)DataContext;
            vm.LinkedTimeModels.CollectionChanged -= LinkedTimeBlocks_CollectionChanged;
            scrubBar.SizeChanged -= ScrubBar_OnSizeChanged;
            playbackElement.MediaEnded -= PlaybackElementOnMediaEnded;
            if (_temporaryLinkVisual != null) { 
            _temporaryLinkVisual.PointerMoved -= ScrubBar_OnPointerMoved;
            _temporaryLinkVisual.PointerReleased -= ScrubBar_OnPointerReleased;
            }
            vm.Controller.Disposed -= ControllerOnDisposed;
        }

        public async void CheckBlocksForHit(double value)
        {
            foreach (var block in _timeBlocks)
            {
                if ((value >= block.StartTime && value <= block.EndTime) || (value <= block.StartTime && value >= block.EndTime))
                {
                    if (block.OnBlock == false)
                    {
                        block.OnBlock = true;
    
                        if (block.HasLinkedNode())
                        {
                            foreach (var element in block.NodeImageTuples)
                            {
                                ThumbnailGrid.Items.Remove(element.Item2);
                            }
                            await block.RefreshThumbnail();
                            foreach (var element in block.NodeImageTuples)
                            {
                                ThumbnailGrid.Items.Add(element.Item2);
                            }
                        }

                        //OnBlockHitEventHandler?.Invoke(element);
                    }
                }
                else if (block.OnBlock == true)
                {
                    block.OnBlock = false;
                    if (block.HasLinkedNode())
                    {
                        foreach (var element in block.NodeImageTuples)
                        {
                            ThumbnailGrid.Items.Remove(element.Item2);

                        }

                    }

                    Debug.WriteLine("block left");
                    //OnBlockLeaveEventHandler?.Invoke(element);
                }
            }
        }

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
            if (playbackElement.Source == null && _loaded == false)
            {
                InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();
                var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get((((VideoNodeViewModel)DataContext).Model as VideoNodeModel).LibraryId).Data);
                memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
                memoryStream.Seek(0);
                playbackElement.SetSource(memoryStream, "video/mp4");
                _loaded = true;
            }
            playbackElement.MediaEnded += PlaybackElementOnMediaEnded;





        }

        private void PlaybackElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            play.Opacity = 1;
        }

        private void PlaybackElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
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

        private void ScrubBar_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_addTimeBlockMode == true)
            {
                if (grid.Children.Contains(_temporaryLinkVisual))
                {
                    int xwithinscrub =
                        (int)(_temporaryLinkVisual.X1 - (Canvas.GetLeft(scrubBar) + scrubBar.Margin.Left));
                    int start = (int)((xwithinscrub / (scrubBar.ActualWidth)) * playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    int x2withinscrub = (int)(_temporaryLinkVisual.X2 - (Canvas.GetLeft(scrubBar) + scrubBar.Margin.Left));
                    int end = (int)((x2withinscrub / (scrubBar.ActualWidth)) * playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    CreateTimeBlock(new TimeSpan(0, 0, 0, 0, start), new TimeSpan(0, 0, 0, 0, end));
                    grid.Children.Remove(_temporaryLinkVisual);
                    ((UIElement)sender).ReleasePointerCapture(e.Pointer);


                }
            }
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
        private void SrubBar_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_addTimeBlockMode == false)
            {
                double ratio = e.GetPosition((UIElement)sender).X / scrubBar.ActualWidth;
                double millliseconds = playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
                playbackElement.Position = time;
            }
            else if (_addTimeBlockMode == true)
            {

            }
        }

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as VideoNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);
            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            Grid.SetRow(line, 1);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
            timeBlockVM.setUpHandlers(line.getLine());
        }
        private void ScrubBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            CheckBlocksForHit(playbackElement.Position.TotalMilliseconds);

        }

        private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            foreach (var element in _timeBlocks)
            {
                element.ResizeLine1();
            }
        }

        private void AddTimeBlock_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_addTimeBlockMode == false)
            {
                _addTimeBlockMode = true;
                scrubBar.SetValue(Canvas.ZIndexProperty, 0);
            }
            else
            {
                _addTimeBlockMode = false;
                scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            }
        }
        private void OnStop_Click(object sender, TappedRoutedEventArgs e)
        {
            /*     if (_recording)
                 {
                     ToggleRecording(CurrentAudioFile.Name);
                 }*/
            playbackElement.Stop();
            scrubBar.Value = 0;
            
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
            Binding b = new Binding();
            b.ElementName = "playbackElement";
            b.Path = new PropertyPath("Position.TotalMilliseconds");
            scrubBar.SetBinding(ProgressBar.ValueProperty, b);
            playbackElement.Play();
        }

        private void OnPause_Click(object sender, RoutedEventArgs e)
        {

            playbackElement.Pause();
            //    pause.Opacity = .3;
        }
    }
}
