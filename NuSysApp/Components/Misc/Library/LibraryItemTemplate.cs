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
        public string LibraryElementId { get; private set; }
        public string RegionId { get; private set; }
        public Uri ThumbnailUri { get; private set; }
        public LibraryItemTemplate(LibraryElementController controller)
        {
            Title = controller.LibraryElementModel.Title;
            controller.TitleChanged += Controller_TitleChanged;
            ThumbnailUri = controller.SmallIconUri;
            Timestamp = controller.LibraryElementModel.Timestamp?.Substring(0, Math.Max(0,controller.LibraryElementModel.Timestamp.Length - 3));
            Type = controller.LibraryElementModel.Type.ToString();
            LibraryElementId = controller.LibraryElementModel.LibraryElementId;
        }

        public LibraryItemTemplate(RegionLibraryElementController libraryElementController)
        {
            Title = libraryElementController.Title;
            libraryElementController.TitleChanged += Controller_TitleChanged;
            ThumbnailUri = libraryElementController.SmallIconUri;
            Timestamp = libraryElementController.LibraryElementModel.Timestamp.Substring(0, libraryElementController.LibraryElementModel.Timestamp.Length - 3);
            Type = libraryElementController.LibraryElementModel.Type.ToString();
            LibraryElementId = libraryElementController.LibraryElementModel.LibraryElementId;
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
