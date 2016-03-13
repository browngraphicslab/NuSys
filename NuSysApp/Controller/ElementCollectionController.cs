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
            var contentModel = SessionController.Instance.ContentController.Get(model.ContentId);
            if (contentModel != null)
            {
                ((CollectionContentModel)contentModel).OnChildAdded += delegate(string id)
                {
                    if (SessionController.Instance.IdToControllers.ContainsKey(id))
                    {
                        AddChild(SessionController.Instance.IdToControllers[id]);
                    }
                };
                ((CollectionContentModel)contentModel).OnChildRemoved += delegate (string id)
                {
                    if (SessionController.Instance.IdToControllers.ContainsKey(id))
                    {
                        RemoveChild(SessionController.Instance.IdToControllers[id]);
                    }
                };
            }
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
