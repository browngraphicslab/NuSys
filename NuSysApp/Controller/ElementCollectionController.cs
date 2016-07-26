using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ElementCollectionController : ElementController
    {

        public delegate void ChildChangedHandler(object source, ElementController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public delegate void CollectionViewChangedHandler(object source, CollectionElementModel.CollectionViewType type);
        public event CollectionViewChangedHandler CollectionViewChanged;

        public ElementCollectionController(ElementModel model) : base(model)
        {
            var contentModel = SessionController.Instance.ContentController.GetContent(model.LibraryId);
            if (contentModel != null)
            {
                ((CollectionLibraryElementModel) contentModel).OnChildAdded += AddChildById;
                ((CollectionLibraryElementModel) contentModel).OnChildRemoved += RemoveChildById;
            }

            Disposed += OnDisposed;
        }

        private void OnDisposed(object source, object args)
        {
            var contentModel = SessionController.Instance.ContentController.GetContent(Model.LibraryId);
            if (contentModel != null)
            {
                ((CollectionLibraryElementModel)contentModel).OnChildAdded -= AddChildById;
                ((CollectionLibraryElementModel)contentModel).OnChildRemoved -= RemoveChildById;
            }

            Disposed -= OnDisposed;
        }

        private void AddChildById(string id)
        {
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                AddChild(SessionController.Instance.IdToControllers[id]);
            }
        }

        private void RemoveChildById(string id)
        {
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                RemoveChild(SessionController.Instance.IdToControllers[id]);
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

        public void SetCollectionViewType(CollectionElementModel.CollectionViewType type)
        {
            var colModel = (Model as CollectionElementModel);
            colModel.ActiveCollectionViewType = type;
            CollectionViewChanged?.Invoke(this, type);

            _debouncingDictionary.Add("collectionview", colModel.ActiveCollectionViewType.ToString());
        }

        /// <summary>
        /// Sets whether or not the collection is finite
        /// </summary>
        /// <param name="isFinite"></param>
        public void SetFinite(bool isFinite)
        {
            var colModel = Model as CollectionElementModel;
            if (colModel == null)
            {
                return;
            }
            colModel.CollectionLibraryElementModel.IsFinite = isFinite;
            _debouncingDictionary.Add("finite",isFinite);
        }

        /// <summary>
        /// Sets the shape of the model -- i.e. the points the shape is defined by
        /// </summary>
        /// <param name="points"></param>
        public void SetShape(List<Windows.Foundation.Point> points)
        {
            var colModel = Model as CollectionElementModel;
            if (colModel == null)
            {
                return;
            }
            colModel.CollectionLibraryElementModel.ShapePoints = points;
            _debouncingDictionary.Add("points",points);
        }

    }
}
