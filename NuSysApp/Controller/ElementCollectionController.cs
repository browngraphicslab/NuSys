using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ElementCollectionController : ElementController
    {

        public delegate void ChildChangedHandler(object source, ElementController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public ElementCollectionController(ElementModel model) : base(model)
        {
        }

        public void AddChild( ElementController child )
        {
            ChildAdded?.Invoke(this, child);
        }

        public void RemoveChild(ElementController child)
        {
            ChildRemoved?.Invoke(this, child);
        }
    }
}
