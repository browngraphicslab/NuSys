using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkHomeTabViewModel : DetailHomeTabViewModel
    {
        public string LinkFrom { get; private set; }
        public string LinkTo { get; private set; }

        private LinkLibraryElementController _controller;
        public LinkHomeTabViewModel(LinkLibraryElementController controller, HashSet<Region> regionsToLoad) : base(controller, regionsToLoad)
        {
            _controller = controller;
            var linkModel = controller.LinkLibraryElementModel;

            if (linkModel.OutAtomId.IsRegion)
            {
                var fromController = SessionController.Instance.RegionsController.GetRegionController(linkModel.OutAtomId.RegionId);
                if (fromController == null)
                {
                    var fromLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.OutAtomId.LibraryElementId);
                    LinkFrom = fromLibraryElementController.Title;
                    return;
                }
                LinkFrom = fromController.Title;
            }
            else
            {
                var fromController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.OutAtomId.LibraryElementId);
                LinkFrom = fromController.Title;
            }

            if (linkModel.InAtomId.IsRegion)
            {
                var toController = SessionController.Instance.RegionsController.GetRegionController(linkModel.InAtomId.RegionId);
                if (toController == null)
                {
                    var toLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.InAtomId.LibraryElementId);
                    LinkTo = toLibraryElementController.Title;
                    return;
                }
                LinkTo = toController?.Title;
            }
            else
            {
                var toController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.InAtomId.LibraryElementId);
                LinkTo = toController.Title;
            }


        }

        public void UpdateAnnotation(string text)
        {
            _controller.LinkLibraryElementModel.Data = text;
        }

        public override void AddRegion(object sender, RegionController regionController)
        {
            //throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            //throw new NotImplementedException();
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            //throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {
            //throw new NotImplementedException();
        }

        public override Region GetNewRegion()
        {
            return null;
            //throw new NotImplementedException();
        }
    }
}
