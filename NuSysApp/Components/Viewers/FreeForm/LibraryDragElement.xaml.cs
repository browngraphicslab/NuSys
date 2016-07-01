using System;
using System.Collections.Generic;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryDragElement : UserControl
    {
        public LibraryDragElement()
        {
            this.InitializeComponent();
        }

        public void Show()
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }

        public void SwitchType(ElementType type)
        {
            audio.Visibility = Visibility.Collapsed;
            image.Visibility = Visibility.Collapsed;
            video.Visibility = Visibility.Collapsed;
            pdf.Visibility = Visibility.Collapsed;
            text.Visibility = Visibility.Collapsed;
            collection.Visibility = Visibility.Collapsed;
            IconImage.Visibility = Visibility.Collapsed;

            switch (type)
            {
                case ElementType.Text:
                    text.Visibility = Visibility.Visible;
                    break;
                case ElementType.Image:
                    image.Visibility = Visibility.Visible;
                    break;
                case ElementType.Collection:
                    collection.Visibility = Visibility.Visible;
                    break;
                case ElementType.PDF:
                    pdf.Visibility = Visibility.Visible;
                    break;
                case ElementType.Audio:
                    audio.Visibility = Visibility.Visible;
                    break;
                case ElementType.Video:
                    video.Visibility = Visibility.Visible;
                    break;
            }
        }

        // sets the drag icon to an image
        public void SetIcon(LibraryElementModel element)
        {
            audio.Visibility = Visibility.Collapsed;
            image.Visibility = Visibility.Collapsed;
            video.Visibility = Visibility.Collapsed;
            pdf.Visibility = Visibility.Collapsed;
            text.Visibility = Visibility.Collapsed;
            collection.Visibility = Visibility.Collapsed;
            IconImage.Visibility = Visibility.Collapsed;

            /*
            // move this code to a switch statement if we eventually don't support icons for certain types
            var iconUri = SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId).SmallIconUri;
            IconImage.Source = new BitmapImage(iconUri);
            IconImage.Visibility = Visibility.Visible;
            */

            switch (element.Type)
            {
                // icons supported
                case ElementType.Image:
                case ElementType.Video:
                    var iconUri = SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId).SmallIconUri;
                    IconImage.Source = new BitmapImage(iconUri);
                    IconImage.Visibility = Visibility.Visible;
                    break;
                // no icon support
                case ElementType.PDF:
                case ElementType.Audio:
                case ElementType.Text:
                default:
                    SwitchType(element.Type);
                    break;
            }




        }
    }
}
