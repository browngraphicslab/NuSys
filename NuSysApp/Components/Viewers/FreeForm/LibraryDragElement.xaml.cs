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
using NusysIntermediate;

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

        public void SwitchType(NusysConstants.ElementType type)
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
                case NusysConstants.ElementType.Text:
                    text.Visibility = Visibility.Visible;
                    break;
                case NusysConstants.ElementType.ImageRegion:
                case NusysConstants.ElementType.Image:
                    image.Visibility = Visibility.Visible;
                    break;
                case NusysConstants.ElementType.Collection:
                    collection.Visibility = Visibility.Visible;
                    break;
                case NusysConstants.ElementType.PdfRegion:
                case NusysConstants.ElementType.PDF:
                    pdf.Visibility = Visibility.Visible;
                    break;
                case NusysConstants.ElementType.AudioRegion:
                case NusysConstants.ElementType.Audio:
                    audio.Visibility = Visibility.Visible;
                    break;
                case NusysConstants.ElementType.VideoRegion:
                case NusysConstants.ElementType.Video:
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
                case NusysConstants.ElementType.Image:
                case NusysConstants.ElementType.Video:
                case NusysConstants.ElementType.PDF:
                case NusysConstants.ElementType.Audio:
                case NusysConstants.ElementType.Text:
                case NusysConstants.ElementType.Link:
                case NusysConstants.ElementType.Collection:
                    var iconUri = SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId).SmallIconUri;
                    IconImage.Source = new BitmapImage(iconUri);
                    IconImage.Visibility = Visibility.Visible;
                    break;
                // no icon support          
                default:
                    SwitchType(element.Type);
                    break;
            }

        }
    }
}
