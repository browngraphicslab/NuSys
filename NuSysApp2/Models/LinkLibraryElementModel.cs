using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Newtonsoft.Json;

namespace NuSysApp2
{
    public class LinkLibraryElementModel: LibraryElementModel
    {
        public string InAtomId { get; set; }
        public string OutAtomId { get; set; }
        public Color Color { get; set; }
        public LinkLibraryElementModel(string id1, string id2, string id): base(id, ElementType.Link)
        {
            InAtomId = id1;
            OutAtomId = id2;
            Color = Colors.DarkGoldenrod;
        }

        public override async Task UnPack(Message message)
        {
            if (message.ContainsKey("id1"))
            {
                InAtomId = message["id1"] as string;
                Debug.Assert(InAtomId != null);
            }
            if (message.ContainsKey("id2"))
            {
                OutAtomId = message["id2"] as string;
                Debug.Assert(OutAtomId != null);
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
