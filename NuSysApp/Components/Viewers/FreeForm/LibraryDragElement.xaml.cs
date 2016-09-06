using System;
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

        /// <summary>
        /// Sets the drag icon
        /// </summary>
        /// <param name="element"></param>
        public void SetIcon(LibraryElementModel element)
        {
            IconImage.Visibility = Visibility.Collapsed;

            switch (element.Type) //honestly wtf is this switch statmenet
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
                    Debug.Fail($"The element type {element.Type} passed in does not have thumbnail support or it should be listed above");
                    break;
            }

        }

        /// <summary>
        /// Set the icon of the drag element, does not work for image or video, for that you need to use the LibraryElementModel overload
        /// </summary>
        /// <param name="elementType"></param>
        public void SetIcon(NusysConstants.ElementType elementType)
        {
            IconImage.Visibility = Visibility.Collapsed;

            switch (elementType)
            {
                //default icon support
                case NusysConstants.ElementType.PDF:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/pdf.png"));
                    break;
                case NusysConstants.ElementType.Audio:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/audio.png"));
                    break;
                case NusysConstants.ElementType.Text:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/text.png"));
                    break;
                case NusysConstants.ElementType.Collection:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png"));
                    break;
                case NusysConstants.ElementType.Word:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/word.png"));
                    break;
                case NusysConstants.ElementType.Link:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/library_thumbnails/link.png"));
                    break;
                case NusysConstants.ElementType.Tools:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/tools icon.png"));
                    break;
                case NusysConstants.ElementType.Recording:
                    IconImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/audio main menu.png"));
                    break;
                // no default icon support          
                default:
                    Debug.Fail(
                        "The element passed in does not have a default thumbnail, or it's default thumbnail is not listed here." +
                        "If you passed in an image or a video, please use the LibraryElementModel overload");
                    return;
            }
            IconImage.Visibility = Visibility.Visible;
        }
    }
}
