using NuSysApp.Util;
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
using Windows.UI.Xaml.Navigation;
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfDetailHomeTabView : UserControl
    {
        //private InqCanvasView _inqCanvasView;

        private double _x;
        private double _y;
        private string _libraryElementId;

        public event ContentLoadedEventHandler ContentLoaded;
        public delegate void ContentLoadedEventHandler(object sender);


        public PdfDetailHomeTabView(PdfDetailHomeTabViewModel vm)
        {
            InitializeComponent();
            _libraryElementId = vm.Controller.ContentId;


            Canvas.SetZIndex(ItemsControl, 0);
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.PropertyChanged += PropertyChanged;
            vm.View = this;

            //vm.CreateRegionViews();
            DataContext = vm;
            xImg.ImageOpened += delegate
            {
                ContentLoaded?.Invoke(this);
            };
            Loaded += delegate
            {
                ContentLoaded?.Invoke(this);
            };
            //vm.MakeTagList();

            vm.Controller.Disposed += ControllerOnDisposed;
        }
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RegionViews":
                    break;
            }
        }

        private void XBorderOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //xBorder.Clip = new RectangleGeometry {Rect= new Rect(0,0,e.NewSize.Width, e.NewSize.Height)};
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (PdfDetailHomeTabViewModel)DataContext;
            vm.Controller.Disposed += ControllerOnDisposed;
            DataContext = null;
        }
        
        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipLeft();
            //(_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            //  nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //  nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfDetailHomeTabViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipRight();
            //(_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            // (_inqCanvasView.DataContext as InqCanvasViewModel).Lines.Clear();
            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //   nodeTpl.inkCanvas.ReRenderLines();
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (PdfNodeModel)((PdfNodeViewModel)DataContext).Model;
            var libraryElementController = (DataContext as PdfDetailHomeTabViewModel)?.Controller;
            string token = libraryElementController.GetMetadata("Token")?.ToString();
            await AccessList.OpenFile(token);
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            /*
            var compositeTransform = (CompositeTransform)xImg.RenderTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(e.Position);

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            
            
            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift


           compositeTransform.TranslateX += e.Delta.Translation.X;
           compositeTransform.TranslateY += e.Delta.Translation.Y;

            var minY = 0;
            var maxY = Math.Max(xBorder.ActualHeight - xImg.ActualHeight*compositeTransform.ScaleY, 0);
            compositeTransform.TranslateY = Math.Max(compositeTransform.TranslateY, minY);

            e.Handled = true;
            */
        }

        public double GetPdfHeight()
        {
            return xImg.ActualHeight;
        }

        public double GetPdfWidth()
        {
            return xImg.ActualWidth;
        }

        private void xImg_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = DataContext as PdfDetailHomeTabViewModel;
            foreach (var regionView in vm.RegionViews)
            {
                regionView.Deselect();
            }
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
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(_libraryElementId);
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
            LibraryElementModel element = SessionController.Instance.ContentController.GetContent(_libraryElementId);
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

    }
}