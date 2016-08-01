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
    public class LinkLibraryElementModel : LibraryElementModel
    {
        public string InAtomId { get; set; }
        public string OutAtomId { get; set; }
        public Color Color { get; set; }
        public bool IsBiDirectional { get; set; }
        public LinkLibraryElementModel(string id1, string id2, string id) : base(id, ElementType.Link)
        {
            InAtomId = id1;
            OutAtomId = id2;
            Color = Colors.DarkGoldenrod;
            IsBiDirectional = true;
        }
    }
}
