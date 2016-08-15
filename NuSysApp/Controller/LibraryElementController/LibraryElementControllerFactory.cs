using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class LibraryElementControllerFactory
    {
        /// <summary>
        /// Takes in a library element model and returns a newly created library element controller for the model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static LibraryElementController CreateFromModel(LibraryElementModel model)
        {
            LibraryElementController controller;
            switch (model.Type)
            {
                case NusysConstants.ElementType.ImageRegion:
                    var imageModel = model as RectangleRegion;
                    Debug.Assert(imageModel != null);
                    controller = new RectangleRegionLibraryElementController(imageModel);
                    break;
                case NusysConstants.ElementType.PdfRegion:
                    var pdfModel = model as PdfRegionModel;
                    Debug.Assert(pdfModel != null);
                    controller = new PdfRegionLibraryElementController(pdfModel);
                    break;
                case NusysConstants.ElementType.AudioRegion:
                    var audioModel = model as AudioRegionModel;
                    Debug.Assert(audioModel != null);
                    controller = new AudioRegionLibraryElementController(audioModel);
                    break;
                case NusysConstants.ElementType.VideoRegion:
                    var videoModel = model as VideoRegionModel;
                    Debug.Assert(videoModel != null);
                    controller = new VideoRegionLibraryElementController(videoModel);
                    break;
                case NusysConstants.ElementType.Word:
                    //Do debug.asserts above the controller instantiation to make sure the model types are correct
                    controller = new WordNodeLibraryElementController(model);
                    SessionController.Instance.NuSysNetworkSession.LockController.AddLockable((ILockable)controller);
                    break;
                case NusysConstants.ElementType.Collection:
                    var collectionModel = model as CollectionLibraryElementModel;
                    Debug.Assert(collectionModel != null);
                    controller = new CollectionLibraryElementController(collectionModel);
                    break;
                case NusysConstants.ElementType.Link:
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
