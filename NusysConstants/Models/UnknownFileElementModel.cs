using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class UnknownFileElementModel : ElementModel
    {
        public UnknownFileElementModel(string id): base(id)
        {
            ElementType = NusysConstants.ElementType.Unknown;
        }

        public override async Task<Dictionary<string, object>> Pack()
        {
            return await base.Pack();
        }
    }
}
