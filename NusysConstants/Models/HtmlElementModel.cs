using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class HtmlElementModel : ImageElementModel
    {
        public HtmlElementModel(string elementId) : base(elementId)
        {
            ElementType = NusysConstants.ElementType.HTML;
        }
    }
}
