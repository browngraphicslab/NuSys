﻿using System;
using System.Collections.Generic;
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
                            break;
                        case NusysConstants.ElementType.Image:
                            rectangle = new DetailViewImagePage(parent, resourceCreator, controller as ImageLibraryElementController, true, false);
                            break;
                        case NusysConstants.ElementType.Collection:
                            break;
                        case NusysConstants.ElementType.PDF:
                            rectangle = new DetailViewPdfPage(parent, resourceCreator, controller as PdfLibraryElementController, true, false);
                            break;
                        case NusysConstants.ElementType.Audio:
                            rectangle = new DetailViewAudioPage(parent, resourceCreator, controller as AudioLibraryElementController, false);
                            break;
                        case NusysConstants.ElementType.Video:
                            break;
                        case NusysConstants.ElementType.Link:
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
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No regions page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Aliases:
                    rectangle = new DetailViewAliasesPage(parent, resourceCreator, controller);
                    await rectangle.Load();
                    break;
                case DetailViewPageType.Links:
                    rectangle = new DetailViewLinksPage(parent, resourceCreator, controller);
                    await rectangle.Load();
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
