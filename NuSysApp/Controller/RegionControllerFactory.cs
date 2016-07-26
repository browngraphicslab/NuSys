using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{

    // Factories are awesome 
    public class RegionControllerFactory
    {
        public RegionLibraryElementController CreateFromSendable(Region regionModel)
        {
            RegionLibraryElementController libraryElementController = null;

            switch (regionModel.Type)
            {
                case NusysConstants.ElementType.ImageRegion:
                    var imageModel = regionModel as RectangleRegion;
                    libraryElementController = new RectangleRegionLibraryElementController(imageModel);
                    break;
                case NusysConstants.ElementType.PdfRegion:
                    var pdfModel = regionModel as PdfRegionModel;
                    libraryElementController = new PdfRegionLibraryElementController(pdfModel);
                    break;
                case NusysConstants.ElementType.AudioRegion:
                    var audioModel = regionModel as AudioRegionModel;
                    libraryElementController = new AudioRegionLibraryElementController(audioModel);
                    break;
                case NusysConstants.ElementType.VideoRegion:
                    Debug.Assert(regionModel is VideoRegionModel);
                    libraryElementController = new VideoRegionLibraryElementController(regionModel as VideoRegionModel);
                    break;
                
                
            }
            if (libraryElementController == null)
            {
                return null;
            }

            //SessionController.Instance.RegionsController.Add(libraryElementController, contentId);
            return libraryElementController;
        }
    }
}
