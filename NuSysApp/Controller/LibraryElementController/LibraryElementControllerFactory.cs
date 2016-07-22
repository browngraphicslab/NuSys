using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LibraryElementControllerFactory
    {
        public static LibraryElementController CreateFromModel(LibraryElementModel model)
        {
            LibraryElementController controller;
            switch (model.Type)
            {
                case ElementType.ImageRegion:
                    var imageModel = model as RectangleRegion;
                    Debug.Assert(imageModel != null);
                    controller = new RectangleRegionLibraryElementController(imageModel);
                    break;
                case ElementType.PdfRegion:
                    var pdfModel = model as PdfRegionModel;
                    Debug.Assert(pdfModel != null);
                    controller = new PdfRegionLibraryElementController(pdfModel);
                    break;
                case ElementType.AudioRegion:
                    var audioModel = model as AudioRegionModel;
                    Debug.Assert(audioModel != null);
                    controller = new AudioRegionLibraryElementController(audioModel);
                    break;
                case ElementType.VideoRegion:
                    var videoModel = model as VideoRegionModel;
                    Debug.Assert(videoModel != null);
                    controller = new VideoRegionLibraryElementController(videoModel);
                    break;

                case ElementType.Word:
                    //Do debug.asserts above the controller instantiation to make sure the model types are correct
                    controller = new WordNodeLibraryElementController(model);
                    SessionController.Instance.NuSysNetworkSession.LockController.AddLockable((ILockable)controller);
                    break;
                case ElementType.Link:
                    Debug.Assert(model is LinkLibraryElementModel);
                    controller = new LinkLibraryElementController(model as LinkLibraryElementModel);
                    //SessionController.Instance.LinksController.CreateVisualLinks(controller as LinkLibraryElementController);
                    break;
                default:
                    controller = new LibraryElementController(model);
                    break;
            }
            return controller;
        }
    }
}
