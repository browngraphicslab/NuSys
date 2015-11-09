
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
            ID = id;
                Text = data;       
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;

                byte[] newTextBytes = System.Text.Encoding.UTF8.GetBytes(_text);
                Content = new ContentModel(newTextBytes, ID); //Update Content

                if (NetworkConnector.Instance.IsSendableBeingUpdated(ID))
                {
                    OnTextChanged?.Invoke(this, new TextChangedEventArgs("Text changed", Text));
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

        public override XmlElement WriteXML(XmlDocument doc)
        {
            byte[] newTextBytes = System.Text.Encoding.UTF8.GetBytes(Text);
            Content = new ContentModel(newTextBytes, ID); //Update Content
            
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
