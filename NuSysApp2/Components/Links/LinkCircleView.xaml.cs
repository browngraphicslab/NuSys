using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace NuSysApp2
{
    public sealed partial class LinkCircle : UserControl
    {
        //link id
        public string lID;
        //content id the link is linked to
        public string cID;
        private bool pinned;
        private Thickness _collapsedThickness;
        private Thickness _visibleThickness;
        private bool _firstTimeOpened;
        private BitmapImage _bmp;
        public LinkCircle(string lID, string cID)
        {
            this.lID = lID;
            this.cID = cID;
            //represents if the image has been loaded before
            _firstTimeOpened = false;
            //thickness to make border visible/invisible
            _collapsedThickness = new Thickness(0);
            _visibleThickness = new Thickness(1);

            this.InitializeComponent();
            //border starts off invisible
            border.BorderThickness = _collapsedThickness;
            //thumbnail is not pinned to begin with
            pinned = false;
            _bmp = new BitmapImage(SessionController.Instance.ContentController.GetLibraryElementController(cID).SmallIconUri);
            thumbnail.ImageOpened += Thumbnail_ImageOpened;
            //centering the thumbnail
            (border.RenderTransform as CompositeTransform).TranslateX -= 10;
            thumbnail.Source = _bmp;
            //this is sort of a bandaid rather than a fix
            Canvas.SetZIndex(thumbnail, 50);
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

        //pins or unpins the thumbnail
        private async void circlePointerPressedHandler(object sender, RoutedEventArgs e)
        {
            pinned = !pinned;
            if (pinned)
            {
                thumbnail.Visibility = Visibility.Visible;
                border.BorderThickness = _visibleThickness;
            }
            else
            {
                border.BorderThickness = _collapsedThickness;
                thumbnail.Visibility = Visibility.Collapsed;
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
            if (!pinned)
            {
                border.BorderThickness = _collapsedThickness;
                thumbnail.Visibility = Visibility.Collapsed;
            }
        }

        public Ellipse Circle
        {
            get { return linkButton; }
        }
    }
}
