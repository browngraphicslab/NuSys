using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupModel : NodeModel
    {
        public delegate void NodeChangeHandler(object source, Sendable node);
        public event NodeChangeHandler linkAdded;
        public event NodeChangeHandler ChildAdded;
        public event NodeChangeHandler ChildRemoved;

        private InqCanvasModel _inqModel;
        private readonly List<Sendable> _children = new List<Sendable>();

        public GroupModel(string id) : base(id)
        {
        }


        public void AddChild(Sendable nodeModel)
        {
            _children.Add(nodeModel);
            ChildAdded?.Invoke(this, nodeModel);
        }

        public void RemoveChild(Sendable nodeModel)
        {
            _children.Remove(nodeModel);
            ChildRemoved?.Invoke(this, nodeModel);
        }

        public InqCanvasModel InqModel
        {
            get { return this._inqModel; }
            set { _inqModel = value; }
        }

        public override async Task<Dictionary<string, string>> Pack()
        {
            var dict = await base.Pack();
            dict["nodeType"] = NodeType.Group.ToString();
            var idList = "";
            foreach (var s in Children.Keys)
            {
                idList += s + ",";
            }
            if (idList.Length > 0) { idList.Substring(0, idList.Length - 1); }
            dict.Add("idList", idList);

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
                Children = idDict;
            }
        }//TODO add in pack functions
    }
}