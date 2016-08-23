using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ElementCollectionController : ElementController
    {
        public delegate void CameraPositionChangedHandler(float x, float y);
        public event CameraPositionChangedHandler CameraPositionChanged;

        public delegate void CameraScaleChangedHandler(object source, float x);
        public event CameraScaleChangedHandler CameraScaleChanged;

        public event CameraPositionChangedHandler CameraCenterChanged;

        public delegate void ChildChangedHandler(object source, ElementController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public delegate void CollectionViewChangedHandler(object source, CollectionElementModel.CollectionViewType type);
        public event CollectionViewChangedHandler CollectionViewChanged;

        public ElementCollectionController(ElementModel model) : base(model)
        {
            var collectionController = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryId) as CollectionLibraryElementController;
            if (collectionController != null)
            {
                collectionController.OnChildAdded += AddChildById;
                collectionController.OnChildRemoved += RemoveChildById;
            }

            Disposed += OnDisposed;
        }

        public void SetFinite(bool finite)
        {
            var contentModel = SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as CollectionLibraryElementModel;
            contentModel.IsFinite = finite;
        }

        public void ChangeShape(List<Windows.Foundation.Point> shapepoints)
        {
            var contentModel = SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as CollectionLibraryElementModel;
            contentModel.ShapePoints = new List<PointModel>(shapepoints.Select(p => new PointModel(p.X,p.Y)));
        }

        private void OnDisposed(object source, object args)
        {
            var collectionController = SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId) as CollectionLibraryElementController;
            if (collectionController != null)
            {
                collectionController.OnChildAdded -= AddChildById;
                collectionController.OnChildRemoved -= RemoveChildById;
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

        public void SetCameraPosition(float x, float y)
        {
            CameraPositionChanged?.Invoke(x, y);
        }
        public void SetCameraCenter(float x, float y)
        {
            CameraCenterChanged?.Invoke(x, y);
        }


        public void SetCollectionViewType(CollectionElementModel.CollectionViewType type)
        {
            var colModel = (Model as CollectionElementModel);
            colModel.ActiveCollectionViewType = type;
            CollectionViewChanged?.Invoke(this, type);

            _debouncingDictionary.Add("collectionview", colModel.ActiveCollectionViewType.ToString());
        }
        public override async Task UnPack(Message message)
        {
            var libModel =SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as CollectionLibraryElementModel;
            if (message.ContainsKey("finite"))
            {
                libModel.IsFinite = message.GetBool("finite");
            }
            if (message.ContainsKey("shape_points"))
            {
                libModel.ShapePoints = message.GetList<PointModel>("shape_points");
            }
            base.UnPack(message);
        }

    }
}
