using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        public string InAtomId { get; set; }
        public string OutAtomId { get; set; }

        //public Color Color { get; set; }
        public LinkLibraryElementModel(string id): base(id, NusysConstants.ElementType.Link)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_IN_ID_KEY))
            {
                InAtomId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_IN_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_OUT_ID_KEY))
            {
                OutAtomId = message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_OUT_ID_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
