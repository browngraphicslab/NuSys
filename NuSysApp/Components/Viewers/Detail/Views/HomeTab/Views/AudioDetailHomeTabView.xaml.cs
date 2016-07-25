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
using Path = System.IO.Path;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioDetailHomeTabView : UserControl
    {
        private bool _stopped;
        private bool _loaded;
        private bool _addTimeBlockMode;
        private Line _temporaryLinkVisual;
        private List<AudioRegionViewModel> _timeRegions;


        private double _x;
        private double _y;
        private string _libraryElementId;

        public event ContentLoadedEventHandler ContentLoaded;
        public delegate void ContentLoadedEventHandler(object sender);

        public AudioMediaPlayer AudioMediaPlayer { get { return MediaPlayer; } }

        
        public AudioDetailHomeTabView(AudioDetailHomeTabViewModel vm)
        {
            this.DataContext = vm;
            _libraryElementId = vm.Controller.ContentId;
            this.InitializeComponent();
            _loaded = false;
            _addTimeBlockMode = false;
            //(DataContext as AudioNodeViewModel).addTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
            _timeRegions = new List<AudioRegionViewModel>();
            //scrubBar.SetValue(Canvas.ZIndexProperty, 1);

            AudioMediaPlayer.AudioSource = vm.Controller.GetSource();
            AudioMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            AudioMediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;




            //((AudioNodeModel)(vm.Model)).Controller.OnScrub += ControllerOnScrub;
            //((AudioNodeModel)(vm.Model)).Controller.OnPlay += Controller_OnPlay1;
            //((AudioNodeModel)(vm.Model)).Controller.OnPause += Controller_OnPause1;
            //((AudioNodeModel)(vm.Model)).Controller.OnStop += Controller_OnStop1;
            //scrubBar.Maximum = ((AudioNodeModel)(vm.Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            //scrubBar.Loaded += ScrubBarOnLoaded;
            //((AudioNodeModel)(vm.Model)).Controller.Scrub();

            //(DataContext as AudioNodeViewModel).OnVisualizationLoaded += LoadPlaybackElement;



            /* _temporaryLinkVisual = new Line();
             _temporaryLinkVisual.Stroke = new SolidColorBrush(Colors.Aqua);
             _temporaryLinkVisual.StrokeThickness = VisualizationImage.ActualHeight;
             _temporaryLinkVisual.Y1 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
             _temporaryLinkVisual.Y2 = Canvas.GetTop(VisualizationImage) + VisualizationImage.ActualHeight / 2 + VisualizationImage.Margin.Top;
             _temporaryLinkVisual.PointerMoved += ScrubBar_OnPointerMoved;
             _temporaryLinkVisual.PointerReleased += ScrubBar_OnPointerReleased;
             _temporaryLinkVisual.Opacity = 1;
 */
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.OnRegionSeekPassing += MediaPlayer.onSeekedTo;
            vm.View = this;
            

        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AudioDetailHomeTabViewModel;
            vm.Duration = AudioMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            vm.SetExistingRegions();

            ContentLoaded?.Invoke(this);
        }

        public void StopAudio()
        {
            MediaPlayer.StopMusic();
        }

        public void DisplayRegion(Region region)
        {
//            var rectangleRegion = (TimeRegionModel)region;
//
//            var displayedRegion = new AudioRegionView(new AudioRegionViewModel(rectangleRegion,this));
//            displayedRegion.OnSelected += DisplayedRegion_OnSelected;
//            DisplayedRegion_OnSelected(displayedRegion, true);
//            (this.DataContext as AudioDetailHomeTabViewModel).RegionAdded(rectangleRegion,this);
//            (this.DataContext as AudioDetailHomeTabViewModel).Controller.AddRegion(rectangleRegion);
        }
        private void DisplayedRegion_OnSelected(object sender, bool selected)
        {
            //            SelectedRegion?.Deselected();
            //           SelectedRegion = (ImageRegionView)sender;
            //         SelectedRegion.Selected();

        }
        private void AudioPlayer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as AudioDetailHomeTabViewModel;
            /*
            foreach (var regionView in vm.RegionViews)
            {
                regionView.Deselect();
            }
            */
        }
        private void ControllerOnDisposed(object source, object args)
        {

            var vm = (AudioNodeViewModel) DataContext;
            MediaPlayer.StopMusic();
            //scrubBar.ContainerSizeChanged -= ScrubBar_OnSizeChanged;
            vm.Controller.Disposed -= ControllerOnDisposed;

            /*if (((AudioNodeModel)(vm.Model)).Controller != null) { 
                ((AudioNodeModel)(vm.Model)).Controller.OnScrub -= ControllerOnScrub;
                ((AudioNodeModel)(vm.Model)).Controller.OnPlay -= Controller_OnPlay1;
                ((AudioNodeModel)(vm.Model)).Controller.OnPause -= Controller_OnPause1;
                ((AudioNodeModel)(vm.Model)).Controller.OnStop -= Controller_OnStop1;
                scrubBar.Loaded -= ScrubBarOnLoaded;
                ((AudioNodeModel)(vm.Model)).Controller.Scrub();
            }*/
            

            //(DataContext as AudioNodeViewModel).OnVisualizationLoaded -= LoadPlaybackElement;
            //(DataContext as AudioNodeViewModel).removeTimeBlockChange(LinkedTimeBlocks_CollectionChanged);
     
        }


        public void Dispose()
        {
        }

        #region addToCollection
        private void AddToCollection_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X - 25;
            _y = e.GetCurrentPoint(view).Position.Y - 25;
        }

        private void AddToCollection_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(_libraryElementId);
            if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element?.LibraryElementId) ||
                (element?.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(element);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x;
            t.TranslateY += _y;

            if (!SessionController.Instance.ContentController.ContainsAndLoaded(element.LibraryElementId))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(element.LibraryElementId);
                });
            }

        }



        private void AddToCollection_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(_libraryElementId);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);

            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where(i => i is LibraryView);
            if (itemsBelow.Any())
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Hide();
            }
            else
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Show();

            }
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            var t = (CompositeTransform)rect.RenderTransform;

            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            _x += e.Delta.Translation.X;
            _y += e.Delta.Translation.Y;

        }

        private async void AddToCollection_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(_libraryElementId);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;


            if (rect.Visibility == Visibility.Collapsed)
                return;

            rect.Hide();
            var r =
                SessionController.Instance.SessionView.MainCanvas.TransformToVisual(
                    SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));

            if (_x > SessionController.Instance.SessionView.MainCanvas.ActualWidth - SessionController.Instance.SessionView.DetailViewerView.ActualWidth) return;

            await AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type, element.LibraryElementId);
        }

        public async Task AddNode(Point pos, Size size, ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != ElementType.Collection)
                {
                    var element = SessionController.Instance.ContentController.GetContent(libraryId);
                    var dict = new Message();
                    Dictionary<string, object> metadata;

                    metadata = new Dictionary<string, object>();
                    metadata["node_creation_date"] = DateTime.Now;
                    metadata["node_type"] = elementType + "Node";

                    dict = new Message();
                    dict["title"] = element?.Title + " element";
                    dict["width"] = size.Width.ToString();
                    dict["height"] = size.Height.ToString();
                    dict["type"] = elementType.ToString();
                    dict["x"] = pos.X;
                    dict["y"] = pos.Y;
                    dict["contentId"] = libraryId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                }
                else
                {
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, size.Width,
                            size.Height);
                }
            });
        }

        #endregion addToCollection


        /*private void LoadPlaybackElement()
        {
            grid.Children.Add((DataContext as AudioNodeViewModel).VisualGrid);
            //RenderImageSource((DataContext as AudioNodeViewModel).VisualGrid);
        }*/

        /*private void ScrubBarOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.AddAllLinksVisually();
            this.CheckBlocksForHit(scrubBar.Value);
        }*/
        /*
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
            playbackElement.MediaEnded += PlaybackElementOnMediaEnded;
        }

        private void PlaybackElementOnMediaEnded(object sender, RoutedEventArgs routedEventArgs)
        {
            Play.Opacity = 1;
        }
        
        private void ControllerOnScrub(MediaElement playbackElement)
        {
            if (!playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds.Equals(0))
            {
                scrubBar.Value = scrubBar.Maximum *
                             (playbackElement.Position.TotalMilliseconds /
                              playbackElement.NaturalDuration.TimeSpan.TotalMilliseconds);
            }
            
        }
/*
        public async void CheckBlocksForHit(double value)
        {
            double time = value/scrubBar.Maximum*
                          ((AudioNodeModel) ((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement
                              .NaturalDuration.TimeSpan.TotalMilliseconds;
            foreach (var block in _timeRegions)
            {
                if ((time >= block.StartTime && time <= block.EndTime) || (time <= block.StartTime && time >= block.EndTime))
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
                    //OnBlockLeaveEventHandler?.Invoke(element);
                }
            }
        }
*/
        /*        private void LinkedTimeBlocks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    var timeBlockVM = new LinkedTimeBlockViewModel((DataContext as AudioNodeViewModel).LinkedTimeModels.Last(), ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
                    LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
                    _timeBlocks.Add(timeBlockVM);
                    grid.Children.Add(line);
                    timeBlockVM.setUpHandlers(line.getLine());

                    AudioRegionViewModel vm = new AudioRegionViewModel((TimeRegionModel)(DataContext as AudioNodeViewModel).Controller.LibraryElementModel.Regions.Last(), scrubBar);
                    AudioRegionView region = new AudioRegionView(vm);
                    _timeRegions.Add(vm);
                    grid.Children.Add(region);
                    vm.setUpHandlers(region.getLine());

                }

                private void AddAllLinksVisually()
                {
                    foreach (var element in (DataContext as AudioNodeViewModel).LinkedTimeModels)
                    {
                        var timeBlockVM = new LinkedTimeBlockViewModel(element, ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.PlaybackElement.NaturalDuration.TimeSpan, scrubBar);
                        LinkedTimeBlock line = new LinkedTimeBlock(timeBlockVM);
                        _timeRegions.Add(timeBlockVM);
                        grid.Children.Add(line);
                        timeBlockVM.setUpHandlers(line.getLine());
                    }
                    scrubBar.ContainerSizeChanged += ScrubBar_OnSizeChanged;

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
                        ((AudioNodeModel)((DataContext as AudioNodeViewModel).Model)).Controller.ScrubJump(time);

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

                private void ScrubBar_OnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
                {
                  //   CheckBlocksForHit(scrubBar.Value);

                }

            /*    private void ScrubBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
                {

                    foreach (var element in _timeRegions)
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
                */
    }
}
