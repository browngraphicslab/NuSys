
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
        private string _text;
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
                Content.Data = System.Text.Encoding.UTF8.GetBytes(_text);
                TextChanged?.Invoke(this, new TextChangedEventArgs(_text));
            } 
        }

        public override async Task UnPack(Message props)
        {
            var text = props.GetString("data", "");
            Content = new NodeContentModel(System.Text.Encoding.UTF8.GetBytes(text), Id);
            _text = text;
            base.UnPack(props);
        }

        public override async Task<Dictionary<string,object>> Pack()
        {
            var dict = await base.Pack();
            // TODO: Fix this fix
            if (Text.Length > 2)
                dict.Add("data", Text.Substring(0,Text.Length-2));
            return dict;
        }
    }
}
