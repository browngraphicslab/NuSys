﻿using NusysIntermediate;

namespace NuSysApp
{
    public class ElementControllerFactory
    {
        /// <summary>
        /// Returns the element controller for the passed in element model
        /// </summary>
        /// <param name="elementModel"></param>
        /// <returns></returns>
        public static ElementController CreateFromModel(ElementModel elementModel)
        {
            ElementController controller;

            switch (elementModel.ElementType)
            {
                case NusysConstants.ElementType.Text:
                    controller = new TextNodeController((TextElementModel)elementModel);
                    break;
                case NusysConstants.ElementType.Image:
                    controller = new ImageElementIntanceController(elementModel);
                    break;
                case NusysConstants.ElementType.Word:
                    controller = new ElementController(elementModel);
                    break;
                case NusysConstants.ElementType.PDF:
                    controller = new ElementController(elementModel);
                    break;
                case NusysConstants.ElementType.Audio:
                    controller = new ElementController(elementModel);
                    break;
                case NusysConstants.ElementType.Video:
                    controller = new ElementController(elementModel);
                    break;
                case NusysConstants.ElementType.Collection:
                    controller = new ElementCollectionController(elementModel);
                    break;
                default:
                    controller = new ElementController(elementModel);
                    break;
            }
            return controller;
        }

        
}
}