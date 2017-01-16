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

            switch(pageType)
            {
                case DetailViewPageType.Home:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Text:
                            break;
                        case NusysConstants.ElementType.Image:
                            break;
                        case NusysConstants.ElementType.Collection:
                            break;
                        case NusysConstants.ElementType.PDF:
                            break;
                        case NusysConstants.ElementType.Audio:
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
                            break;
                        case NusysConstants.ElementType.Image:
                            break;
                        case NusysConstants.ElementType.Collection:
                            break;
                        case NusysConstants.ElementType.PDF:
                            break;
                        case NusysConstants.ElementType.Audio:
                            break;
                        case NusysConstants.ElementType.Video:
                            break;
                        case NusysConstants.ElementType.Link:
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
                            rectangle = new DetailViewImageRegionPage(parent, resourceCreator, controller as ImageLibraryElementController);
                            break;
                        case NusysConstants.ElementType.PDF:
                            rectangle = new DetailViewPdfRegionPage(parent, resourceCreator, controller as PdfLibraryElementController);
                            break;
                        case NusysConstants.ElementType.Audio:
                            rectangle = new BaseMediaPlayerUIElement(parent, resourceCreator, controller as AudioLibraryElementController);
                            break;
                        case NusysConstants.ElementType.Video:
                            rectangle = new BaseMediaPlayerUIElement(parent, resourceCreator, controller as AudioLibraryElementController);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No regions page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Aliases:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Text:
                            break;
                        case NusysConstants.ElementType.Image:
                            break;
                        case NusysConstants.ElementType.Collection:
                            break;
                        case NusysConstants.ElementType.PDF:
                            break;
                        case NusysConstants.ElementType.Audio:
                            break;
                        case NusysConstants.ElementType.Video:
                            break;
                        case NusysConstants.ElementType.Link:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No alias page support for {elementType} yet");
                    }
                    break;
                case DetailViewPageType.Links:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Text:
                            break;
                        case NusysConstants.ElementType.Image:
                            break;
                        case NusysConstants.ElementType.Collection:
                            break;
                        case NusysConstants.ElementType.PDF:
                            break;
                        case NusysConstants.ElementType.Audio:
                            break;
                        case NusysConstants.ElementType.Video:
                            break;
                        case NusysConstants.ElementType.Link:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No links page support for {elementType} yet");
                    }
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