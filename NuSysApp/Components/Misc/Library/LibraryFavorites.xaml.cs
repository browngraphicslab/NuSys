using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryFavorites : UserControl
    {

        private LibraryElementPropertiesWindow _propertiesWindow;
        private double _x;
        private double _y;

        private LibraryView _library;
        public LibraryFavorites(LibraryView library, LibraryFavoritesViewModel vm, LibraryElementPropertiesWindow propertiesWindow)
        {

            this.DataContext = vm;
            this.InitializeComponent();
            _library = library;
            _propertiesWindow = propertiesWindow;

            vm.OnItemsChanged += ViewModel_OnItemsChanged;

            /*
            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                ((LibraryBucketViewModel)library.DataContext).OnNewContents += SetItems;
            };
            */




        }

        private void ViewModel_OnItemsChanged(object sender, bool favorited)
        {
            if (!favorited)
                _propertiesWindow.Visibility = Visibility.Collapsed;
        }

        public void SetItems(ICollection<LibraryElementModel> elements)
        {
            var col = ((LibraryFavoritesViewModel)DataContext).PageElements;
            col.Clear();
            foreach (var libraryElementModel in elements)
            {

                if (libraryElementModel.Favorited)
                {
                    col.Add(libraryElementModel);
                    
                }

               
            }
        }

        public async void Update()
        {
            this.SetItems(((LibraryFavoritesViewModel)this.DataContext).PageElements);
        }

        private void LibraryListItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X;
            _y = e.GetCurrentPoint(view).Position.Y;

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            element.FireLightupContent(true);
        }
        private void ListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            _propertiesWindow.SetElement(element);
            _propertiesWindow.Visibility = Visibility.Visible;

            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            element.FireLightupContent(true);
        }
        private void LibraryListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }




            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.Visibility = Visibility.Collapsed;
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.Width = 100;
            rect.Height = 100;
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x - (rect.Width / 2);
            t.TranslateY += _y - (rect.Height / 2);

            if (!SessionController.Instance.ContentController.ContainsAndLoaded(element.Id))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(element.Id);
                });
            }
        }


        private void LibraryListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((WaitingRoomView.InitialWorkspaceId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);

            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where(i => i is LibraryView);
            if (itemsBelow.Any())
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Visibility = Visibility.Collapsed;
            }
            else
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Visibility = Visibility.Visible;

            }
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            var t = (CompositeTransform)rect.RenderTransform;

            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            _x += e.Delta.Translation.X;
            _y += e.Delta.Translation.Y;

            _propertiesWindow.Visibility = Visibility.Collapsed;


        }

        private async void LibraryListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            LibraryElementModel element = (LibraryElementModel)((Grid)sender).DataContext;
            if ((WaitingRoomView.InitialWorkspaceId == element.Id) || (element.Type == ElementType.Link))
            {
                e.Handled = true;
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            rect.Width = 0;
            rect.Height = 0;

            if (SessionController.Instance.SessionView.LibraryDraggingRectangle.Visibility == Visibility.Collapsed)
                return;
            var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));
            await _library.AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type, element.Id);
        }

        private void ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            _propertiesWindow.SetElement(((LibraryElementModel)e.ClickedItem));
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
