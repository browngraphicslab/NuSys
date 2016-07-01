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

namespace NuSysApp
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
        public LinkCircle(string lID, string cID)
        {
            this.lID = lID;
            this.cID = cID;
            _collapsedThickness = new Thickness(0);
            _visibleThickness = new Thickness(1);
            this.InitializeComponent();
            pinned = false;
            var bmp = new BitmapImage(SessionController.Instance.ContentController.GetLibraryElementController(cID).SmallIconUri);
            thumbnail.Source = bmp;
            Canvas.SetZIndex(thumbnail, 50);
            border.Height = bmp.DecodePixelHeight;
            border.Width = bmp.DecodePixelWidth;
            (thumbnail.RenderTransform as CompositeTransform).TranslateY = -bmp.DecodePixelHeight - 20;
        }

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

        private async void circlePointerEnteredHandler(object sender, RoutedEventArgs e)
        {
            thumbnail.Visibility = Visibility.Visible;
        }

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
