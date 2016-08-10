using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Numerics;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using SharpDX.Direct2D1;
using Matrix3x2 = System.Numerics.Matrix3x2;
using System.Numerics;
using Windows.Devices.Input;
using Windows.Media.Playback;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
using Point = Windows.Foundation.Point;
using Vector2 = System.Numerics.Vector2;


namespace NuSysApp
{
    public class NuSysRenderer
    { 
        private static volatile NuSysRenderer instance;
        private static object syncRoot = new Object();

        private CanvasAnimatedControl _canvas;

        private TempLinkRenderItem _tempLink;
        private MinimapRenderItem _minimap;
        private SelectMode _selectMode;
        private List<uint> _activePointers = new List<uint>();
        private ElementSelectionRenderItem _elementSelectionRenderItem;
        public CollectionRenderItem CurrentCollection { get; private set; }
        private Dictionary<CollectionRenderItem, CollectionInteractionManager> _interactionManagers = new Dictionary<CollectionRenderItem, CollectionInteractionManager>();
        private enum LinkType { Semantic, Trail, None }
        private LinkType _currentLinkType = LinkType.Semantic;
        private LinkType _selectedLinkType = LinkType.None;


        public CanvasAnimatedControl Canvas
        {
            get { return _canvas; }
        }

        public Size Size { get; set; }
        public CollectionRenderItem InitialCollection { get; private set; }
        public ObservableCollection<ElementRenderItem> Selections { get; set; } = new ObservableCollection<ElementRenderItem>();
        private Vector2 _markingMenuStartPos;

        private NuSysRenderer()
        {
        }

        private void CanvasOnCreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            
        }

        public async Task Init(CanvasAnimatedControl canvas)
        {
            Size = new Size(canvas.Width, canvas.Height);
            _canvas = canvas;
            _canvas.Draw += CanvasOnDraw;
            _canvas.Update += CanvasOnUpdate;
            _canvas.CreateResources += CanvasOnCreateResources;
            _canvas.SizeChanged += CanvasOnSizeChanged;

            _canvas.PointerPressed += XRenderCanvasOnPointerPressed;
            _canvas.PointerReleased += XRenderCanvasOnPointerReleased;

            var vm = (FreeFormViewerViewModel) canvas.DataContext;
            InitialCollection = new CollectionRenderItem(vm, null, canvas, true);
            SwitchCollection(InitialCollection);
            

            vm.X = 0;
            vm.Y = 0;
            vm.Width = Size.Width;
            vm.Height = Size.Height;
            _elementSelectionRenderItem = new ElementSelectionRenderItem(vm, InitialCollection, _canvas);
     
            _minimap = new MinimapRenderItem(vm, InitialCollection, canvas);
        }

        private void OnInkStopped(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink stopped");
            CurrentCollection.InkRenderItem.StopInkByEvent(e);
        }

        private void OnInkDrawing(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink drawing");
            CurrentCollection.InkRenderItem.UpdateInkByEvent(e);
        }

        private void OnInkStarted(PointerRoutedEventArgs e)
        {
            //Debug.WriteLine("ink started");
            CurrentCollection.InkRenderItem.StartInkByEvent(e);
        }

        private void XRenderCanvasOnPointerReleased(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Remove(args.Pointer.PointerId);
        }

        private void XRenderCanvasOnPointerPressed(object sender, PointerRoutedEventArgs args)
        {
            if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                _activePointers.Add(args.Pointer.PointerId);
        }

        private void OnItemLongTapped(BaseRenderItem element, PointerRoutedEventArgs args)
        {
            var collection = element as CollectionRenderItem;
            SwitchCollection(collection);
        }

        private void SwitchCollection(CollectionRenderItem collection)
        {
            if (collection != CurrentCollection && collection != null)
            {
                if (CurrentCollection != null) { 
                    _interactionManagers[CurrentCollection].ItemTapped -= OnItemTapped;
                    _interactionManagers[CurrentCollection].ItemLongTapped -= OnItemLongTapped;
                    _interactionManagers[CurrentCollection].ItemDoubleTapped -= OnItemDoubleTapped;
                    _interactionManagers[CurrentCollection].InkStarted -= OnInkStarted;
                    _interactionManagers[CurrentCollection].InkDrawing -= OnInkDrawing;
                    _interactionManagers[CurrentCollection].InkStopped -= OnInkStopped;
                    _interactionManagers[CurrentCollection].LinkCreated -= OnLinkCreated;
                    _interactionManagers[CurrentCollection].MarkingMenuPointerReleased -= OnMarkingMenuPointerReleased;
                    _interactionManagers[CurrentCollection].MarkingMenuPointerMove -= OnMarkingMenuPointerMove;
                    _interactionManagers[CurrentCollection].DuplicateCreated -= OnDuplicateCreated;
                    _interactionManagers[CurrentCollection].Dispose();
                    _interactionManagers.Remove(CurrentCollection);
                }

                CurrentCollection = collection;

                _interactionManagers[collection] = new CollectionInteractionManager(collection);
                _interactionManagers[collection].ItemTapped += OnItemTapped;
                _interactionManagers[collection].ItemLongTapped += OnItemLongTapped;
                _interactionManagers[collection].ItemDoubleTapped += OnItemDoubleTapped;
                _interactionManagers[CurrentCollection].InkStarted += OnInkStarted;
                _interactionManagers[CurrentCollection].InkDrawing += OnInkDrawing;
                _interactionManagers[CurrentCollection].InkStopped += OnInkStopped;
                _interactionManagers[CurrentCollection].LinkCreated += OnLinkCreated;
                _interactionManagers[CurrentCollection].MarkingMenuPointerReleased += OnMarkingMenuPointerReleased;
                _interactionManagers[CurrentCollection].MarkingMenuPointerMove += OnMarkingMenuPointerMove;
                _interactionManagers[CurrentCollection].DuplicateCreated += OnDuplicateCreated;

            }
        }

        private void OnItemDoubleTapped(BaseRenderItem item, PointerRoutedEventArgs args)
        {
            var element = item as ElementRenderItem;
            if (element == null)
                return;

            var libraryElementModelId = element.ViewModel.Controller.LibraryElementModel.LibraryElementId;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);
            SessionController.Instance.SessionView.ShowDetailView(controller);
        }

        private async void OnDuplicateCreated(ElementRenderItem element, Vector2 point)
        {
            var targetPoint = Vector2.Transform(point, Win2dUtil.Invert(GetTransformUntil(element)));
            element.ViewModel.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y, new Message(await element.ViewModel.Model.Pack()));
        }

        private void OnMarkingMenuPointerMove(Vector2 point)
        {
            var range = 35f;
            int startIndex;
            switch (_currentLinkType)
            {
                case LinkType.Semantic:
                    startIndex = 1;
                    break;
                case LinkType.Trail:
                    startIndex = 2;
                    break;
                default:
                    startIndex = 0;
                    break;
            }

            var deltaY = point.Y - _markingMenuStartPos.Y;
            var newY = startIndex * range + deltaY + range/2;

            if (newY < range)
            {
                _selectedLinkType = LinkType.None;
                _tempLink.Color = Colors.Transparent;
            } else if (newY >= range && newY < range*2)
            {
                //_currentLinkType = LinkType.Semantic;
                _selectedLinkType = LinkType.Semantic;
                _tempLink.Color = Colors.DodgerBlue;
            }
            else if (newY >= range*3)
            {
                //_currentLinkType = LinkType.Trail;
                _selectedLinkType = LinkType.Trail;
                _tempLink.Color = Colors.PaleVioletRed;
            }
        }

        private void OnMarkingMenuPointerReleased()
        {
            CurrentCollection.Remove(_tempLink);
            
            if (_tempLink.Element1.ViewModel.ContentId == _tempLink.Element2.ViewModel.ContentId)
                return;

            var m = new Message();
            m["id1"] = _tempLink.Element1.ViewModel.ContentId;
            m["id2"] = _tempLink.Element2.ViewModel.ContentId;
            if (_selectedLinkType == LinkType.Semantic)
                SessionController.Instance.LinksController.RequestLink(m);
            else if (_selectedLinkType == LinkType.Trail)
                SessionController.Instance.NuSysNetworkSession.AddPresentationLink(_tempLink.Element1.ViewModel.Id, _tempLink.Element2.ViewModel.Id, CurrentCollection.ViewModel.Controller.LibraryElementModel.LibraryElementId);

            _currentLinkType = _selectedLinkType;

        }

        private void OnLinkCreated(ElementRenderItem element1, ElementRenderItem element2)
        {
            Color color;
            switch (_currentLinkType)
            {
                case LinkType.Semantic:
                    color = Colors.DodgerBlue;
                    break;
                case LinkType.Trail:
                    color = Colors.PaleVioletRed;
                    break;
                default:
                    color = Colors.Transparent;
                    break;
            }

            _markingMenuStartPos = _interactionManagers[CurrentCollection].MarkingMenuPointerPosition;
            _tempLink = new TempLinkRenderItem(element1, element2, color, CurrentCollection, _canvas);
            CurrentCollection.AddTempLink(_tempLink);

            /*
            if (element1.ViewModel.ContentId == element2.ViewModel.ContentId)
                return;

            var m = new Message();
            m["id1"] = element1.ViewModel.ContentId;
            m["id2"] = element2.ViewModel.ContentId;
            SessionController.Instance.LinksController.RequestLink(m);
            */
        }

        private void OnItemTapped(BaseRenderItem element, PointerRoutedEventArgs args)
        {
            if (Selections.Count == 1)
            {
                var p = args.GetCurrentPoint(null).Position;
                var vec = new Vector2((float)p.X, (float)p.Y);
                if (_elementSelectionRenderItem.BtnDelete.HitTest(vec))
                {
                    SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Selections.First().ViewModel.Id));
                    Selections.RemoveAt(0);
                    return;
                }
                if (_elementSelectionRenderItem.BtnPresent.HitTest(vec))
                {
                    var sv = SessionController.Instance.SessionView;
                    sv.EnterPresentationMode(Selections.First().ViewModel);
                }
            }


            var elementRenderItem = element as ElementRenderItem;
            if (elementRenderItem == InitialCollection || elementRenderItem == CurrentCollection || elementRenderItem == null)
            {
                ClearSelections();
                if (element == null)
                    SwitchCollection(InitialCollection);
            }
            else
            {
                if (args.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                {
                    var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Control);
                  
                    if (!keyState.HasFlag(CoreVirtualKeyStates.Down))
                        ClearSelections();
                    elementRenderItem.ViewModel.IsSelected = true;
                    Selections.Add(elementRenderItem);
                }

                if (args.Pointer.PointerDeviceType == PointerDeviceType.Touch)
                {
                    if (_activePointers.Count == 0)
                        ClearSelections();
                    elementRenderItem.ViewModel.IsSelected = true;
                    Selections.Add(elementRenderItem);
                }
            }
        }

        private void ClearSelections()
        {
            foreach (var selection in Selections)
            {
                selection.ViewModel.IsSelected = false;
            }
            Selections.Clear();
        }


        private void CanvasOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Size = sizeChangedEventArgs.NewSize;
            _minimap.CreateResources();
            _minimap.IsDirty = true;
        }

        public BaseRenderItem GetRenderItemAt(Vector2 sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            collection = collection ?? InitialCollection;
            var mat = GetTransformUntil(collection);
            return _GetRenderItemAt(collection, sp, mat, 0, maxLevel);
        }

        public BaseRenderItem GetRenderItemAt(Point sp, CollectionRenderItem collection = null, int maxLevel = int.MaxValue)
        {
            var result = GetRenderItemAt(new Vector2((float)sp.X, (float)sp.Y), collection, maxLevel);
            return result;
        }

        public Matrix3x2 GetCollectionTransform(CollectionRenderItem collection)
        {
            var transforms = new List<CollectionRenderItem> {collection};
           
            var parent = collection.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();
            return transforms.Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }

        public Matrix3x2 GetTransformUntil(BaseRenderItem item, bool withCamera = false)
        {
            var transforms = new List<I2dTransformable>();
        
            var parent = item.Parent;
            while (parent != null)
            {
                transforms.Add(parent);
                parent = parent.Parent;
            }

            transforms.Reverse();

            return transforms.Select(t1 => t1 as CollectionRenderItem).Aggregate(Matrix3x2.Identity, (current, t) => Win2dUtil.Invert(t.Camera.C)*t.Camera.S*t.Camera.C*t.Camera.T*Win2dUtil.Invert(t.C)*t.S*t.C*t.T*current);
        }



        private BaseRenderItem _GetRenderItemAt(CollectionRenderItem collection, Vector2 sp, Matrix3x2 transform, int currentLevel, int maxLevel)
        {


            if (currentLevel < maxLevel)
            {
                var poo = Win2dUtil.Invert(collection.Camera.C) * collection.Camera.S * collection.Camera.C * collection.Camera.T *
          Win2dUtil.Invert(collection.C) * collection.S * collection.C * collection.T * transform;
                var childTransform = Win2dUtil.Invert(poo);

                foreach (var renderItem in collection.GetRenderItems())
                {
                    var innerCollection = renderItem as CollectionRenderItem;
                    if (innerCollection != null)
                    {

                        if (currentLevel + 1 < maxLevel)
                        {
                            var result = _GetRenderItemAt(innerCollection, sp, poo, currentLevel + 1, maxLevel);
                            if (result != collection)
                                return result;
                        }
                        else
                        {
                            if (innerCollection.HitTest(Vector2.Transform(sp, childTransform)))
                            {
                                return innerCollection;
                            }
                        }
                    }


                    if (renderItem.HitTest(Vector2.Transform(sp, childTransform)))
                    {
                        return renderItem;
                    }
                }
            }

            var poo2 = transform;
            var collectionTranform = Win2dUtil.Invert(poo2);

            if (collection.HitTest(Vector2.Transform(sp, collectionTranform)))
                return collection;

            return null;
        }

        private void CanvasOnUpdate(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            InitialCollection.Update();
            _minimap.IsDirty = true;
            _minimap.Update();
            _elementSelectionRenderItem.Update();
        }

        private void CanvasOnDraw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            using(var ds = args.DrawingSession) {
                ds.Clear(Colors.Transparent);
                ds.Transform = Matrix3x2.Identity;
                InitialCollection.Draw(ds);
                ds.Transform = Matrix3x2.Identity;
                _minimap.Draw(ds);
                _elementSelectionRenderItem.Draw(ds);
            }
        }

        public static NuSysRenderer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new NuSysRenderer();
                    }
                }

                return instance;
            }
        }
    }
}
