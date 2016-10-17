﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using NuSysApp;

namespace NuSysApp
{ 

    public static class DetailViewPageFactory
    {

        public static RectangleUIElement GetPage(DetailViewPageType pageType, LibraryElementController controller)
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
                                $"No home page support for {nameof(elementType)} yet");
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
                                $"No metadata page support for {nameof(elementType)} yet");
                    }
                    break;
                case DetailViewPageType.Region:
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Image:
                            break;
                        case NusysConstants.ElementType.PDF:
                            break;
                        case NusysConstants.ElementType.Audio:
                            break;
                        case NusysConstants.ElementType.Video:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(elementType),
                                $"No regions page support for {nameof(elementType)} yet");
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
                                $"No alias page support for {nameof(elementType)} yet");
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
                                $"No links page support for {nameof(elementType)} yet");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pageType), pageType, null);
            }

            return rectangle;
        }
    }

}
