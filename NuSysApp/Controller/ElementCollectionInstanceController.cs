using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ElementCollectionInstanceController : ElementInstanceController
    {

        public delegate void ChildChangedHandler(object source, ElementInstanceController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public ElementCollectionInstanceController(ElementInstanceModel model) : base(model)
        {
        }

        public void AddChild( ElementInstanceController child )
        {
            var model = (ElementCollectionInstanceModel) Model;
            model.ElementCollectionModel.Children.Add(child.Model);

            ChildAdded?.Invoke(this, child);
        }

        public void RemoveChild(ElementInstanceController child)
        {
            var model = (ElementCollectionInstanceModel)Model;
            model.ElementCollectionModel.Children.Remove(child.Model);

            ChildRemoved?.Invoke(this, child);
        }
    }
}
