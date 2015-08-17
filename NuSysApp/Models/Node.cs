using NuSysApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class Node : Atom
    {
        public Node(int id) : base(id)
        {
            StartLines = new List<Link>();
            EndLines = new List<Link>();
        }

        public string Data { get; set; }

        public Content Content { set; get; }

        public List<Link> StartLines { get; }

        public List<Link> EndLines { get; }

        public List<Node> ConnectedNodes { get; }

        public int X { get; set; }

        public int Y { get; set; }

        public MatrixTransform Transform { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public Constants.NodeType NodeType { get; set; }

        public Group ParentGroup { get; set; }

        public virtual string GetContentSource()
        {
            return null;
        }


        public virtual XmlElement WriteXML(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                node.SetAttributeNode(attr);
            }

            return node;
        }

        /// <summary>
        /// Writes the XML of the attributes that all nodes have
        /// </summary>
        /// <param name="doc">Main xmlDocument</param>
        /// <returns></returns>

        public List<XmlAttribute> getBasicXML(XmlDocument doc)
        {
            List<XmlAttribute> basicXml = new List<XmlAttribute>();

            //create xml attribute nodes
            XmlAttribute type = doc.CreateAttribute("nodeType");
            type.Value = NodeType.ToString();

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = ID.ToString();


            //TODO: Uncomment this when parent group IDs are set
            //XmlAttribute groupID = doc.CreateAttribute("groupID");
            //groupID.Value = ParentGroup.Model.ID.ToString();

            XmlAttribute x = doc.CreateAttribute("x");
            x.Value = Transform.Matrix.OffsetX.ToString();

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = Transform.Matrix.OffsetY.ToString();

            XmlAttribute height = doc.CreateAttribute("height");
            height.Value = Height.ToString();

            XmlAttribute width = doc.CreateAttribute("width");
            width.Value = Width.ToString();

            //append to list and return
            basicXml.Add(type);
            basicXml.Add(id);
            basicXml.Add(x);
            basicXml.Add(y);
            basicXml.Add(height);
            basicXml.Add(width);

            return basicXml;
        }

    }
}


