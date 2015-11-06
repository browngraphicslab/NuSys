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
        public event NodeChangeHandler NodeRemoved;

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

        public void RemoveNode(NodeModel nodeModel)
        {
            _children.Remove(nodeModel);
            NodeRemoved?.Invoke(this, nodeModel);
        }

        public InqCanvasModel InqModel
        {
            get { return this._inqModel; }
        }
    }
}