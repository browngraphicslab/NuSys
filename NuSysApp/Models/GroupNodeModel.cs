using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupNodeModel : NodeModel
    {
        #region Private Members

        private Dictionary<string, Sendable> _idDict;
        #endregion Private Members

        #region Events and Handlers
        public delegate void DeleteEventHandler(object source, DeleteEventArgs e);
        public event DeleteEventHandler OnDeletion;

        public delegate void LocationUpdateEventHandler(object source, LocationUpdateEventArgs e);
        public event LocationUpdateEventHandler OnLocationUpdate;

        public delegate void WidthHeightUpdateEventHandler(object source, WidthHeightUpdateEventArgs e);
        public event WidthHeightUpdateEventHandler OnWidthHeightUpdate;

        public delegate void AddToGroupEventHandler(object source, AddToGroupEventArgs e);

        public event AddToGroupEventHandler OnAddToGroup;
        #endregion Events and Handlers

        public GroupNodeModel(string id): base(id)
        {
            this.ID = id;
            _idDict = new Dictionary<string, Sendable>();
            NodeModelList = new ObservableCollection<NodeModel>();
        }

        public void Add(AtomModel atom)
        {
            if (_idDict.ContainsKey(atom.ID))
            {
                Debug.WriteLine("Could not add atom - Atom already exists in group");
                return;
            }
            _idDict.Add(atom.ID, atom);
            OnAddToGroup?.Invoke(this, new AddToGroupEventArgs("Added node group", this, (NodeModel)atom));
        }

        public void Remove(AtomModel atom)
        {
            if (!_idDict.ContainsKey(atom.ID))
            {
                Debug.WriteLine("Could not remove atom - Atom doesn't exist in group");
                return;
            }
            _idDict.Remove(atom.ID);
        }

        public ObservableCollection<NodeModel> NodeModelList { get; set; }

        public ObservableCollection<LinkModel> LinkModelList { get; set; }

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
            foreach (Sendable id in _idDict.Values)
            {
                NodeModel node = id as NodeModel;
                groupNode.AppendChild(node.WriteXML(doc));
            }
            return groupNode;
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict["nodeType"] = NodeType.Group.ToString();
            var idList = "";
            foreach (var s in _idDict.Keys)
            {
                idList += s + ",";
            }
            if (idList.Length > 0) { idList.Substring(0, idList.Length - 1); }
            dict.Add("idList",idList);

            return dict;
        }//TODO add in pack functions
        public override async Task UnPack(Message props)
        {
            base.UnPack(props);

            if (props.ContainsKey("idList"))
            {
                var ids = props["idList"];
                var idList = ids.Split(',');
                var idDict = new Dictionary<string, Sendable>();
                foreach (string id in idList)
                {
                    var tempNode = (NodeModel)SessionController.Instance.IdToSendables[id];
                    idDict.Add(id, tempNode);
                }
                _idDict = idDict;
            }
        }//TODO add in pack functions
    }
}
