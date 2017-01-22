using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class ElementCollectionController : ElementController, ToolStartable
    {

        public delegate void CameraPositionChangedHandler(float x, float y);
        public event CameraPositionChangedHandler CameraPositionChanged;

        public delegate void CameraScaleChangedHandler(object source, float x);
        public event CameraScaleChangedHandler CameraScaleChanged;

        public event CameraPositionChangedHandler CameraCenterChanged;

        /// <summary>
        /// This is the number of groups that this is contained in, so if we have an infinite loop of groups
        /// then we can look at this variable and cut off the rendering of new groups 
        /// 
        /// this is session only, not saved to the server
        /// </summary>
        public int Depth { get; set; }


        public delegate void ChildChangedHandler(object source, ElementController child);
        public event ChildChangedHandler ChildAdded;
        public event ChildChangedHandler ChildRemoved;

        public delegate void CollectionViewChangedHandler(object source, CollectionElementModel.CollectionViewType type);
        public event CollectionViewChangedHandler CollectionViewChanged;

        public ElementCollectionController(ElementModel model) : base(model)
        {
            Depth = 0;
            var collectionController = SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryId) as CollectionLibraryElementController;

            Debug.Assert(collectionController != null);
            if (collectionController != null)
            {
                collectionController.OnChildAdded += AddChildById;
                collectionController.OnChildRemoved += RemoveChildById;
                collectionController.FiniteBoolChanged += CollectionControllerOnFiniteBoolChanged;
            }
            ToolController.ToolControllers.Add(Id, this);
        }

        /// <summary>
        /// event fired when the library element model's finite boolean changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void CollectionControllerOnFiniteBoolChanged(object sender, bool b)
        {
            if (b)
            {
                SetCameraCenter((float) Constants.InitialCenter, (float) Constants.InitialCenter);
            }
        }

        /// <summary>
        /// Creates a tool from this collection at the point passed in. Or if there is already a tool at the point
        /// it just adds the collection as a parent
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void CreateToolFromCollection(float x, float y)
        {

            var dragDestinationController = (SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(new Vector2(x, y), null, 2) as ToolWindow)?.Vm?.Controller; //maybe replace null w render engine.root

            if (dragDestinationController != null)
            {
                dragDestinationController.AddParent(this);
            }
            else
            {
                var canvasCoordinate = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2(x, y), SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection);
                var model = new BasicToolModel();
                var controller = new BasicToolController(model);
                controller.AddParent(this);
                UITask.Run(() =>
                {
                    var viewModel = new BasicToolViewModel(controller)
                    {
                        Filter = ToolModel.ToolFilterTypeTitle.Title,
                    };
                    controller.SetSize(500, 500);
                    controller.SetPosition(canvasCoordinate.X, canvasCoordinate.Y);
                    SessionController.Instance.ActiveFreeFormViewer.AddTool(viewModel);
                });
            }
        }



        public void SetFinite(bool finite)
        {
            var contentModel = SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as CollectionLibraryElementModel;
            contentModel.IsFinite = finite;
        }

        public override void Dispose()
        {
            var collectionController = SessionController.Instance.ContentController.GetLibraryElementController(Model.LibraryId) as CollectionLibraryElementController;
            Debug.Assert(collectionController != null);
            if (collectionController != null)
            {
                collectionController.OnChildAdded -= AddChildById;
                collectionController.OnChildRemoved -= RemoveChildById;
                collectionController.FiniteBoolChanged -= CollectionControllerOnFiniteBoolChanged;
            }
            ToolController.ToolControllers.Remove(Id);

            base.Dispose();

        }

        private void AddChildById(string id)
        {
            if (SessionController.Instance.ElementModelIdToElementController.ContainsKey(id))
            {
                AddChild(SessionController.Instance.ElementModelIdToElementController[id]);
            }
            RefreshFromTopOfChain();
        }

        private void RemoveChildById(string id)
        {
            if (SessionController.Instance.ElementModelIdToElementController.ContainsKey(id))
            {
                RemoveChild(SessionController.Instance.ElementModelIdToElementController[id]);
            }
            RefreshFromTopOfChain();
        }
        public void AddChild( ElementController child )
        {
            ChildAdded?.Invoke(this, child);
        }

        public void RemoveChild(ElementController child)
        {
            ChildRemoved?.Invoke(this, child);
        }


        public void SetCameraScale(float scale)
        {
            CameraScaleChanged?.Invoke(this,scale);
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
            base.UnPack(message);
        }

        /// <summary>
        /// Returns list of elements within the collection as the output library ids
        /// The bool recursively reload does nothing because the collection can only ever be the
        /// start of a tool chain.
        /// </summary>
        public HashSet<string> GetOutputLibraryIds()
        {
            var libraryElementIds = new HashSet<string>();
            var collectionLibraryElementModel =
                SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as
                    CollectionLibraryElementModel;
            foreach (var node in collectionLibraryElementModel.Children)
            {
                if (SessionController.Instance.ElementModelIdToElementController.ContainsKey(node))
                {
                    libraryElementIds.Add(
                        SessionController.Instance.ElementModelIdToElementController[node]?
                            .LibraryElementModel?.LibraryElementId);
                }
            }
            return libraryElementIds;
        }

        /// <summary>
        /// since this collection has no parents to merge, this method jsut returns GetOutputLibraryIds().
        /// </summary>
        /// <param name="recursiveRefresh"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUpdatedDataList()
        {
            return GetOutputLibraryIds();
        }

        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        public event EventHandler<string> Disposed;
        public event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;
        public string GetID()
        {
            return Id;
        }

        /// <summary>
        /// Returns an empty hashset because a collection has no parents
        /// </summary>
        public HashSet<string> GetParentIds()
        {
            return new HashSet<string>();
        }

        public void RefreshFromTopOfChain()
        {
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }
    }
}
