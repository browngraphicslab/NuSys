using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class DisplayNodeModel : ElementModel
    {
        public DisplayNodeModel(string id) : base(id)
        {
            ElementType = NusysConstants.ElementType.Display;
        }
    }
}
