using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml.Input;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.Direct2D1;
using Windows.UI.Xaml.Media;
using SharpDX.MediaFoundation;
using System.Collections;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;

namespace NuSysApp
{

    public class Transformable : I2dTransformable
    {
        public Matrix3x2 T { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 S { get; set; } = Matrix3x2.Identity;
        public Matrix3x2 C { get; set; } = Matrix3x2.Identity;

        public Size Size { get; set; }

        public Point Position { get; set; }
        public Vector2 CameraTranslation { get; set; }
        public Vector2 CameraCenter { get; set; }
        public float CameraScale { get; set; }

        public void Update() { }
    }

    public class CollectionInteractionManager : IDisposable
    {
        public delegate void RenderItemSelectedHandler(ElementRenderItem element);
        public delegate void InkDrawHandler(CanvasPointer pointer);
        public delegate void LinkCreatedHandler(ElementRenderItem element1, ElementRenderItem element2);
        public delegate void DuplicatedCreated(ElementRenderItem element, Vector2 point);
        public delegate void MarkingMenuPointerReleasedHandler();
        public delegate void MarkingMenuPointerMoveHandler(Vector2 p);
        public delegate void TranslateHandler(CanvasPointer pointer, Vector2 point, Vector2 delta);
        public delegate void MovedHandler(CanvasPointer pointer, ElementRenderItem element, Vector2 delta);
        public delegate void PanZoomHandler(Vector2 center, Vector2 deltaTranslation, float deltaZoom);
        public delegate void CollectionSwitchedHandler(CollectionRenderItem collection);
        public delegate void ElementDropHandler(ElementRenderItem element, CollectionRenderItem collection, CanvasPointer pointer);
        public delegate void SelectionInkPressedHandler(CanvasPointer pointer, IEnumerable<Vector2> ink);

        public event ElementDropHandler ElementAddedToCollection;
        public event SelectionInkPressedHandler SelectionInkPressed;
        public event RenderItemSelectedHandler ItemSelected;
        public event RenderItemSelectedHandler MultimediaElementActivated;
        public event RenderItemSelectedHandler DoubleTapped;
        public event MovedHandler ItemMoved;
        public event MarkingMenuPointerReleasedHandler SelectionsCleared;
        public event InkDrawHandler InkStarted;
        public event InkDrawHandler InkDrawing;
        public event InkDrawHandler InkStopped;
        public event LinkCreatedHandler LinkCreated;
        public event LinkCreatedHandler TrailCreated;
        public event MarkingMenuPointerReleasedHandler MarkingMenuPointerReleased;
        public event MarkingMenuPointerMoveHandler MarkingMenuPointerMove;
        public event DuplicatedCreated DuplicateCreated;
        public event PanZoomHandler PanZoomed;
        public event TranslateHandler Panned;
        public event CollectionSwitchedHandler CollectionSwitched;
        public event TranslateHandler ResizerDragged;
        public event MarkingMenuPointerReleasedHandler ResizerStarted;
        public event MarkingMenuPointerReleasedHandler ResizerStopped;

        private enum Mode
        {
            PanZoom,
            Ink,
            MoveNode,
            OutOfBounds,
            Link,
            None,
            Trail
        }

        private Mode _mode = Mode.None;

        private Dictionary<ElementViewModel, Transformable> _transformables =
            new Dictionary<ElementViewModel, Transformable>();

        private BaseRenderItem _selectedRenderItem;
        private BaseRenderItem _secondSelectedRenderItem;
        private CollectionRenderItem _collection;

        private Matrix3x2 _transform = Matrix3x2.Identity;
        private CanvasInteractionManager _canvasInteractionManager;
        private CanvasPointer _finalInkPointer;
        private bool _resizerHit;
        private bool _isTwoElementsPressed;
        private CanvasPointer _nodeMarkingMenuPointer;
        private Tuple<ElementRenderItem, ElementRenderItem> _potentiaLink; 


        public ObservableCollection<ElementRenderItem> Selections { get; set; } = new ObservableCollection<ElementRenderItem>();

        public CollectionInteractionManager(CanvasInteractionManager canvasInteractionManager, CollectionRenderItem collection)
        {
            _collection = collection;
            _canvasInteractionManager = canvasInteractionManager;
            _canvasInteractionManager.PointerPressed += OnPointerPressed;
            _canvasInteractionManager.PointerReleased += OnPointerReleased;
            _canvasInteractionManager.PanZoomed += OnPanZoomed;
            _canvasInteractionManager.Translated += OnTranslated;
            _canvasInteractionManager.ItemTapped += CanvasInteractionManagerOnItemTapped;
            _canvasInteractionManager.ItemLongTapped += CanvasInteractionManagerOnItemLongTapped;
            _canvasInteractionManager.ItemDoubleTapped += CanvasInteractionManagerOnItemDoubleTapped;
            _canvasInteractionManager.AllPointersReleased += CanvasInteractionManagerOnAllPointersReleased;
            _canvasInteractionManager.TwoPointerPressed += CanvasInteractionManagerOnTwoPointerPressed;
        }

        private void CollectionInteractionManagerOnTwoElementsReleased()
        {
            var menu = NuSysRenderer.Instance.NodeMarkingMenu;
            menu.IsVisible = false;
            _canvasInteractionManager.PointerMoved -= CanvasInteractionManagerOnPointerMoved;
            if (menu.CurrentIndex == 0)
                LinkCreated?.Invoke(_potentiaLink.Item1, _potentiaLink.Item2);
            if (menu.CurrentIndex == 1)
                TrailCreated?.Invoke(_potentiaLink.Item1, _potentiaLink.Item2);
        }

        private void CollectionInteractionManagerOnTwoElementsPressed(ElementRenderItem element1, ElementRenderItem element2, CanvasPointer pointer1, CanvasPointer pointer2)
        {
            _potentiaLink = new Tuple<ElementRenderItem, ElementRenderItem>(element1, element2);
            _isTwoElementsPressed = true;
            _nodeMarkingMenuPointer = pointer2;
            NuSysRenderer.Instance.NodeMarkingMenu.UpdatePointerLocation(pointer2.CurrentPoint);
            NuSysRenderer.Instance.NodeMarkingMenu.IsVisible = true;
            NuSysRenderer.Instance.NodeMarkingMenu.Show(pointer2.CurrentPoint.X, pointer2.CurrentPoint.Y);

            _canvasInteractionManager.PointerMoved += CanvasInteractionManagerOnPointerMoved;
        }

        private void CanvasInteractionManagerOnPointerMoved(CanvasPointer pointer)
        {
            if (pointer == _nodeMarkingMenuPointer)
            {
                NuSysRenderer.Instance.NodeMarkingMenu.UpdatePointerLocation(pointer.CurrentPoint);
            }
        }

        private void CanvasInteractionManagerOnTwoPointerPressed(CanvasPointer pointer1, CanvasPointer pointer2)
        {
            var item1 = NuSysRenderer.Instance.GetRenderItemAt(pointer1.CurrentPoint, _collection, 1);
            var item2 = NuSysRenderer.Instance.GetRenderItemAt(pointer2.CurrentPoint, _collection, 1);
            if (!(item1 is ElementRenderItem) || !(item2 is ElementRenderItem))
                return;

            if (item1 == _collection || item2 == _collection)
                return;

            CollectionInteractionManagerOnTwoElementsPressed((ElementRenderItem)item1, (ElementRenderItem)item2, pointer1, pointer2);
        }

        private void OnPointerPressed(CanvasPointer pointer)
        {
            if (pointer.DeviceType == PointerDeviceType.Touch)
                OnTouchPointerPressed(pointer);

            if (pointer.DeviceType == PointerDeviceType.Mouse)
                OnMousePointerPressed(pointer);

            if (pointer.DeviceType == PointerDeviceType.Pen) { 
                OnPenPointerPressed(pointer);
                _canvasInteractionManager.PointerMoved += OnPenPointerMoved;
            }
        }

        private void OnPointerMoved(CanvasPointer pointer)
        {
            if (pointer.DeviceType == PointerDeviceType.Mouse)
                OnMousePointerMoved(pointer);
        }

        private void OnPointerReleased(CanvasPointer pointer)
        {
            if (pointer.DeviceType == PointerDeviceType.Touch)
                OnTouchPointerReleased(pointer);

            if (pointer.DeviceType == PointerDeviceType.Mouse)
                OnMousePointerReleased(pointer);

            if (pointer.DeviceType == PointerDeviceType.Pen)
            {
                OnPenPointerReleased(pointer);
                
            }
                
        }

        private void OnPenPointerMoved(CanvasPointer pointer)
        {
            InkDrawing?.Invoke(pointer);
        }

        private void OnPenPointerPressed(CanvasPointer pointer)
        {
            InkStarted?.Invoke(pointer);
        }

        private void OnPenPointerReleased(CanvasPointer pointer)
        {
            _canvasInteractionManager.PointerMoved -= OnPenPointerMoved;
            _finalInkPointer = pointer;
            InkStopped?.Invoke(pointer);
        }

        private void OnTouchPointerPressed(CanvasPointer pointer)
        {
            if (_finalInkPointer != null && (pointer.LastUpdated - _finalInkPointer.LastUpdated).TotalMilliseconds < 2000)
            {
                var currentCollection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;
                var latestStroke = currentCollection.InkRenderItem.LatestStroke;
                var t = Win2dUtil.Invert(NuSysRenderer.Instance.GetCollectionTransform(currentCollection));
                if (InkUtil.IsPointCloseToStroke(Vector2.Transform(pointer.CurrentPoint, t), latestStroke))
                {
                    SelectionInkPressed?.Invoke(pointer, latestStroke.GetInkPoints().Select(p => new Vector2((float)p.Position.X, (float)p.Position.Y)));
                }
            }

            var until = NuSysRenderer.Instance.GetTransformUntil(_collection);
            _transform = Win2dUtil.Invert(_collection.C) * _collection.S * _collection.C * _collection.T * until;

            if (_canvasInteractionManager.ActiveCanvasPointers.Count == 1)
            {
                var hit = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
                if (hit == NuSysRenderer.Instance.ElementSelectionRenderItem.Resizer)
                {
                    _resizerHit = true;
                    if (_resizerHit)
                    {
                        ResizerStarted?.Invoke();
                    }

                }
                _selectedRenderItem = hit as ElementRenderItem;
                if (_selectedRenderItem != null && _selectedRenderItem.Parent != null && _selectedRenderItem != SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection)
                    _selectedRenderItem.Parent.BringForward(_selectedRenderItem as ElementRenderItem);
            }
            if (_canvasInteractionManager.ActiveCanvasPointers.Count == 2)
            {

                _secondSelectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1) as ElementRenderItem;
                _transformables.Clear();

                if (_canvasInteractionManager.ActiveCanvasPointers[0].MillisecondsActive > 300)
                {
                    if (_selectedRenderItem != null && _selectedRenderItem != _collection && _secondSelectedRenderItem == _collection)
                    {
                        DuplicateCreated?.Invoke((ElementRenderItem)_selectedRenderItem, _canvasInteractionManager.ActiveCanvasPointers[1].CurrentPoint);
                    }
                }
            }
        }

        private void OnTouchPointerReleased(CanvasPointer pointer)
        {
            if (_isTwoElementsPressed)
            {
                _isTwoElementsPressed = false;
                CollectionInteractionManagerOnTwoElementsReleased();
            }

            if (_selectedRenderItem is CollectionRenderItem)
            {
                var coll = (CollectionRenderItem)_selectedRenderItem;
                coll.ViewModel.CameraTranslation = new Vector2(coll.Camera.T.M31, coll.Camera.T.M32);
                coll.ViewModel.CameraCenter = new Vector2(coll.Camera.C.M31, coll.Camera.C.M32);
                coll.ViewModel.CameraScale = coll.Camera.S.M11;
            }

            var currentCollection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;
            if (!(_selectedRenderItem is ElementRenderItem) || _selectedRenderItem == currentCollection || _canvasInteractionManager.ActiveCanvasPointers.Count > 0 || pointer.MillisecondsActive < 500 || pointer.DistanceTraveled < 50)
            {
                return;
            }


            var hits = NuSysRenderer.Instance.GetRenderItemsAt(pointer.CurrentPoint);
            var underlyingCollections = hits.OfType<CollectionRenderItem>().ToList();
            if (underlyingCollections.Count() == 1)
            {
                var hit = underlyingCollections.First();
                if (hit != currentCollection && hit != _selectedRenderItem)
                    ElementAddedToCollection?.Invoke((ElementRenderItem) _selectedRenderItem, hit, pointer);
            } else if (underlyingCollections.Count() > 1)
            {
                while (underlyingCollections.Last() == _selectedRenderItem)
                {
                    underlyingCollections.RemoveAt(underlyingCollections.Count-1);
                }
                var hit = underlyingCollections.Last();
                var selectedCollection = (_selectedRenderItem as CollectionRenderItem);
                if (selectedCollection != null)
                {
                    var renderitems = selectedCollection.GetRenderItems();
                    if (hit != currentCollection && hit != _selectedRenderItem && renderitems != null &&
                        !renderitems.Contains(hit))
                        ElementAddedToCollection?.Invoke((ElementRenderItem) _selectedRenderItem, hit, pointer);
                }
                else
                {
                    if (hit != currentCollection && hit != _selectedRenderItem)
                        ElementAddedToCollection?.Invoke((ElementRenderItem)_selectedRenderItem, hit, pointer);
                }
            }       
        }

        private void OnMousePointerPressed(CanvasPointer pointer)
        {
            var keyStateI = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.I);
            if (keyStateI.HasFlag(CoreVirtualKeyStates.Down))
            {
                _mode = Mode.Ink;
                InkStarted?.Invoke(pointer);
                _canvasInteractionManager.PointerMoved += OnPointerMoved;
                return;
            }

            var keyStateL = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.L);
            if (keyStateL.HasFlag(CoreVirtualKeyStates.Down))
            {
                _selectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
                _mode = Mode.Link;
                return;
            }

            var keyStateT = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.T);
            if (keyStateT.HasFlag(CoreVirtualKeyStates.Down))
            {
                _selectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
                _mode = Mode.Trail;
                return;
            }


            OnTouchPointerPressed(pointer);
        }

        private void OnMousePointerReleased(CanvasPointer pointer)
        {
            if (_mode == Mode.Ink)
            {
                var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.I);

                if (keyState.HasFlag(CoreVirtualKeyStates.Down))
                {
                    _finalInkPointer = pointer;
                    InkStopped?.Invoke(pointer);
                    _canvasInteractionManager.PointerMoved -= OnPointerMoved;
                }
                _mode = Mode.None;
                return;
            }

            if (_mode == Mode.Link)
            {
                var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.L);

                if (keyState.HasFlag(CoreVirtualKeyStates.Down))
                {
                    _secondSelectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

                    if (_selectedRenderItem != null && _selectedRenderItem != _collection &&
                        _secondSelectedRenderItem != null && _secondSelectedRenderItem != _collection)
                    {
                        LinkCreated?.Invoke((ElementRenderItem) _selectedRenderItem, (ElementRenderItem) _secondSelectedRenderItem);
                    }
                }
                _mode = Mode.None;

                return;
            }

            if (_mode == Mode.Trail)
            {
                var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.T);

                if (keyState.HasFlag(CoreVirtualKeyStates.Down))
                {
                    _secondSelectedRenderItem = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

                    if (_selectedRenderItem != null && _selectedRenderItem != _collection &&
                        _secondSelectedRenderItem != null && _secondSelectedRenderItem != _collection)
                    {
                        TrailCreated?.Invoke((ElementRenderItem)_selectedRenderItem, (ElementRenderItem)_secondSelectedRenderItem);
                    }
                }
                _mode = Mode.None;

                return;
            }

            OnTouchPointerReleased(pointer);
           
        }

        private void OnMousePointerMoved(CanvasPointer pointer)
        {
            if (_mode == Mode.Ink)
            {
                InkDrawing?.Invoke(pointer);
                return;
            }
        }

        private void CanvasInteractionManagerOnItemLongTapped(CanvasPointer pointer)
        {
            var element = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

            if (element == SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection)
                return;

            if (element is CollectionRenderItem)
                CollectionSwitched?.Invoke(element as CollectionRenderItem);
            if (element is VideoElementRenderItem)
                MultimediaElementActivated?.Invoke(element as VideoElementRenderItem);
            if (element is AudioElementRenderItem)
                MultimediaElementActivated?.Invoke(element as AudioElementRenderItem);
        }


        private void CanvasInteractionManagerOnAllPointersReleased()
        {
            if (_resizerHit)
            {
                ResizerStopped?.Invoke();
            }
            _resizerHit = false;

            _selectedRenderItem = null;
            _secondSelectedRenderItem = null;
        }

        private void CanvasInteractionManagerOnItemDoubleTapped(CanvasPointer pointer)
        {
            var element = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
            var elementRenderItem = element as ElementRenderItem;
            if (elementRenderItem == null)
                return;

            DoubleTapped?.Invoke(elementRenderItem);
        }

        private void CanvasInteractionManagerOnItemTapped(CanvasPointer pointer)
        {
            var element = NuSysRenderer.Instance.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

            if (element is NodeMenuButtonRenderItem)
                return;

            var elementRenderItem = element as ElementRenderItem;
            var initialCollection = SessionController.Instance.SessionView.FreeFormViewer.InitialCollection;
            var currentCollection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;
            if (elementRenderItem == initialCollection || elementRenderItem == currentCollection || elementRenderItem == null)
            {
                SelectionsCleared?.Invoke();
                /*
                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Source = null;
                SessionController.Instance.SessionView.FreeFormViewer.VideoPlayer.Visibility = Visibility.Collapsed;
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.AudioSource = null;
                SessionController.Instance.SessionView.FreeFormViewer.AudioPlayer.Visibility = Visibility.Collapsed;
                */
                if (element == null)
                    CollectionSwitched?.Invoke(initialCollection);

            }
            else {

                if (_canvasInteractionManager.ActiveCanvasPointers.Count == 0 && pointer.MillisecondsActive < 150)
                {
                    SelectionsCleared?.Invoke();
                }
                ItemSelected?.Invoke(elementRenderItem);
            }
        }

        private void OnTranslated(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            if (_mode != Mode.None)
                return;

            if (_selectedRenderItem == _collection)
                Panned?.Invoke(pointer, point, delta);
            else if (_selectedRenderItem is ElementRenderItem)
                ItemMoved?.Invoke(pointer, (ElementRenderItem)_selectedRenderItem, delta);
            else if (_resizerHit)
            {
                ResizerDragged?.Invoke(pointer, point, delta);
            }
        }

        private void OnPanZoomed(Vector2 center, Vector2 deltaTranslation, float deltaZoom)
        {
            if (!_isTwoElementsPressed)
                PanZoomed?.Invoke(center, deltaTranslation, deltaZoom);
        }

        public void Dispose()
        {
            _canvasInteractionManager.PointerPressed -= OnPointerPressed;
            _canvasInteractionManager.PointerReleased -= OnPointerReleased;
            _canvasInteractionManager.PanZoomed -= OnPanZoomed;
            _canvasInteractionManager.Translated -= OnTranslated;
            _canvasInteractionManager.ItemTapped -= CanvasInteractionManagerOnItemTapped;
            _canvasInteractionManager.ItemLongTapped -= CanvasInteractionManagerOnItemLongTapped;
            _canvasInteractionManager.ItemDoubleTapped -= CanvasInteractionManagerOnItemDoubleTapped;
            _canvasInteractionManager.AllPointersReleased -= CanvasInteractionManagerOnAllPointersReleased;
            _canvasInteractionManager.TwoPointerPressed -= CanvasInteractionManagerOnTwoPointerPressed;
            _canvasInteractionManager.PointerMoved -= OnPenPointerMoved;
            _canvasInteractionManager.PointerMoved -= CanvasInteractionManagerOnPointerMoved;
        }
    }
}