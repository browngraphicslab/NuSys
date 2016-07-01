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
        public string cID;
        private bool pinned;
        public LinkCircle(string cID)
        {
            this.cID = cID;
            this.InitializeComponent();
            pinned = false;
            //this.thumbnail = SessionController.Instance.ContentController.GetContent(cID).T
            var s = SessionController.Instance.ContentController.GetLibraryElementController(cID);
            var bmp = new BitmapImage(SessionController.Instance.ContentController.GetLibraryElementController(cID).LargeIconUri);
            thumbnail.Source = bmp;
            (thumbnail.RenderTransform as CompositeTransform).TranslateY = -bmp.DecodePixelHeight - 20;
            
        }

        private async void circlePointerPressedHandler(object sender, RoutedEventArgs e)
        {
            pinned = !pinned;
            if (pinned)
            {
                thumbnail.Visibility = Visibility.Visible;
                //positiion thumbnail
            }
            else
            {
                thumbnail.Visibility = Visibility.Collapsed;
            }
        }

        private async void circlePointerEnteredHandler(object sender, RoutedEventArgs e)
        {
            thumbnail.Visibility = Visibility.Visible;
            //position thumbnail
        }

        private async void circlePointerExitedHandler(object sender, RoutedEventArgs e)
        {
            if (!pinned)
            {
                thumbnail.Visibility = Visibility.Collapsed;
            }
        }

        public Ellipse Circle
        {
            get { return linkButton; }
        }
    }
}
