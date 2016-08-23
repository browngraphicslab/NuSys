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
        public string LinkedTo { get; private set; }

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
                if (SessionController.Instance.RegionsController.IsRegionId(linkModel.OutAtomId))
                {
                    var regionController = SessionController.Instance.RegionsController.GetRegionController(linkModel.OutAtomId);
                    Debug.Assert(regionController != null);
                    LinkedTo = regionController.Title;
                }
                else if(SessionController.Instance.LinksController.IsContentId(linkModel.OutAtomId))
                {
                    var libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(linkModel.OutAtomId);
                    Debug.Assert(libraryElementModel != null);
                    LinkedTo = libraryElementModel.Title;
                }
                //LinkedTo = "";
            }
            else
            {
                if (SessionController.Instance.RegionsController.IsRegionId(linkModel.InAtomId))
                {
                    var regionController = SessionController.Instance.RegionsController.GetRegionController(linkModel.InAtomId);
                    Debug.Assert(regionController != null);
                    LinkedTo = regionController.Title;
                }
                else if (SessionController.Instance.LinksController.IsContentId(linkModel.InAtomId))
                {
                    var libraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(linkModel.InAtomId);
                    Debug.Assert(libraryElementModel != null);
                    LinkedTo = libraryElementModel.Title;
                }
                
            }
            ID = controller.LinkLibraryElementModel.LibraryElementId;
        }

        private void Controller_TitleChanged(object sender, string title)
        {
            Title = title;
        }

    }
}
