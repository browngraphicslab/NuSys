using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class PdfRegionController : RegionController
    {
        public delegate void PageLocationChangedEventHandler(object sender, int pageLocation);

        public event PageLocationChangedEventHandler PageLocationChanged;
        public PdfRegionController(PdfRegion model) : base(model)
        {
            
        }

        public void SetPageLocation(int page)
        {
            var pdfRegion = Model as PdfRegion;
            if (pdfRegion == null)
            {
                return;
            }
            pdfRegion.PageLocation = page;
            PageLocationChanged?.Invoke(this, page);
        }
    }
}
