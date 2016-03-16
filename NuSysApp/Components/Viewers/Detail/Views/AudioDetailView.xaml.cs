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
using Path = System.IO.Path;

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
        private MediaElement _playbackElement ;

        public AudioDetailView(AudioNodeViewModel vm)
        {
            _playbackElement = ((AudioNodeModel)(vm.Model)).Controller.PlaybackElement;
            this.DataContext = vm;
            this.InitializeComponent();
            _loaded = false;
            _addTimeBlockMode = false;
            (DataContext as AudioNodeViewModel).addTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            scrubBar.SetValue(Canvas.ZIndexProperty, 1);
            ((AudioNodeModel)(vm.Model)).Controller.OnScrub += Controller_OnScrub;
            ((AudioNodeModel)(vm.Model)).Controller.OnPlay += Controller_OnPlay1;
            ((AudioNodeModel)(vm.Model)).Controller.OnPause += Controller_OnPause1;
            ((AudioNodeModel)(vm.Model)).Controller.OnStop += Controller_OnStop1;

            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
            _temporaryLinkVisual = new Line();
            _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
            _temporaryLinkVisual.StrokeThickness = VisualizationImage.ActualHeight;
            _temporaryLinkVisual.Y1 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.Y2 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
            _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
            _temporaryLinkVisual.Opacity = 1;

            this.AddAllLinksVisually();


        }

        private void Controller_OnStop1(MediaElement playbackElement)
        {
            _stopped = true;
            Play.Opacity = 1;
            Pause.Opacity = 1;
        }

        private void Controller_OnPause1(MediaElement playbackElement)
        {
            Play.Opacity = 1;
            Pause.Opacity = 0.3;
        }

        private void Controller_OnPlay1(MediaElement playbackElement)
        {
            Play.Opacity = .3;
            Pause.Opacity = 1;
            playbackElement.MediaEnded += delegate (object o, RoutedEventArgs e2)
            {
                Play.Opacity = 1;
            };
        }

        private void Controller_OnScrub(MediaElement playbackElement)
        {
            scrubBar.Value = scrubBar.Maximum*
                             (playbackElement.Position.TotalMilliseconds/
                              playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
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
            var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as AudioNodeViewModel).LinkedTimeModels.Last(), ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
            LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
            timeBlockVM.setUpHandlers(line.getLine());
        }

        private void AddAllLinksVisually()
        {
            foreach (var element in (DataContext as AudioNodeViewModel).LinkedTimeModels)
            {
                var timeBlockVM = new LinkedTimeBlockViewModel(element, ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
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
                double millliseconds = ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

                TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)millliseconds);
                ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Scrub(time);

            }
        }

        public void CreateTimeBlock(TimeSpan start, TimeSpan end)
        {
            LinkedTimeBlockModel model = new LinkedTimeBlockModel(start, end);
            LinkedTimeBlockViewModel link = new LinkedTimeBlockViewModel(model, ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);

            (DataContext as AudioNodeViewModel).AddLinkTimeModel(model);

        }

        private void ScrubBar_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {

            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed)
            {
                if (_addTimeBlockMode == false)
                {
                    double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                    double seconds = ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalSeconds * ratio;

                    TimeSpan time = new TimeSpan(0, 0, (int)seconds);
                    ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Scrub(time);

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

        private void ScrubBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            CheckBlocksForHit(((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.Position.TotalMilliseconds);

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
            Play.Opacity = .3;
            Pause.Opacity = 1;
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Play();

        }
        private void Pause_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Play.Opacity = 1;
            Pause.Opacity = 0.3;
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Pause();


        }
        private void Stop_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _stopped = true;
            Play.Opacity = 1;
            Pause.Opacity = 1;
            e.Handled = true;
            ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Stop();

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
                    if (xwithinscrub<0)
                    {
                        xwithinscrub = 0;
                    }
                    else if (xwithinscrub > Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth)
                    {
                        xwithinscrub = (int)(Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth);
                    }
                    int start = (int)((xwithinscrub / (scrubBar.ActualWidth)) * ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    int x2withinscrub = (int)(_temporaryLinkVisual.X2 - (Canvas.GetLeft(scrubBar) + scrubBar.Margin.Left));
                    if (x2withinscrub < 0)
                    {
                        x2withinscrub = 0;
                    }
                    else if (x2withinscrub > Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth)
                    {
                        x2withinscrub = (int)(Canvas.GetLeft(scrubBar) + scrubBar.ActualWidth);
                    }
                    int end = (int)((x2withinscrub / (scrubBar.ActualWidth)) * ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
                    CreateTimeBlock(new TimeSpan(0, 0, 0, 0, start), new TimeSpan(0, 0, 0, 0, end));
                    grid.Children.Remove(_temporaryLinkVisual);
                    ((UIElement)sender).ReleasePointerCapture(e.Pointer);


                }
            }
        }
        
    }
}
