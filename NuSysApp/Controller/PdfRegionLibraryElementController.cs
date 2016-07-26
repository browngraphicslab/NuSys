using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;


namespace NuSysApp
{
    public class PdfRegionLibraryElementController : RectangleRegionLibraryElementController
    {
        public delegate void PageLocationChangedEventHandler(object sender, int pageLocation);
        public event PageLocationChangedEventHandler PageLocationChanged;

        public PdfRegionModel PdfRegionModel
        {
            get
            {
                return base.LibraryElementModel as PdfRegionModel;
            }
        }

        public PdfRegionLibraryElementController(PdfRegionModel model) : base(model)
        {
        }

        public void SetPageLocation(int page)
        {
            PdfRegionModel.PageLocation = page;
            PageLocationChanged?.Invoke(this, page);
        }

        public override void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey("page_location"))
            {
                SetPageLocation(message.GetInt("page_location", 1));
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);
        }
    }
}
