using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectWrite;

namespace NuSysApp
{
    public class LinkHomeTabViewModel : DetailHomeTabViewModel
    {
        public string LinkFrom { get; private set; }
        public string LinkTo { get; private set; }
        public string Annotation { get; private set; }

        private LinkLibraryElementController _controller;
        public LinkHomeTabViewModel(LinkLibraryElementController controller) : base(controller)
        {
            _controller = controller;
            var linkModel = controller.LinkLibraryElementModel;

            if (SessionController.Instance.RegionsController.IsRegionId(linkModel.OutAtomId))
            {
                var fromController = SessionController.Instance.RegionsController.GetRegionController(linkModel.OutAtomId);
                if (fromController == null)
                {
                    var fromLibraryElementController =
                        SessionController.Instance.ContentController.GetLibraryElementController(
                            linkModel.OutAtomId);
                    LinkFrom = fromLibraryElementController.Title;

                }
                else
                {
                    LinkFrom = fromController.Title;
                }
                
            }
            else
            {
                var fromController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.OutAtomId);
                LinkFrom = fromController.Title;
            }

            if (SessionController.Instance.RegionsController.IsRegionId(linkModel.InAtomId))
            {
                var toController = SessionController.Instance.RegionsController.GetRegionController(linkModel.InAtomId);
                if (toController == null)
                {
                    var toLibraryElementController =
                        SessionController.Instance.ContentController.GetLibraryElementController(
                            linkModel.InAtomId);
                    LinkTo = toLibraryElementController.Title;
                    return;
                }
                else
                {
                    LinkTo = toController?.Title;
                }
                
            }
            else
            {
                var toController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.InAtomId);
                LinkTo = toController.Title;
            }

            Annotation = controller.LinkLibraryElementModel.Data;
            controller.ContentChanged += Controller_ContentChanged; 

        }

        private void Controller_ContentChanged(object source, string contentData)
        {
            _controller.ContentChanged -= Controller_ContentChanged;
            Annotation = contentData;
            _controller.ContentChanged += Controller_ContentChanged;

        }

        public void UpdateAnnotation(string text)
        {

            _controller.SetContentData(text); 
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

        public override Message GetNewRegionMessage()
        {
            return null;
            //throw new NotImplementedException();
        }
    }
}
