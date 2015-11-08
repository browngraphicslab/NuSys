using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class GroupNodeModel : GroupModel
    {
        #region Private Members
        
        public event AddToGroupEventHandler OnAddToGroup;
        #endregion Events and Handlers

        public GroupNodeModel(string id): base(id)
        {
        }

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
            foreach (Sendable id in Children.Values)
            {
                NodeModel node = id as NodeModel;
                groupNode.AppendChild(node.WriteXML(doc));
            }
            return groupNode;
        }

        
    }
}
