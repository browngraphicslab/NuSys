
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;

namespace NuSysApp
{
    public class TextNode : Node
    {
        public TextNode(string data, string id): base(id)
        {
            Text = data;
            this.ID = id;
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }

        public override void UnPack(Dictionary<string, string> props)
        {
            if (props.ContainsKey("text"))
            {
                Text = props["text"];
                this.DebounceDict.Add("text", Text);
            }
            base.UnPack(props);
        }

        public override Dictionary<string,string> Pack()
        {
            Dictionary<string, string> dict = base.Pack();
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
