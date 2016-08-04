using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using Windows.UI.Xaml.Navigation;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfDetailHomeTabView : UserControl
    {

        private double _x;
        private double _y;
        private string _libraryElementId;

        public PdfDetailHomeTabView(PdfDetailHomeTabViewModel vm)
        {
            InitializeComponent();
            _libraryElementId = vm.LibraryElementController.ContentId;

            vm.LibraryElementController.Disposed += ControllerOnDisposed;

            // disable page left and page right buttons for pdf regions
            if (vm.LibraryElementController.LibraryElementModel.Type == ElementType.PdfRegion)
            {
                pageLeft.Visibility = Visibility.Collapsed;
                pageRight.Visibility = Visibility.Collapsed;
            }

            DataContext = vm;
            vm.PageLocationChanged += Vm_PageLocationChanged;
            Loaded += PdfDetailHomeTabView_Loaded;

            xClippingWrapper.Controller = vm.LibraryElementController;
            xClippingWrapper.ProcessLibraryElementController();

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed; 
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            Dispose(); 
        }

        private void Vm_PageLocationChanged(object sender, int pageLocation)
        {
            UpdateRegionViews(pageLocation);
        }

        private async void PdfDetailHomeTabView_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as PdfDetailHomeTabViewModel;
            xClippingWrapper.Controller = vm.LibraryElementController;
            await xClippingWrapper.ProcessLibraryElementController();
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }
        
        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipLeft();
        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipRight();
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

        private async void UpdateRegionViews(int currentPageNumber)
        {
            foreach (var item in xClippingWrapper.GetRegionItems())
            {
                var regionView = item as PDFRegionView;
                var model = (regionView?.DataContext as PdfRegionViewModel)?.Model as PdfRegionModel;
                Debug.Assert(regionView != null);
                await UITask.Run(() =>
                {
                    regionView.Visibility = model?.PageLocation == currentPageNumber ? Visibility.Visible : Visibility.Collapsed;
                });
            }
        }

        public void Dispose()
        {
            var vm = DataContext as PdfDetailHomeTabViewModel;
            if (vm != null) // because delete library element request can remove the view model outside of this
            {
                vm.PageLocationChanged -= Vm_PageLocationChanged;
            }

            xClippingWrapper.Dispose();
        }

        #endregion addToCollection

    }
}