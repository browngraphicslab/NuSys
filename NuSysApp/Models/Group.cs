using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp.Models
{
    public class Group : Node
    {
        public Group(string id): base(id)
        {
            this.ID = id;
            NodeModelList = new ObservableCollection<Node>();
        }

        public ObservableCollection<Node> NodeModelList { get; set; }

       public ObservableCollection<Link> LinkModelList { get; set; }
        public override XmlElement WriteXML(XmlDocument doc)
        {
            //Main XmlElement 
            XmlElement groupNode = doc.CreateElement(string.Empty, "Group", string.Empty); //TODO: Change how we determine node type for name

            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                groupNode.SetAttributeNode(attr);
            }

            //get nodes within groups
            foreach (Node n in NodeModelList)
            {
                groupNode.AppendChild(n.WriteXML(doc));
            }
            return groupNode;
        }
    }
}
