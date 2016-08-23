using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp2
{
    public class LibraryBucketViewModel
    {
        public delegate void NewContentsEventHandler(ICollection<LibraryElementModel> elements);

        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(LibraryElementModel element);

        public event NewElementAvailableEventHandler OnNewElementAvailable;

        public delegate void ElementDeletedEventHandler(LibraryElementModel element);

        public event ElementDeletedEventHandler OnElementDeleted;

        public delegate void HighlightElementEventHandler(LibraryElementModel element, bool highlight);

        public event HighlightElementEventHandler OnHighlightElement;

        private double _width, _height;

        public LibraryBucketViewModel()
        {
            SessionController.Instance.ContentController.OnNewContent += FireNewContentAvailable;
            SessionController.Instance.ContentController.OnElementDelete += FireElementDeleted;
        }
        
        private void FireNewContentAvailable(LibraryElementModel content)
        {
            OnNewElementAvailable?.Invoke(content);
        }

        private void FireElementDeleted(LibraryElementModel element)
        {
            OnElementDeleted?.Invoke(element);
        }

    }
}
