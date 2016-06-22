using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RegionControllerFactory
    {
        public RegionController CreateFromSendable(Region regionModel)
        {
            RegionController controller = null;

            switch (regionModel.Type)
            {
                case Region.RegionType.Rectangle:
                    controller = new RegionController(regionModel);
                    break;
                case Region.RegionType.Pdf:
                    var pdfModel = regionModel as PdfRegion;
                    controller = new PdfRegionController(pdfModel);
                    break;
                case Region.RegionType.Time:
                    controller = new RegionController(regionModel);
                    break;
                case Region.RegionType.Video:
                    controller = new RegionController(regionModel);
                    break;
            }

            if (controller == null)
                return null;
            return controller;
        }
    }
}
