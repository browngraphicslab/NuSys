using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class PdfRegionModel : RectangleRegion 
    {

        public int PageLocation { get; set; }
        public PdfRegionModel(string libraryId) : base(libraryId, NusysConstants.ElementType.PdfRegion)
        {
        }
    }
}
