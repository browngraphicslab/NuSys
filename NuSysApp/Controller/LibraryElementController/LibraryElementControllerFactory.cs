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
                case NusysConstants.ElementType.Image:
                    var imageModel = model as ImageLibraryElementModel;
                    Debug.Assert(imageModel != null);
                    controller = new ImageLibraryElementController(imageModel);
                    break;
                case NusysConstants.ElementType.PDF:
                    var pdfModel = model as PdfLibraryElementModel;
                    Debug.Assert(pdfModel != null);
                    controller = new PdfLibraryElementController(pdfModel);
                    break;
                case NusysConstants.ElementType.Audio:
                    var audioModel = model as AudioLibraryElementModel;
                    Debug.Assert(audioModel != null);
                    controller = new AudioLibraryElementController(audioModel);
                    break;
                case NusysConstants.ElementType.Video:
                    var videoModel = model as VideoLibraryElementModel;
                    Debug.Assert(videoModel != null);
                    controller = new VideoLibraryElementController(videoModel);
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
