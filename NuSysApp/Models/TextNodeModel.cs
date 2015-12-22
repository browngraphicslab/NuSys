
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
        public event TextChangedEventHandler OnTextChanged;

        public TextNodeModel(string data, string id): base(id)
        {
            NodeType = NodeType.Text;
            Id = id;
            Text = data;       
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                byte[] newTextBytes = System.Text.Encoding.UTF8.GetBytes(_text);
                Content = new NodeContentModel(newTextBytes, Id); //Update Content

                if (NetworkConnector.Instance.IsSendableBeingUpdated(Id))
                {
                    OnTextChanged?.Invoke(this, new TextChangedEventArgs(Text));
                }
                else
                {
                    this.DebounceDict.Add("data", value);
                }
            } 
        }

        public override async Task UnPack(Message props)
        {
            if (props.ContainsKey("data"))
            {
                Text = props["data"];
            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string,string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            dict.Add("data", Text);
            dict.Add("nodeType", NodeType.Text.ToString());
            return dict;
        }
    }
}
