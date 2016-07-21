using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{

    // Factories are awesome 
    public class RegionControllerFactory
    {
        public RegionLibraryElementController CreateFromSendable(Region regionModel, string contentId)
        {
            RegionLibraryElementController libraryElementController = null;

            switch (regionModel.Type)
            {
                case Region.RegionType.Rectangle:
                    var imageModel = regionModel as RectangleRegion;
                    libraryElementController = new RectangleRegionLibraryElementController(imageModel);
                    break;
                case Region.RegionType.Pdf:
                    var pdfModel = regionModel as PdfRegion;
                    libraryElementController = new PdfRegionLibraryElementController(pdfModel);
                    break;
                case Region.RegionType.Time:
                    var audioModel = regionModel as TimeRegionModel;
                    libraryElementController = new AudioRegionLibraryElementController(audioModel);
                    break;
                case Region.RegionType.Video:
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
