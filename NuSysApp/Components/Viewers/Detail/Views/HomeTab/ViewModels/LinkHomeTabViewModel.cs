using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
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
            
            var fromController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.OutAtomId);
            LinkFrom = fromController.Title;
            
            var toController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel.InAtomId);
            LinkTo = toController.Title;
            

            Annotation = controller.Data;
            controller.ContentDataController.ContentDataUpdated += Controller_ContentChanged; 

        }

        private void Controller_ContentChanged(object source, string contentData)
        {
            _controller.ContentDataController.ContentDataUpdated -= Controller_ContentChanged;
            Annotation = contentData;
            _controller.ContentDataController.ContentDataUpdated += Controller_ContentChanged;

        }

        public void UpdateAnnotation(string text)
        {
            _controller.ContentDataController.SetData(text);
        }

        // There is no region here so this method should not be called
        public override CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            throw new NotImplementedException();
        }
    }
}
