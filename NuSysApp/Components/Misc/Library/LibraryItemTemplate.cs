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
        public string RegionId { get; private set; }
        public Uri ThumbnailUri { get; private set; }
        public LibraryItemTemplate(LibraryElementController controller)
        {
            Title = controller.LibraryElementModel.Title;
            controller.TitleChanged += Controller_TitleChanged;
            ThumbnailUri = controller.SmallIconUri;
            Timestamp = controller.LibraryElementModel.Timestamp?.Substring(0, controller.LibraryElementModel.Timestamp.Length - 3);
            Type = controller.LibraryElementModel.Type.ToString();
            ContentID = controller.LibraryElementModel.LibraryElementId;
        }

        public LibraryItemTemplate(RegionController controller)
        {
            Title = controller.Title;
            controller.TitleChanged += Controller_TitleChanged;
            ThumbnailUri = controller.SmallIconUri;
            Timestamp = controller.LibraryElementModel.Timestamp.Substring(0, controller.LibraryElementModel.Timestamp.Length - 3);
            Type = controller.LibraryElementModel.Type.ToString();
            ContentID = controller.Model.Id;
        }

        private void Controller_TitleChanged(object sender, string title)
        {
            if (title == null)
            {
                return;
            }
            Title = title;
        }



        
    }
}
