using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupModel : NodeModel
    {
        public delegate void GroupChildChangeHandler(object source, AtomModel node);

        public event GroupChildChangeHandler ChildAdded;
        public event GroupChildChangeHandler ChildRemoved;

        private InqCanvasModel _inqModel;
        private readonly List<AtomModel> _children = new List<AtomModel>();

        public GroupModel(string id) : base(id)
        {
        }

        public void AddChild(AtomModel atomModel)
        {
            _children.Add(atomModel);
            ChildAdded?.Invoke(this, atomModel);
        }

        public void RemoveChild(AtomModel atomModel)
        {
            _children.Remove(atomModel);
            ChildRemoved?.Invoke(this, atomModel);
        }

        public InqCanvasModel InqModel
        {
            get { return this._inqModel; }
        }
    }
}