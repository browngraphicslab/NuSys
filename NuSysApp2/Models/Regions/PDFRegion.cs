using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class PdfRegion : RectangleRegion 
    {

        public int PageLocation { get; set; }
        public PdfRegion(string id) : base(id,ElementType.PdfRegion)
        {
        }
        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("page_location"))
            {
                PageLocation = message.GetInt("page_location");
            }
            await base.UnPack(message);
        }
    }
}
