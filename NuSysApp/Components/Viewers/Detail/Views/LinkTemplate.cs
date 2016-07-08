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

        public LinkTemplate(LinkLibraryElementController controller, LinkId id)
        {
            Title = "Unnamed Link";
            if (controller.Title != null)
            {
                Title = controller.Title;
            }
            controller.TitleChanged += Controller_TitleChanged;

            var linkModel = controller.LinkLibraryElementModel;
            if (linkModel.InAtomId == id)
            {
                if (linkModel.OutAtomId.IsRegion)
                {
                    var regionController = SessionController.Instance.RegionsController.GetRegionController(linkModel.OutAtomId.RegionId);
                    Debug.Assert(regionController != null);
                    LinkedTo = regionController.Title;
                }
                else
                {
                    var libraryElementModel = SessionController.Instance.ContentController.GetContent(linkModel.OutAtomId.LibraryElementId);
                    Debug.Assert(libraryElementModel != null);
                    LinkedTo = libraryElementModel.Title;
                }
                
            }
            else
            {
                if (linkModel.InAtomId.IsRegion)
                {
                    var regionController = SessionController.Instance.RegionsController.GetRegionController(linkModel.InAtomId.RegionId);
                    Debug.Assert(regionController != null);
                    LinkedTo = regionController.Title;
                }
                else
                {
                    var libraryElementModel = SessionController.Instance.ContentController.GetContent(linkModel.InAtomId.LibraryElementId);
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
