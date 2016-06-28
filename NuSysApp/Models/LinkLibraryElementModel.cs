using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        public string InAtomId { get; set; }
        public string OutAtomId { get; set; }
        public LinkLibraryElementModel(string id1, string id2, string id, ElementType elementType = ElementType.Link, Dictionary<string, Tuple<string, Boolean>> metadata = null, string contentName = null, bool favorited = false): base(id, elementType, metadata, contentName, favorited)
        {
            InAtomId = id1;
            OutAtomId = id2;
        }

        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("id1"))
            {
                InAtomId = message.GetString("id1");
            }
            if (message.ContainsKey("id2"))
            {
                OutAtomId = message.GetString("id2");
            }
            base.UnPack(message);
        }

    }
}
