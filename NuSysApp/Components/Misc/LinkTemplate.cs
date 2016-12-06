using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkTemplate : BaseINPC
    {
        private string _title;
        // This stores the title of the element the link goes to
        public string LinkedTo { get; private set; }
        // The ID of the element the link goes to
        public string IDLinkedTo { get; private set; }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            }
        }

        public string ID { get; set; }

        public LinkTemplate(LinkLibraryElementController controller, string detailViewOpenElementContentId)
        {
            Title = "Unnamed Link";
            if (controller.Title != null)
            {
                Title = controller.Title;
            }
            controller.TitleChanged += Controller_TitleChanged;

            var linkModel = controller.LinkLibraryElementModel;
            if (linkModel.InAtomId == detailViewOpenElementContentId)
            {
                var libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(linkModel.OutAtomId);
                Debug.Assert(libraryElementModel != null);
                LinkedTo = libraryElementModel.Title;
                IDLinkedTo = libraryElementModel.LibraryElementId;

            }
            else
            {
                var libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(linkModel.InAtomId);
                Debug.Assert(libraryElementModel != null);
                LinkedTo = libraryElementModel.Title;
                IDLinkedTo = libraryElementModel.LibraryElementId;
            }
            ID = controller.LinkLibraryElementModel.LibraryElementId;
        }

        private void Controller_TitleChanged(object sender, string title)
        {
            Title = title;
        }

    }
}
