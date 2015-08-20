
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;

namespace NuSysApp
{
    public class TextNode : Node
    {
        private string _text;
        public TextNode(string data, string id): base(id)
        {
            Text = data;
            this.ID = id;
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                if (NetworkConnector.Instance.ModelLocked)
                {
                    RaisePropertyChanged("Model_Text");
                }
                else
                {
                    this.DebounceDict.Add("text", value);
                    //Debug.WriteLine("Got the text: "+value);
                }
            } 
        }

        public override string GetContentSource()
        {
            return Text;
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
            dict.Add("text",Text);
            return dict;
        }
        public override XmlElement WriteXML(XmlDocument doc)
        {

            //XmlElement 
            XmlElement textNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach(XmlAttribute attr in basicXml)
            {
                textNode.SetAttributeNode(attr);
            }

            //Text (TODO: Uncomment this section when we figure out how to store just the string of the textnode)
            ////XmlAttribute text = doc.CreateAttribute("text");
            ////text.Value = currModel.Text;
            ////textNode.SetAttributeNode(text);

            return textNode;       
        }
    }
}
