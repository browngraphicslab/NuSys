using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using NuSysApp.Components.Nodes;
using NuSysApp.Nodes.AudioNode;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioDetailView : UserControl
    {
        private bool _stopped;
        private bool _loaded;
        private bool _addTimeBlockMode;
        private Line _temporaryLinkVisual;
        private List<LinkedTimeBlockViewModel> _timeBlocks;

        public AudioDetailView(AudioNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _loaded = false;
            _addTimeBlockMode = false;
            (DataContext as AudioNodeViewModel).addTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);





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
                        Debug.WriteLine("block hit");
                        if (block.HasLinkedNode())
                        {
                            foreach (var element in block.NodeImageTuples)
                            {
                                ThumbnailGrid.Items.Remove(element.Item2);
                            }
                            await block.RefreshThumbnail();
                            foreach (var element in block.NodeImageTuples)
                            {
                                Debug.Write("fdsjakflds;");
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

        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as AudioNodeViewModel).LinkedTimeModels.Last(), playbackElement.NaturalDuration.TimeSpan, scrubBar);
            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
            timeBlockVM.setUpHandlers(line.getLine());
        }

        private void AddAllLinksVisually()
        {
            foreach (var element in (DataContext as AudioNodeViewModel).LinkedTimeModels)
            {
                var timeBlockVM = new LinkedTimeBlockViewModel(element, playbackElement.NaturalDuration.TimeSpan, scrubBar);
                LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
                _timeBlocks.Add(timeBlockVM);
                grid.Children.Add(line);
                timeBlockVM.setUpHandlers(line.getLine());
            }
            scrubBar.SizeChanged += ScrubBar_OnSizeChanged;

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

        public void CreateTimeBlock(TimeSpan start, TimeSpan end)
        {
            LinkedTimeBlockModel model = new LinkedTimeBlockModel(start, end);
            LinkedTimeBlockViewModel link = new LinkedTimeBlockViewModel(model, playbackElement.NaturalDuration.TimeSpan, scrubBar);

            (DataContext as AudioNodeViewModel).AddLinkTimeModel(model);

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

        private void PlaybackElement_Onloaded(object sender, RoutedEventArgs e)
        {
            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                Play.Opacity = 1;
            };
            _temporaryLinkVisual = new Line();
            _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
            _temporaryLinkVisual.StrokeThickness = VisualizationImage.ActualHeight;
            _temporaryLinkVisual.Y1 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.Y2 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
            _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
            _temporaryLinkVisual.Opacity = 1;




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

        private void Play_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (playbackElement.Source == null && _loaded == false)
            {
                playbackElement.SetSource((DataContext as AudioNodeViewModel).AudioSource, "audio/mp3");

                _loaded = true;
            }
            Play.Opacity = .3;
            Pause.Opacity = 1;
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                Play.Opacity = 1;
            };
            playbackElement.Play();
        }
        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Play.Opacity = 1;
            Pause.Opacity = 0.3;
            playbackElement.Pause();

        }
        private void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            playbackElement.Stop();
            _stopped = true;
            Play.Opacity = 1;
            Pause.Opacity = 1;
            e.Handled = true;
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

        private void PlaybackElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            this.AddAllLinksVisually();
        }
    }
}
