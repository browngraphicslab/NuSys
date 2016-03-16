using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CollectionContentModel : NodeContentModel
    {
        private HashSet<string> _children;

        public delegate void ChildAddedEventHandler(string id);
        public event ChildAddedEventHandler OnChildAdded;

        public delegate void ChildRemovedEventHandler(string id);
        public event ChildRemovedEventHandler OnChildRemoved;
        public CollectionContentModel(string id, IEnumerable<string> startingIDs = null, string contentName = null) : base(null,id, ElementType.Collection, contentName)
        {
            _children = startingIDs != null ? new HashSet<string>(startingIDs) : new HashSet<string>();
        }

        public bool AddChild(string id)
        {
            if (! _children.Contains(id))
            {
                _children.Add(id);
                OnChildAdded?.Invoke(id);
                return true;
            }
            return false;
        }

        public bool RemoveChild(string id)
        {
            if (_children.Contains(id))
            {
                _children.Remove(id);
                OnChildRemoved?.Invoke(id);
                return true;
            }
            return false;
        }
        public HashSet<string> Children { get { return _children; } }
    }
}
