using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        public LinkId InAtomId { get; set; }
        public LinkId OutAtomId { get; set; }
        public Color Color { get; set; }
        public LinkLibraryElementModel(LinkId id1, LinkId id2, string id, Color c, ElementType elementType = ElementType.Link, Dictionary<string, MetadataEntry> metadata = null, string contentName = null, bool favorited = false): base(id, elementType, metadata, contentName, favorited)

        {
            InAtomId = id1;
            OutAtomId = id2;

            Color = c;
        }

        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("id1"))
            {
                InAtomId = JsonConvert.DeserializeObject<LinkId>((string)message["id1"]);
            }
            if (message.ContainsKey("id2"))
            {
                OutAtomId = JsonConvert.DeserializeObject<LinkId>((string)message["id2"]);
            }
            if (message.ContainsKey("color"))
            {

                string hexColor = message.GetString("color");
                byte a = byte.Parse(hexColor.Substring(1, 2), NumberStyles.HexNumber);
                byte r = byte.Parse(hexColor.Substring(3, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(hexColor.Substring(5, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(hexColor.Substring(7, 2), NumberStyles.HexNumber);

                Color = Color.FromArgb(a, r, g, b);
                //Color = Color.FromArgb(message.GetString("color"));
            }
            base.UnPack(message);

        }

    }
}
