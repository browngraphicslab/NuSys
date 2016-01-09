
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class TextNodeModel : NodeModel
    {
        private string _text = string.Empty;
        public delegate void TextChangedEventHandler(object source, TextChangedEventArgs e);
        public event TextChangedEventHandler TextChanged;

        public TextNodeModel(string id): base(id)
        {
            NodeType = NodeType.Text;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                SessionController.Instance.ContentController.Get(ContentId).Data = _text;
                TextChanged?.Invoke(this, new TextChangedEventArgs(_text));
            } 
        }

        public override async Task UnPack(Message props)
        {
            base.UnPack(props);
            if (ContentId != null)
               _text = SessionController.Instance.ContentController.Get(ContentId).Data;
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            var dict = await base.Pack();
            return dict;
        }
    }
}
