using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        /// <summary>
        /// This is the library element id of the in library element 
        /// </summary>
        public string InAtomId { get; set; }

        /// <summary>
        /// This is the library element id of the out library element
        /// </summary>
        public string OutAtomId { get; set; }

        /// <summary>
        /// This controls the visualization of the link's directional arrow.
        /// </summary>
        public NusysConstants.LinkDirection ArrowDirection { get; set; }

        //public Color Color { get; set; }
        public LinkLibraryElementModel(string id): base(id, NusysConstants.ElementType.Link)
        {
        }

        public override void UnPackFromDatabaseKeys(Message message)
        {
            if (message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY))
            {
                InAtomId = message.GetString(NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY);
            }
            if (message.ContainsKey(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY))
            {
                OutAtomId = message.GetString(NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY);
            }
            base.UnPackFromDatabaseKeys(message);
        }
    }
}
