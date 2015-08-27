using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;

namespace NuSysApp
{
    public class Group : Node
    {
        private Dictionary<string, Sendable> _idDict;
        public Group(string id): base(id)
        {
            this.ID = id;
            _idDict = new Dictionary<string, Sendable>();
            NodeModelList = new ObservableCollection<Node>();
        }

        public void Add(Atom atom)
        {
            if (_idDict.ContainsKey(atom.ID))
            {
                Debug.WriteLine("Could not add atom - Atom already exists in group");
                return;
            }
            _idDict.Add(atom.ID, atom);
        }

        public void Remove(Atom atom)
        {
            if (!_idDict.ContainsKey(atom.ID))
            {
                Debug.WriteLine("Could not remove atom - Atom doesn't exist in group");
                return;
            }
            _idDict.Remove(atom.ID);
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

        public override async Task UnPack(Dictionary<string, string> props)
        {
            base.UnPack(props);
        }//TODO add in pack functions
    }
}
