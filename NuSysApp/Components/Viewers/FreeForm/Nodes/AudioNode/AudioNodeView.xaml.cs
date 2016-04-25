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
        private bool _addTimeBlockMode;
        private Line _temporaryLinkVisual;



        public AudioNodeView(AudioNodeViewModel vm)
        {
            InitializeComponent();
            this.DataContext = vm;
            _addTimeBlockMode = false;
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

            _temporaryLinkVisual = new Line();
            _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
            _temporaryLinkVisual.StrokeThickness = VisualizationImage.ActualHeight;
            _temporaryLinkVisual.Y1 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.Y2 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
            _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
            _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
            _temporaryLinkVisual.Opacity = 1;

            vm.Controller.Disposed += ControllerOnDisposed;

            ((AudioNodeModel)vm.Model).OnJump += AudioNodeView_OnJump;

            playbackElement.MediaEnded += MediaEnded;
        }

        private void MediaEnded(object sender, RoutedEventArgs e)
        {
            AudioNodeView_OnJump(new TimeSpan(0));
        }

        public void AudioNodeView_OnJump(TimeSpan time)
        {
            ((AudioNodeModel) ((DataContext as AudioNodeViewModel).Model)).Controller.ScrubJump(time);
        }

        private void AddAllLinksVisually()
        {
            foreach (var element in (DataContext as AudioNodeViewModel).LinkedTimeModels)
            {
                var timeBlockVM = new LinkedTimeBlockViewModel(element, ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
                LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
                line.SetValue(Canvas.ZIndexProperty, 1);
                line.OnTimeChange += ReSaveLinkModels;
                _timeBlocks.Add(timeBlockVM);
                grid.Children.Add(line);
                timeBlockVM.setUpHandlers(line.getLine());
            }
            scrubBar.SizeChanged += ScrubBar_OnSizeChanged;
            

        }

        private void PlaybackElement_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (AudioNodeViewModel)DataContext;
            vm.PropertyChanged -= Node_SelectionChanged;
            (DataContext as AudioNodeViewModel).removeTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeBlocks = null;

            if (((AudioNodeModel)(vm.Model)).Controller != null) { 
                ((AudioNodeModel)(vm.Model)).Controller.OnPlay -= Controller_OnPlay;
                ((AudioNodeModel)(vm.Model)).Controller.OnPause -= Controller_OnPause;
                ((AudioNodeModel)(vm.Model)).Controller.OnStop -= Controller_OnStop;
                ((AudioNodeModel)(vm.Model)).Controller = null;
            }
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
            line.SetValue(Canvas.ZIndexProperty, 1);
            line.OnTimeChange += ReSaveLinkModels;

            _timeBlocks.Add(timeBlockVM);
            grid.Children.Add(line);
            (DataContext as ElementViewModel).Controller.SaveTimeBlock();
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

        private async Task RenderImageSource(Grid RenderedGrid)
        {

            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            double x = grid.Height;
            grid.Width = RenderedGrid.Width * 2;
            try
            {
                await renderTargetBitmap.RenderAsync(RenderedGrid, 1000, 100);
            }
            catch(Exception e)
            {
                return;
            }
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
            if (LinkedTimeBlock._box1 != null && (LinkedTimeBlock._box1.FocusState == FocusState.Unfocused))
            {
                LinkedTimeBlock.removeBox();
            }
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
                if (_addTimeBlockMode == false)
                {
                    double ratio = e.GetCurrentPoint((UIElement)sender).Position.X / scrubBar.ActualWidth;
                    double milliseconds = ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalMilliseconds * ratio;

                    TimeSpan time = new TimeSpan(0, 0, 0, 0, (int)milliseconds);
                    ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.ScrubJump(time);

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
            if ((DataContext as AudioNodeViewModel).VisualGrid != null)
            {
                LoadPlaybackElement();
            }
            else
            {
                (DataContext as AudioNodeViewModel).OnVisualizationLoaded += LoadPlaybackElement;
            }
        }
        private async void LoadPlaybackElement()
        {
            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            await RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
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
            if (DataContext != null) // CPTTE fix
                ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.Scrub();
        }

        private void Region_OnClick(object sender, RoutedEventArgs e)
        {

            this.CreateTimeBlock(playbackElement.Position, new TimeSpan(0,0,0,0,(int)playbackElement.Position.TotalMilliseconds + (int)(playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds - playbackElement.Position.TotalMilliseconds)/4));

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

        public void CreateTimeBlock(TimeSpan start, TimeSpan end)
        {
            LinkedTimeBlockModel model = new LinkedTimeBlockModel(start, end);
            LinkedTimeBlockViewModel link = new LinkedTimeBlockViewModel(model, ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
            (DataContext as AudioNodeViewModel).AddLinkTimeModel(model);

        }

        public void ReSaveLinkModels()
        {
            (DataContext as ElementViewModel).Controller.SaveTimeBlock();

        }

        private void ScrubBar_OnLoaded(object sender, RoutedEventArgs e)
        {
            //this.AddAllLinksVisually();
        }

        private void PlaybackElement_OnMediaOpened(object sender, RoutedEventArgs e)
        {
            this.AddAllLinksVisually();
        }

        private void Grid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //if ((LinkedTimeBlock._box1.FocusState == FocusState.Unfocused))
            //{
                //LinkedTimeBlock.removeBox();
            //}
            
        }
    }
}
