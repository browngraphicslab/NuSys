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
using System.Threading.Tasks;
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class VideoDetailHomeTabView : UserControl
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private bool _addTimeBlockMode;
        private bool _loaded;
        private Line _temporaryLinkVisual;
        private List<LinkedTimeBlockViewModel> _timeBlocks;


        private double _x;
        private double _y;
        private string _libraryElementId;


        public event ContentLoadedEventHandler ContentLoaded;
        public delegate void ContentLoadedEventHandler(object sender);


        public VideoDetailHomeTabView(VideoDetailHomeTabViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;
            _libraryElementId = vm.LibraryElementController.ContentId;

            VideoMediaPlayer.Source = vm.LibraryElementController.GetSource();
            VideoMediaPlayer.MediaPlayer.MediaOpened += vm.VideoMediaPlayer_Loaded;

            _isRecording = false;
            //vm.LinkedTimeModels.CollectionChanged += LinkedTimeBlocks_CollectionChanged;
            _timeBlocks = new List<LinkedTimeBlockViewModel>();
            
            vm.LibraryElementController.Disposed += ControllerOnDisposed;
            vm.View = this;
            VideoMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            
            VideoMediaPlayer.ScrubBar.ValueChanged += vm.ScrubBarOnValueChanged;
            vm.OnRegionSeekPassing += VideoMediaPlayer.onSeekedTo;
            //Loaded += delegate (object sender, RoutedEventArgs args)
            //{
            //    var sw = SessionController.Instance.SessionView.ActualWidth / 2;
            //    var sh = SessionController.Instance.SessionView.ActualHeight / 2;

            //    var ratio = playbackElement.ActualWidth > playbackElement.ActualHeight ? playbackElement.ActualWidth / sw : playbackElement.ActualHeight / sh;
            //    playbackElement.Width = playbackElement.ActualWidth / ratio;
            //    playbackElement.Height = playbackElement.ActualHeight / ratio;
            //};
        }

        private async void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            vm.VideoDuration = VideoMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            //HACKY. DELETE AFTER DEMO
            await Task.Delay(500);
            vm.SetExistingRegions();
            ContentLoaded?.Invoke(this);
            
        }

        public void Dispose()
        {
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (VideoNodeViewModel)DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
        }
        
        private void MediaPlayer_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as VideoDetailHomeTabViewModel;
            //foreach (var regionView in vm.RegionViews)
            //{
              //regionView.Deselect();
            //}
        }
        
        public double VideoWidth => VideoMediaPlayer.MediaPlayer.ActualWidth;
        public double VideoHeight => VideoMediaPlayer.MediaPlayer.ActualHeight;
        public void StopVideo()
        {
            VideoMediaPlayer.StopVideo();
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
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((SessionController.Instance.ActiveFreeFormViewer.LibraryElementId == element?.LibraryElementId) ||
                (element?.Type == NusysConstants.ElementType.Link))
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
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
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
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
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

        public async Task AddNode(Point pos, Size size, NusysConstants.ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != NusysConstants.ElementType.Collection)
                {
                    var element = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId);
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
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
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

    }
}
