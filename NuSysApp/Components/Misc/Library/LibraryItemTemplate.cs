using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class LibraryItemTemplate : BaseINPC
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public string Type { get; private set; }
        public string Timestamp { get; private set; }
        public string ContentID { get; private set; }
        
        public BitmapImage ThumbnailUri { get; private set; }
        public LibraryItemTemplate(LibraryElementController controller)
        {
            Title = controller.LibraryElementModel.Title;
            controller.TitleChanged += Controller_TitleChanged;
            ThumbnailUri = new BitmapImage(controller.SmallIconUri);
            Timestamp = controller.LibraryElementModel.Timestamp;
            Type = controller.LibraryElementModel.Type.ToString();
            ContentID = controller.LibraryElementModel.LibraryElementId;
        }

        private void Controller_TitleChanged(object sender, string title)
        {
            Title = title;
        }

        
    }
}
