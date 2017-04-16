using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Windows.Devices.Input;
using Windows.System;
using Windows.UI.Core;
using NusysIntermediate;

namespace NuSysApp
{
    public class CollectionInteractionManager : IDisposable
    {
        public delegate void LinkSelectedHandler(LinkRenderItem element, CanvasPointer point);
        public delegate void TrailSelectedHandler(TrailRenderItem element, CanvasPointer point);
        public delegate void BaseRenderItemSelectedHandler(BaseRenderItem element);
        public delegate void RenderItemSelectedHandler(ElementRenderItem element, CanvasPointer pointer);
        public delegate void InkDrawHandler(CanvasPointer pointer);
        public delegate void LinkCreatedHandler(ElementRenderItem element1, ElementRenderItem element2);
        public delegate void DuplicatedCreated(ElementRenderItem element, Vector2 point);
        public delegate void MarkingMenuPointerReleasedHandler();
        public delegate void RenderItemHandler(BaseRenderItem item, CanvasPointer point);
        public delegate void MarkingMenuPointerMoveHandler(Vector2 p);
        public delegate void TranslateHandler(CanvasPointer pointer, Vector2 point, Vector2 delta);

        public delegate void ResizeHandler(
            CanvasPointer pointer, Vector2 point, Vector2 delta, NodeResizerRenderItem.ResizerPosition resizerPosition);
        public delegate void MovedHandler(CanvasPointer pointer, ElementRenderItem element, Vector2 delta);
        public delegate void PanZoomHandler(Vector2 center, Vector2 deltaTranslation, float deltaZoom);
        public delegate void CollectionSwitchedHandler(CollectionRenderItem collection);
        public delegate void ElementDropHandler(ElementRenderItem element, CollectionRenderItem collection, CanvasPointer pointer);
        public delegate void SelectionInkPressedHandler(CanvasPointer pointer, IEnumerable<Vector2> ink);

        public event ElementDropHandler ElementAddedToCollection;
        public event SelectionInkPressedHandler SelectionInkPressed;
        public event RenderItemSelectedHandler ItemSelected;
        public event LinkSelectedHandler LinkSelected;
        public event TrailSelectedHandler TrailSelected;
        public event RenderItemSelectedHandler MultimediaElementActivated;
        public event BaseRenderItemSelectedHandler DoubleTapped;
        public event MovedHandler ItemMoved;
        public event PanZoomHandler SelectionPanZoomed;
        public event MarkingMenuPointerReleasedHandler SelectionsCleared;
        public event RenderItemHandler RenderItemPressed;
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
        public event ResizeHandler ResizerDragged;
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

        private Dictionary<ElementViewModel, RenderItemTransform> _transformables =
            new Dictionary<ElementViewModel, RenderItemTransform>();

        private BaseRenderItem _selectedRenderItem;
        private BaseRenderItem _secondSelectedRenderItem;
        private CollectionRenderItem _collection;

        private Matrix3x2 _transform = Matrix3x2.Identity;
        private CanvasInteractionManager _canvasInteractionManager;
        private uint _finalInkPointer;
        private DateTime _finalInkPointerUpdated;
        private NodeResizerRenderItem _resizer;
        private bool _isTwoElementsPressed;
        private CanvasPointer _nodeMarkingMenuPointer;
        private Tuple<ElementRenderItem, ElementRenderItem> _potentiaLink;
        private FreeFormViewer _freeFormViewer;

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
            _canvasInteractionManager.PointerWheelChanged += CanvasInteractionManagerOnPointerWheelChanged;
            _freeFormViewer =  SessionController.Instance.SessionView.FreeFormViewer;
        }

        private void CanvasInteractionManagerOnPointerWheelChanged(CanvasPointer pointer, float delta)
        {
            var item = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
            if (!(item is ElementRenderItem))
            {
                return;
            }
            if ((item as ElementRenderItem)?.IsInteractable() == true)
            {
                return;
            }
            var zoomspeed = delta < 0 ? 0.8f : 1.2f;
            PanZoomed?.Invoke(pointer.CurrentPoint, Vector2.Zero, zoomspeed);
        }

        private void CollectionInteractionManagerOnTwoElementsReleased()
        {
            var menu = _freeFormViewer.RenderEngine.NodeMarkingMenu;
            menu.IsVisible = false;
            _canvasInteractionManager.PointerMoved -= CanvasInteractionManagerOnPointerMoved;

            SessionController.Instance.SessionSettings.LinksVisible = LinkVisibilityOption.AllLinks;

            if (menu.CurrentIndex == 0)
                LinkCreated?.Invoke(_potentiaLink.Item1, _potentiaLink.Item2);
            if (menu.CurrentIndex == 1)
                TrailCreated?.Invoke(_potentiaLink.Item1, _potentiaLink.Item2);
        }

        private void CollectionInteractionManagerOnTwoElementsPressed(ElementRenderItem element1, ElementRenderItem element2, CanvasPointer pointer1, CanvasPointer pointer2)
        {
            if (element1 == element2)
                return;
            
            _potentiaLink = new Tuple<ElementRenderItem, ElementRenderItem>(element1, element2);
            _isTwoElementsPressed = true;
            _nodeMarkingMenuPointer = pointer2;
            _freeFormViewer.RenderEngine.NodeMarkingMenu.UpdatePointerLocation(pointer2.CurrentPoint);
            _freeFormViewer.RenderEngine.NodeMarkingMenu.IsVisible = true;
            _freeFormViewer.RenderEngine.NodeMarkingMenu.Show(pointer2.CurrentPoint.X, pointer2.CurrentPoint.Y);

            _canvasInteractionManager.PointerMoved += CanvasInteractionManagerOnPointerMoved;
        }

        private void CanvasInteractionManagerOnPointerMoved(CanvasPointer pointer)
        {
            if (pointer.PointerId == _nodeMarkingMenuPointer.PointerId)
            {
                _freeFormViewer.RenderEngine.NodeMarkingMenu.UpdatePointerLocation(pointer.CurrentPoint);
            }
        }

        private void CanvasInteractionManagerOnTwoPointerPressed(CanvasPointer pointer1, CanvasPointer pointer2)
        {
            var item1 = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer1.CurrentPoint, _collection, 1);
            var item2 = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer2.CurrentPoint, _collection, 1);

            if (item1 == _collection || item2 == _collection)
            {
                return;
            }

            if (item1 is ElementRenderItem && item2 is LinkRenderItem)
            {
                FollowLink(item1 as ElementRenderItem, item2 as LinkRenderItem);
            }

            if (item2 is ElementRenderItem && item1 is LinkRenderItem)
            {
                FollowLink(item2 as ElementRenderItem, item1 as LinkRenderItem);
            }

            if (!(item1 is ElementRenderItem) || !(item2 is ElementRenderItem))
            {
                return;
            }


            CollectionInteractionManagerOnTwoElementsPressed((ElementRenderItem)item1, (ElementRenderItem)item2, pointer1, pointer2);
        }

        private void FollowLink(ElementRenderItem start, LinkRenderItem link)
        {
            var startController = start?.ViewModel?.Controller;
            var linkController = link?.ViewModel?.Controller;
            FollowLink(startController, linkController);
        }

        public void FollowLink(ElementController nodeController, LinkController linkController)
        {
            Debug.Assert(linkController != null);
            Debug.Assert(nodeController != null);
            if (linkController != null && nodeController != null)
            {
                var linkLEM = linkController.LibraryElementController.LibraryElementModel as LinkLibraryElementModel;
                Debug.Assert(linkLEM != null);
                if (linkLEM != null)
                {
                    var otherId = nodeController.Id == linkController.Model.InAtomId ? linkController.Model.OutAtomId : linkController.Model.InAtomId;
                    var endPoint = SessionController.Instance.ElementModelIdToElementController.Values.FirstOrDefault(item => item.Id == otherId);
                    if (endPoint != null)
                    {
                        SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CenterCameraOnElement(endPoint.Id);
                    }
                }
            }
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
            SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.RunOnGameLoopThreadAsync(delegate {
                InkDrawing?.Invoke(pointer);
            });
        }

        private void OnPenPointerPressed(CanvasPointer pointer)
        {
            SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.RunOnGameLoopThreadAsync(delegate {
                InkStarted?.Invoke(pointer);
            });
        }

        private void OnPenPointerReleased(CanvasPointer pointer)
        {
            _canvasInteractionManager.PointerMoved -= OnPenPointerMoved;
            _finalInkPointer = pointer.PointerId;
            _finalInkPointerUpdated = DateTime.Now;
            SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.RunOnGameLoopThreadAsync(delegate {
                InkStopped?.Invoke(pointer);
            });
        }

        private void OnTouchPointerPressed(CanvasPointer pointer)
        {
            if ((pointer.LastUpdated - _finalInkPointerUpdated).TotalMilliseconds < 2000)
            {
                var currentCollection = _freeFormViewer.CurrentCollection;
                var latestStroke = currentCollection.InkRenderItem.LatestStroke;
                if (latestStroke != null && InkUtil.IsPointCloseToStroke(Vector2.Transform(pointer.CurrentPoint, currentCollection.Camera.ScreenToLocalMatrix), latestStroke))
                {
                    SelectionInkPressed?.Invoke(pointer, latestStroke.GetInkPoints().Select(p => new Vector2((float)p.Position.X, (float)p.Position.Y)));
                }
            }

            var failure = _collection?.Camera?.LocalToScreenMatrix == null;
            Debug.Assert(!failure,"do not simply remove this or similiar debug asserts. These will be returned if they fail, but that skips important functionality." +
                                  "TODO: for all similar debug.asserts, figure out origin of issue.");
            if (failure)
            {
                return;
            }

            _transform = _collection.Camera.LocalToScreenMatrix;

            if (_canvasInteractionManager.ActiveCanvasPointers.Count == 1)
            {
                var hit = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
                if (!(hit is BaseInteractiveUIElement))
                {
                    if (_freeFormViewer.RenderEngine.ElementSelectionRect.Resizers.Contains(hit))
                    {
                        _resizer = (NodeResizerRenderItem)hit;
                        if (_resizer != null)
                        {
                            ResizerStarted?.Invoke();
                        }

                    }
                }
                else
                {
                    if ((hit as BaseInteractiveUIElement).IsInteractable())
                    {
                        return;
                    }
                }
                RenderItemPressed?.Invoke(hit, pointer);

                _selectedRenderItem = hit as ElementRenderItem;

            }
            if (_canvasInteractionManager.ActiveCanvasPointers.Count == 2)
            {

                _secondSelectedRenderItem = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1) as ElementRenderItem;
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

            // if the thing we were selecting was a collection render item update it's camera transforms
            // since we may have moved it
            if (_selectedRenderItem is CollectionRenderItem)
            {
                var coll = (CollectionRenderItem)_selectedRenderItem;
                coll.ViewModel.CameraTranslation = new Vector2(coll.Camera.T.M31, coll.Camera.T.M32);
                coll.ViewModel.CameraCenter = new Vector2(coll.Camera.C.M31, coll.Camera.C.M32);
                coll.ViewModel.CameraScale = coll.Camera.S.M11;
            }


            // the currentCollection is the collection that has focus, if we long tap on a collection that will be the current collection
            var currentCollection = _freeFormViewer.CurrentCollection;

            // if the thing we moved is not a "node" or it is the current collection or we have multiple pointers down or ... interactions too long etc
            // then don't do anything
            if (!(_selectedRenderItem is ElementRenderItem) || _selectedRenderItem == currentCollection || _canvasInteractionManager.ActiveCanvasPointers.Count > 0 || pointer.MillisecondsActive < 500 || pointer.DistanceTraveled < 50)
            {
                return;
            }

            // otherwise get a list of the elements which lie under the node that was released
            var hits = _freeFormViewer.RenderEngine.GetRenderItemsAt(pointer.CurrentPoint);

            // get a list of the collections which lie under the thing we released
            var underlyingCollections = hits.OfType<CollectionRenderItem>().ToList();

            // when is the underlying collection count one you make ask?
            // simply when we take a node and drag it out of a currently focused collection onto the main workspace
            if (underlyingCollections.Count() == 1)
            {
                var hit = underlyingCollections.First();
                if (hit != currentCollection && hit != _selectedRenderItem)
                    ElementAddedToCollection?.Invoke((ElementRenderItem) _selectedRenderItem, hit, pointer);
            } else if (underlyingCollections.Count() > 1)
            {
                // if we were dragging a collection remove it from the list of hit items
                while (underlyingCollections.Last() == _selectedRenderItem)
                {
                    underlyingCollections.RemoveAt(underlyingCollections.Count-1);
                }

                // get the last thing we hit, if we dragged over nested collections this would be the inner most collection
                var hit = underlyingCollections.Last();
                var selectedCollection = (_selectedRenderItem as CollectionRenderItem);

                // if the thing we dragged was a collection
                if (selectedCollection != null)
                {

                    // get a list of the render items inside of the collection
                    var renderitems = selectedCollection.GetChildren();
                    if (hit != currentCollection &&  // if the hit is not the collection we are focusing on
                        hit != _selectedRenderItem &&  // and the hit is not the collection we clicked on
                        renderitems != null && // null check the collection's children
                        !renderitems.Contains(hit) // if the thing we hit was not contained in the collection we were dragging
                        /*&& (hit.ViewModel.Model.ParentCollectionId == null || hit.ViewModel.Model.ParentCollectionId == currentCollection.ViewModel.Model.LibraryId)*/)
                        ElementAddedToCollection?.Invoke((ElementRenderItem) _selectedRenderItem, hit, pointer);
                }
                else
                {
                    if (hit != currentCollection && hit != _selectedRenderItem /*&& hit.ViewModel.Model.ParentCollectionId == currentCollection.ViewModel.Model.LibraryId*/)
                    {
                        ElementAddedToCollection?.Invoke((ElementRenderItem) _selectedRenderItem, hit, pointer);
                    }
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
                _selectedRenderItem = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
                _mode = Mode.Link;
                return;
            }

            var keyStateT = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.T);
            if (keyStateT.HasFlag(CoreVirtualKeyStates.Down))
            {
                _selectedRenderItem = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
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
                    _finalInkPointerUpdated = pointer.LastUpdated;
                    SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas.RunOnGameLoopThreadAsync(delegate {
                        InkStopped?.Invoke(pointer);
                    });
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
                    _secondSelectedRenderItem = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection,
                        1);

                    if (_selectedRenderItem != null && _selectedRenderItem != _collection &&
                        _secondSelectedRenderItem != null && _secondSelectedRenderItem != _collection &&
                        _selectedRenderItem != _secondSelectedRenderItem)
                    {
                        if (_selectedRenderItem is ElementRenderItem && _secondSelectedRenderItem is ElementRenderItem)
                        {
                            SessionController.Instance.SessionSettings.LinksVisible = LinkVisibilityOption.AllLinks;
                            LinkCreated?.Invoke((ElementRenderItem)_selectedRenderItem,
                                (ElementRenderItem)_secondSelectedRenderItem);

                        } 
                        else
                        {
                            Debug.Assert(false, "Failed to cast to element render item;");
                        }
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
                    _secondSelectedRenderItem = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

                    var item1 = _selectedRenderItem as ElementRenderItem;
                    var item2 = _secondSelectedRenderItem as ElementRenderItem;

                    if (!(item1 == null || item2 == null || item1 == _collection || item2 == _collection))
                    {
                        SessionController.Instance.SessionSettings.LinksVisible = LinkVisibilityOption.AllLinks;
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
            var element = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

            if (element == _freeFormViewer.CurrentCollection)
                return;

            if (element is CollectionRenderItem)
                CollectionSwitched?.Invoke(element as CollectionRenderItem);
            if (element is VideoElementRenderItem)
                //MultimediaElementActivated?.Invoke(element as VideoElementRenderItem);
                SessionController.Instance.SessionView.FreeFormViewer.PlayFullScreenVideo((element as VideoElementRenderItem).ViewModel.Controller.LibraryElementController as VideoLibraryElementController, true);
            if (element is AudioElementRenderItem)
                MultimediaElementActivated?.Invoke(element as AudioElementRenderItem, pointer);
        }


        private void CanvasInteractionManagerOnAllPointersReleased()
        {
            if (_resizer != null)
            {
                ResizerStopped?.Invoke();
            }
            _resizer = null;

            _selectedRenderItem = null;
            _secondSelectedRenderItem = null;
        }

        private void CanvasInteractionManagerOnItemDoubleTapped(CanvasPointer pointer)
        {
            var element = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);
            BaseRenderItem hit;
            hit = element as ElementRenderItem;
            if (hit == null)
                hit = element as LinkRenderItem;
            if (hit == null)
                return;

            DoubleTapped?.Invoke(hit);

        }

        private void CanvasInteractionManagerOnItemTapped(CanvasPointer pointer)
        {
            var element = _freeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, _collection, 1);

            if (element is NodeMenuButtonRenderItem || element is PseudoElementRenderItem || element is PdfPageButtonRenderItem)
                return;

            if (element is LinkRenderItem)
            {
                LinkSelected?.Invoke((LinkRenderItem) element, pointer);
            }

            if (element is TrailRenderItem)
            {
                TrailSelected?.Invoke((TrailRenderItem)element, pointer);
            }

            var elementRenderItem = element as ElementRenderItem;
            var initialCollection = _freeFormViewer.InitialCollection;
            var currentCollection = _freeFormViewer.CurrentCollection;
            if (elementRenderItem == initialCollection || elementRenderItem == currentCollection || elementRenderItem == null)
            {
                if (element is BaseInteractiveUIElement && !(element is CollectionRenderItem))
                    {
                        return;
                    }
                SelectionsCleared?.Invoke();
                if (element == null)
                    CollectionSwitched?.Invoke(initialCollection);

            }
            else {

                if (_canvasInteractionManager.ActiveCanvasPointers.Count == 0 && pointer.MillisecondsActive < 150)
                {
                    var keyStateShift = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift);
                    if (!keyStateShift.HasFlag(CoreVirtualKeyStates.Down))
                    {
                        SelectionsCleared?.Invoke();
                    }
                }
                ItemSelected?.Invoke(elementRenderItem, pointer);
            }
        }

        private void OnTranslated(CanvasPointer pointer, Vector2 point, Vector2 delta)
        {
            if (_mode != Mode.None)
                return;

            if (_selectedRenderItem == _collection)
            {
                Panned?.Invoke(pointer, point, delta);
            }
            else if (_selectedRenderItem is ElementRenderItem)
            {
                ///This checks if the collection you are editting is editable by the current user
                /// Todo, bring this logic elsewhere
                var parent = SessionController.Instance.ContentController.GetLibraryElementController(((ElementRenderItem)_selectedRenderItem).ViewModel.Model.ParentCollectionId);
                if (parent.LibraryElementModel.AccessType == NusysConstants.AccessType.Public ||
                    parent.LibraryElementModel.Creator == WaitingRoomView.UserID)
                {
                    ItemMoved?.Invoke(pointer, (ElementRenderItem) _selectedRenderItem, delta);
                }
                else
                {
                    Panned?.Invoke(pointer, point, delta);
                }
            }
            else if (_resizer != null)
            {
                ResizerDragged?.Invoke(pointer, point, delta, _resizer.Position);
            }
        }

        private void OnPanZoomed(Vector2 center, Vector2 deltaTranslation, float deltaZoom)
        {
            if (_isTwoElementsPressed)
            {
                return;
            }
            if (ResizeableWindowUIElement.CurrentlyDraggingWindow != null)
            {
                ResizeableWindowUIElement.CurrentlyDraggingWindow.ResizeFromPinch(deltaTranslation,deltaZoom);
                return;
            }

            if (_freeFormViewer.Selections.Count() == 1 && _freeFormViewer.Selections.First().ViewModel == null)
            {
                _freeFormViewer.Selections.Clear();
            }

            if (_freeFormViewer.Selections.Count == 0)
                PanZoomed?.Invoke(center, deltaTranslation, deltaZoom);
            else
            {
                SelectionPanZoomed?.Invoke(center, deltaTranslation, deltaZoom);
            }
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
            _canvasInteractionManager.PointerWheelChanged -= CanvasInteractionManagerOnPointerWheelChanged;
        }
    }
}