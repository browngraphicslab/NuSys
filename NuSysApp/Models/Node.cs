using NuSysApp.Models;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool IsAnnotation { get; set; }
        public Atom ClippedParent { get; set; }

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
            basicXml.Add(type);

            XmlAttribute id = doc.CreateAttribute("id");
            id.Value = ID.ToString();
            basicXml.Add(id);

            if (ParentGroup != null)
            {
                XmlAttribute groupID = doc.CreateAttribute("groupID");
                groupID.Value = this.ParentGroup.ID.ToString();
                basicXml.Add(groupID);
            }

            XmlAttribute x = doc.CreateAttribute("x");
            x.Value = Transform.Matrix.OffsetX.ToString();
            basicXml.Add(x);

            XmlAttribute y = doc.CreateAttribute("y");
            y.Value = Transform.Matrix.OffsetY.ToString();
            basicXml.Add(y);

            XmlAttribute height = doc.CreateAttribute("height");
            height.Value = Height.ToString();
            basicXml.Add(height);

            XmlAttribute width = doc.CreateAttribute("width");
            width.Value = Width.ToString();
            basicXml.Add(width);

            // if the node is an annotation, add information to the xml about the link it is attached to
            if (this.IsAnnotation)
            {
                XmlAttribute clippedParent = doc.CreateAttribute("ClippedParent");
                clippedParent.Value = ClippedParent.ID.ToString();
                basicXml.Add(clippedParent);
            }

            return basicXml;
        }
    }
}


