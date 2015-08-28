
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class TextNode : Node
    {
        private string _text;
        public delegate void TextChangedEventHandler(object source, TextChangedEventArgs e);
        public event TextChangedEventHandler OnTextChanged;
        public TextNode(string data, string id): base(id)
        {
            ID = id;
            Text = data;
            if (Text != null)
            {
                byte[] textToBytes = Convert.FromBase64String(Text); //Converts RTF to Byte array
                Content = new Content(textToBytes, id);
            }         
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;

                byte[] newTextBytes = System.Text.Encoding.UTF8.GetBytes(_text);
                Content = new Content(newTextBytes, ID); //Update Content

                if (NetworkConnector.Instance.ModelIntermediate.IsSendableLocked(ID))
                {
                    OnTextChanged?.Invoke(this, new TextChangedEventArgs("Text changed", Text));
                }
                else
                {
                    this.DebounceDict.Add("text", value);
                }
            } 
        }

        public override async Task UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("text"))
            {
                Text = props["text"];
            }
            base.UnPack(props);
        }

        public override async Task<Dictionary<string,string>> Pack()
        {
            Dictionary<string, string> dict = await base.Pack();
            dict.Add("text", Text);
            dict.Add("nodeType", NodeType.Text.ToString());
            return dict;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {
            byte[] newTextBytes = System.Text.Encoding.UTF8.GetBytes(Text);
            Content = new Content(newTextBytes, ID); //Update Content
            
            //XmlElement 
            XmlElement textNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc); //TODO Make his polymorphic
            foreach(XmlAttribute attr in basicXml)
            {
                textNode.SetAttributeNode(attr);
            }
            return textNode;       
        }
    }
}
