using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class HtmlLibraryElementModel : ImageLibraryElementModel
    {
        public HtmlLibraryElementModel(string libraryElementId, NusysConstants.ElementType type = NusysConstants.ElementType.HTML) : base(libraryElementId, type)
        {

        }
    }
}
