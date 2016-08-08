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
using NusysIntermediate;
using Path = System.IO.Path;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioDetailHomeTabView : UserControl
    {
        private bool _stopped;

        // drag element variables
        private double _x;
        private double _y;
        private string _libraryElementId;

        public event ContentLoadedEventHandler ContentLoaded;
        public delegate void ContentLoadedEventHandler(object sender);

        public AudioMediaPlayer AudioMediaPlayer { get { return MediaPlayer; } }

        
        public AudioDetailHomeTabView(AudioDetailHomeTabViewModel vm)
        {
            this.DataContext = vm; // has to be set before initComponent so child xaml elements inherit it
            _libraryElementId = vm.LibraryElementController.ContentId;
            this.InitializeComponent();

            AudioMediaPlayer.AudioSource = new Uri(vm.LibraryElementController.Data);
            AudioMediaPlayer.MediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;

            vm.LibraryElementController.Disposed += ControllerOnDisposed;
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose();
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AudioDetailHomeTabViewModel;
            vm.Duration = AudioMediaPlayer.MediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            ContentLoaded?.Invoke(this);
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (AudioNodeViewModel) DataContext;
            MediaPlayer.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;   
        }


        public void Dispose()
        {
            MediaPlayer.Dispose();
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

            if (!SessionController.Instance.ContentController.ContainsContentDataModel(element.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(element.ContentDataModelId);
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
                    var collection = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId) as CollectionLibraryElementModel;
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, collection.IsFinite, 
                            new List<Point>(collection.ShapePoints.Select(p => new Point(p.X,p.Y))), size.Width, size.Height);
                }
            });
        }

        #endregion addToCollection
      
    }
}
