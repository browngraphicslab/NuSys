using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using LdaLibrary;


namespace NuSysApp
{
    public class PdfRegionController : RectangleRegionController
    {
        public delegate void PageLocationChangedEventHandler(object sender, int pageLocation);

        public event PageLocationChangedEventHandler PageLocationChanged;


        public event LocationChangedEventHandler LocationChanged;
        public delegate void LocationChangedEventHandler(object sender, Point topLeft);

        public event SizeChangedEventHandler SizeChanged;
        public delegate void SizeChangedEventHandler(object sender, double width, double height);

        public PdfRegion PdfRegionModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is PdfRegion);
                return LibraryElementModel as PdfRegion;
            }
        }

        public PdfRegionController(PdfRegion model) : base(model)
        {
        }

        public void SetPageLocation(int page)
        {
            if (_blockServerInteraction)
            {
                return;
            }
            var pdfRegion = PdfRegionModel as PdfRegion;
            if (pdfRegion == null)
            {
                return;
            }
            pdfRegion.PageLocation = page;
            PageLocationChanged?.Invoke(this, page);
            _debouncingDictionary.Add("page_location", page);
        }

        public override void UnPack(Message message)
        {
            SetBlockServerInteraction(true);
            if (message.ContainsKey("page_location"))
            {
                SetPageLocation(message.GetInt("page_location"));
            }
            base.UnPack(message);
            SetBlockServerInteraction(false);
        }
    }
}
