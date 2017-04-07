using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using NuSysApp;

namespace NuSysApp
{ 

    public static class DetailViewPageFactory
    {

        public static async Task<RectangleUIElement> GetPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, DetailViewPageType pageType, LibraryElementController controller)
        {
            RectangleUIElement rectangle = null;
            var elementType = controller.LibraryElementModel.Type;

            //asynchronously fetches the content data model.  If it exists locally, this call is constant time.
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(controller.LibraryElementModel.ContentDataModelId);
            switch(pageType)
            {
                case DetailViewPageType.Home:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Text:
                            rectangle = new DetailViewTextPage(parent, resourceCreator, controller);
                            break;
                        case NusysConstants.ElementType.HTML:
                        case NusysConstants.ElementType.Image:
                            Debug.Assert(controller is ImageLibraryElementController);
                            rectangle = new DetailViewImagePage(parent, resourceCreator, controller as ImageLibraryElementController, true, false);
                            break;
                        case NusysConstants.ElementType.Collection:
                            Debug.Assert(controller is CollectionLibraryElementController);
                            rectangle = new DetailViewCollectionPage(parent, resourceCreator, controller as CollectionLibraryElementController);
                            break;
                        case NusysConstants.ElementType.PDF:
                            Debug.Assert(controller is PdfLibraryElementController);
                            rectangle = new DetailViewPdfPage(parent, resourceCreator, controller as PdfLibraryElementController, true, false);
                            break;
                        case NusysConstants.ElementType.Audio:
                            Debug.Assert(controller is AudioLibraryElementController);
                            rectangle = new DetailViewAudioPage(parent, resourceCreator, controller as AudioLibraryElementController, false);
                            break;
                        case NusysConstants.ElementType.Video:
                            Debug.Assert(controller is VideoLibraryElementController);
                            rectangle = new DetailViewVideoPage(parent, resourceCreator, controller as VideoLibraryElementController, false);
                            break;
                        case NusysConstants.ElementType.Link:
                            Debug.Assert(controller is LinkLibraryElementController);
                            rectangle = new DetailViewLinkPage(parent, resourceCreator, controller as LinkLibraryElementController);
                            break;
                        case NusysConstants.ElementType.Word:
                            Debug.Assert(controller is WordNodeLibraryElementController);
                            rectangle = new DetailViewWordPage(parent, resourceCreator, controller as WordNodeLibraryElementController);
                            break;
                        case NusysConstants.ElementType.Unknown:
                            rectangle = new DetailViewUnknownFilePage(parent, resourceCreator, controller as LibraryElementController);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No home page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Metadata:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Text:
                        case NusysConstants.ElementType.Image:
                        case NusysConstants.ElementType.Collection:
                        case NusysConstants.ElementType.PDF:
                        case NusysConstants.ElementType.Audio:
                        case NusysConstants.ElementType.Video:
                        case NusysConstants.ElementType.Link:
                        case NusysConstants.ElementType.HTML:
                        case NusysConstants.ElementType.Unknown:
                            rectangle = new DetailViewMetadataPage(parent, resourceCreator, controller);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No metadata page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Region:

                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Image:
                            rectangle = new DetailViewImagePage(parent, resourceCreator, controller as ImageLibraryElementController, false, true);
                            break;
                        case NusysConstants.ElementType.PDF:
                            rectangle = new DetailViewPdfPage(parent, resourceCreator, controller as PdfLibraryElementController, false, true);
                            break;
                        case NusysConstants.ElementType.Audio:
                            rectangle = new DetailViewAudioPage(parent, resourceCreator, controller as AudioLibraryElementController, true);
                            break;
                        case NusysConstants.ElementType.Video:
                            rectangle = new DetailViewVideoPage(parent, resourceCreator, controller as VideoLibraryElementController, true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No regions page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Aliases:
                    rectangle = new DetailViewAliasesPage(parent, resourceCreator, controller);
                    break;
                case DetailViewPageType.Links:
                    rectangle = new DetailViewLinksPage(parent, resourceCreator, controller);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null);
            }
            var load = rectangle?.Load();
            if (load != null) await load;

            return rectangle;
        }
    }
}
