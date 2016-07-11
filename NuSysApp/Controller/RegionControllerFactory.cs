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
        public RegionController CreateFromSendable(Region regionModel, string contentId)
        {
            RegionController controller = null;

            switch (regionModel.Type)
            {
                case Region.RegionType.Rectangle:
                    var imageModel = regionModel as RectangleRegion;
                    controller = new RectangleRegionController(imageModel);
                    break;
                case Region.RegionType.Pdf:
                    var pdfModel = regionModel as PdfRegion;
                    controller = new PdfRegionController(pdfModel);
                    break;
                case Region.RegionType.Time:
                    var audioModel = regionModel as TimeRegionModel;
                    controller = new AudioRegionController(audioModel);
                    break;
                case Region.RegionType.Video:
                    Debug.Assert(regionModel is VideoRegionModel);
                    controller = new VideoRegionController(regionModel as VideoRegionModel);
                    break;
                
            }
            if (controller == null)
            {
                return null;
            }

            SessionController.Instance.RegionsController.Add(controller, contentId);
            return controller;
        }
    }
}
