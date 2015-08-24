
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
            Text = data;
            byte[] textToBytes = Convert.FromBase64String(Text); //Converts RTF to Byte array
            Content = new Content(textToBytes, id);
            this.ID = id;
          
        }

        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                byte[] newTextBytes = Convert.FromBase64String(_text);
                Content = new Content(newTextBytes, ID); //Update Content
                if (NetworkConnector.Instance.ModelLocked)
                {
                    OnTextChanged?.Invoke(this, new TextChangedEventArgs("Text changed", Text));
                }
                else
                {
                    this.DebounceDict.Add("text", value);
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
            dict.Add("nodeType", NodeType.Text.ToString());
            return dict;
        }

        public override XmlElement WriteXML(XmlDocument doc)
        {

            //XmlElement 
            XmlElement textNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);//TODO Make his polymorphic
            foreach(XmlAttribute attr in basicXml)
            {
                textNode.SetAttributeNode(attr);
            }

            doc.LoadXml(textNode.OuterXml);
            //textNode.InnerText = Text;


            //Text (TODO: Uncomment this section when we figure out how to store just the string of the textnode)
            XmlAttribute text = doc.CreateAttribute("text");
            //text.Value = "<![CDATA[" + Text + "]]>";
            //textNode.SetAttributeNode(text);

            textNode.InnerText = Text;

            Debug.WriteLine("Text = " + text.Value.ToString());

            //Text (TODO: Uncomment this section when we figure out how to store just the string of the textnode)
            //XmlCDataSection text = doc.CreateCDataSection(Text);
            //textNode.AppendChild(text);

            /*string xml = Text;
            StringBuilder encodedString = new StringBuilder(xml.Length);
            using(var writer = XmlWriter.Create(encodedString))
            {
                writer.WriteString(xml);
            }*/

            Debug.WriteLine("Updated XML = " + textNode.InnerXml);
            Debug.WriteLine("Text = " + Text);

            return textNode;       
        }
    }
}
