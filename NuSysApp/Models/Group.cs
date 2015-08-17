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

        }

        public ObservableCollection<UserControl> AtomViewList { get; set; }

        public ObservableCollection<NodeViewModel> NodeViewModelList { get; set; }

        public ObservableCollection<LinkViewModel> LinkViewModelList { get; set; }

        public override XmlElement WriteXML(XmlDocument doc)
        {

            //Main XmlElement 
            XmlElement groupNode = doc.CreateElement(string.Empty, "Node", string.Empty); //TODO: Change how we determine node type for name


            //Other attributes - id, x, y, height, width
            List<XmlAttribute> basicXml = this.getBasicXML(doc);
            foreach (XmlAttribute attr in basicXml)
            {
                groupNode.SetAttributeNode(attr);
            }

            //get nodes within groups
            foreach (NodeViewModel nodevm in NodeViewModelList)
            {
                groupNode.AppendChild(nodevm.WriteXML(doc));
            }
            return groupNode;
        }
    }
}
