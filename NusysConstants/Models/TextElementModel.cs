using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using NusysIntermediate;

namespace NusysIntermediate
{
    public class TextElementModel : ElementModel
    {
        public delegate void TextChangedEventHandler(object source, string text);
        public event TextChangedEventHandler TextChanged;

        public TextElementModel(string id): base(id)
        {
            ElementType = NusysConstants.ElementType.Text;
        }

        public override async Task UnPack(Message props)
        {
            await base.UnPack(props);
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            return await base.Pack();
        }
    }
}
