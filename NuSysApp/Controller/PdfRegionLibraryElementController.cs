using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;


namespace NuSysApp
{
    public class PdfRegionLibraryElementController : RectangleRegionLibraryElementController
    {
        public delegate void PageLocationChangedEventHandler(object sender, int pageLocation);

        public event PageLocationChangedEventHandler PageLocationChanged;


        public event LocationChangedEventHandler LocationChanged;
        public delegate void LocationChangedEventHandler(object sender, Point topLeft);

        public event SizeChangedEventHandler SizeChanged;
        public delegate void SizeChangedEventHandler(object sender, double width, double height);

        public PdfRegionLibraryElementController(PdfRegion model) : base(model)
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

        public void SetSize(double width, double height)
        {
            Model.Width = width;
            Model.Height = height;
            SizeChanged?.Invoke(this, width, height);
            UpdateServer();
        }
        public void SetLocation(Point topLeft)
        {
            Model.TopLeftPoint = new Point(Math.Max(0.001, topLeft.X), Math.Max(0.001, topLeft.Y));
            LocationChanged?.Invoke(this, Model.TopLeftPoint);
            UpdateServer();
        }

        public override void UnPack(Region region)
        {
            SetBlockServerBoolean(true);
            var r = region as PdfRegion;
            if (r != null)
            {
                SetPageLocation(r.PageLocation);
            }
            base.UnPack(region);
            SetBlockServerBoolean(false);
        }
    }
}
