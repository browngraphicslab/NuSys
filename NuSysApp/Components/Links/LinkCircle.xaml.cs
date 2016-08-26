﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LinkCircle : UserControl
    {
        //link id
        public string LinkLibraryElementId;
        //content id the link is linked to
        public string LibraryId;
        private LibraryElementController _linkLibraryElementController;
        private LibraryElementController _libraryElementController;
        private bool _pinned;

        protected bool Pinned
        {
            get { return _pinned; }
            set
            {
                _pinned = value;
                xPinHighlight.Visibility = _pinned == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        private Thickness _collapsedThickness;
        private Thickness _visibleThickness;
        private bool _firstTimeOpened;
        private BitmapImage _bmp;
        private bool _doubleTap;

        public event EventHandler Disposed;

        /// <summary>
        /// constructor for link circle.  takes in link-id, content Id
        /// </summary>
        /// <param name="linkLibraryElementId"></param>
        /// <param name="libraryId"></param>
        public LinkCircle(string linkLibraryElementId, string libraryId)
        {
            this.LinkLibraryElementId = linkLibraryElementId;
            this.LibraryId = libraryId;
            //represents if the image has been loaded before
            _firstTimeOpened = false;
            //thickness to make border visible/invisible
            _collapsedThickness = new Thickness(0);
            _visibleThickness = new Thickness(1);

            this.InitializeComponent();
            //border starts off invisible
            border.BorderThickness = _collapsedThickness;
            //thumbnail is not pinned to begin with
            Pinned = false;

            // The controller of the element the link goes to
            _libraryElementController =
                SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            // The controller of the link itself
            _linkLibraryElementController =
                SessionController.Instance.ContentController.GetLibraryElementController(linkLibraryElementId);
            _linkLibraryElementController.Disposed += Controller_Disposed;
            if (_libraryElementController != null)
            {
                _bmp = new BitmapImage(_libraryElementController?.SmallIconUri);

                _libraryElementController.LinkRemoved += LibraryElementController_LinkRemoved;

                thumbnail.ImageOpened += Thumbnail_ImageOpened;

                //centering the thumbnail
                (border.RenderTransform as CompositeTransform).TranslateX -= 10;
                thumbnail.Source = _bmp;
                //this is sort of a bandaid rather than a fix
                Canvas.SetZIndex(thumbnail, 50);
            }
            linkButton.Fill = new SolidColorBrush(MediaUtil.GetHashColorFromString(_libraryElementController?.LibraryElementModel?.LibraryElementId));
        }

        private void Controller_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        private void LibraryElementController_LinkRemoved(object sender, string e)
        {
            Disposed?.Invoke(this, EventArgs.Empty);
            foreach (var elementController in SessionController.Instance.LinksController.GetInstancesOfLibraryElement(LibraryId)
            {
                elementController.UpdateCircleLinks();
            }
        }

        private void Thumbnail_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (!_firstTimeOpened)
            {
                _firstTimeOpened = true;
                double toTransY = (50 * _bmp.PixelHeight / _bmp.PixelWidth) + 5;
                (border.RenderTransform as CompositeTransform).TranslateY -= toTransY;

            }
        }

        //makes thumbnail visible while pointer is hovering over the circle
        private async void circlePointerEnteredHandler(object sender, RoutedEventArgs e)
        {
            thumbnail.Visibility = Visibility.Visible;
            border.BorderThickness = _visibleThickness;
        }

        //makes thumbnail invisible if it is not pinned when the pointer leaves the circle
        private async void circlePointerExitedHandler(object sender, RoutedEventArgs e)
        {
            if (!Pinned)
            {
                border.BorderThickness = _collapsedThickness;
                thumbnail.Visibility = Visibility.Collapsed;
            }
        }

        public Ellipse Circle
        {
            get { return linkButton; }
        }

        private void Thumbnail_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (Pinned)
            {
                thumbnail.Visibility = Visibility.Collapsed;
                border.BorderThickness = _collapsedThickness;
                Pinned = !Pinned;
            }

        }

        // pins or unpins the thumbnails
        private async void LinkButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _doubleTap = false;
            await Task.Delay(200);
            if (_doubleTap)
            {
                return;
            }
            Pinned = !Pinned;
            if (Pinned)
            {
                thumbnail.Visibility = Visibility.Visible;
                border.BorderThickness = _visibleThickness;
            }
            else
            {
                border.BorderThickness = _collapsedThickness;
                thumbnail.Visibility = Visibility.Collapsed;
            }

            //If links to a region....
            var regionController = SessionController.Instance.ContentController.GetLibraryElementController(LinkLibraryElementId) as RegionLibraryElementController;
            if (regionController != null)
            {
             //   regionController.Select();
            }
        }

        /// <summary>
        /// Shows the detail view of the linked element when the circle link is doubled tapped on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkButton_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doubleTap = true;
            var libraryElementController =
                SessionController.Instance.ContentController.GetLibraryElementController(LibraryId);
            Debug.Assert(libraryElementController != null);

            // don't show the detail view in readonly mode
            if (!SessionController.Instance.SessionView.IsReadonly)
            {
                SessionController.Instance.SessionView.ShowDetailView(libraryElementController);
            }
            e.Handled = true;
        }

        private void Dispose()
        {
            _libraryElementController.LinkRemoved -= LibraryElementController_LinkRemoved;
            thumbnail.ImageOpened -= Thumbnail_ImageOpened;
            _linkLibraryElementController.Disposed -= Controller_Disposed;
            _linkLibraryElementController = null;
            _libraryElementController = null;
        }
    }
}
