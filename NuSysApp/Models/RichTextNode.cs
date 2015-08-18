
using System.Collections.Generic;
using System.Xml;

namespace NuSysApp
{
    public class RichTextNode : Node
    {
        public RichTextNode(string data, int id): base(id)
        {
            Text = data;
        }

        public string Text { get; set; }

        public override string GetContentSource()
        {
            return Text;
        }

        public string Data
        {
            get; set;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {

            //XmlElement 
            XmlElement richTextNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                richTextNode.SetAttributeNode(attr);
            }

            //Text
            XmlAttribute text = doc.CreateAttribute("text");
            text.Value = Text;
            richTextNode.SetAttributeNode(text);

            return richTextNode;
        }
    }
}
