﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using MyToolkit.Utilities;
using NusysIntermediate;
using Wintellect.PowerCollections;

namespace NuSysApp
{
    public class CollectionRenderItem : ElementRenderItem
    {
        private CanvasAnimatedControl _canvas;
        private Size _elementSize;
        private CanvasGeometry _shape;
        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed,
        };

        protected List<BaseRenderItem> _renderItems0 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems1 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems2 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems3 = new List<BaseRenderItem>();

        public InkRenderItem InkRenderItem { get; set; }
        public ElementCollectionViewModel ViewModel;
        public CanvasGeometry Mask { get; set; }
        private List<BaseRenderItem> _allRenderItems = new List<BaseRenderItem>();

        public RenderItemTransform Camera { get; set; } = new RenderItemTransform();
        
        public int NumLinks => ViewModel.Links.Count;

        private CollectionRenderItem _parent;
  
        /// <summary>
        /// ListView and its mask will be null when inactive.
        /// </summary>
        private ListViewUIElementContainer<string> _listView;
        private RectangleUIElement _listViewMask;



        public CollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi canvas, bool interactionEnabled = false) : base(vm, parent, canvas)
        {
            _canvas = (CanvasAnimatedControl)ResourceCreator;
            ViewModel = vm;
            var collectionController = (ElementCollectionController)vm.Controller;
            collectionController.CameraPositionChanged += OnCameraPositionChanged;
            collectionController.CameraCenterChanged += OnCameraCenterChanged;

            if (!interactionEnabled)
                Transform.LocalPosition = new Vector2((float)vm.X, (float)vm.Y);

            Camera.SetParent(Transform);
            Camera.LocalPosition = vm.CameraTranslation;
            Camera.LocalScaleCenter = vm.CameraCenter;
            Camera.LocalScale = new Vector2(vm.CameraScale, vm.CameraScale);

            vm.Elements.CollectionChanged += OnElementsChanged;
            vm.Links.CollectionChanged += OnElementsChanged;
            vm.Trails.CollectionChanged += OnElementsChanged;
            vm.AtomViewList.CollectionChanged += OnElementsChanged;

            _parent = parent;
           
        }

        public override void Dispose()
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (IsDisposed)
                    return;

                var collectionController = (ElementCollectionController) ViewModel.Controller;
                collectionController.CameraPositionChanged -= OnCameraPositionChanged;
                collectionController.CameraCenterChanged -= OnCameraCenterChanged;

                ViewModel.Elements.CollectionChanged -= OnElementsChanged;
                ViewModel.Links.CollectionChanged -= OnElementsChanged;
                ViewModel.Trails.CollectionChanged -= OnElementsChanged;
                ViewModel.AtomViewList.CollectionChanged -= OnElementsChanged;

                foreach (var item in _renderItems0.ToArray())
                    Remove(item);

                foreach (var item in _renderItems1.ToArray())
                    Remove(item);

                foreach (var item in _renderItems2.ToArray())
                    Remove(item);

                foreach (var item in _renderItems3.ToArray())
                    Remove(item);

                _renderItems0.Clear();
                _renderItems1.Clear();
                _renderItems2.Clear();
                _renderItems3.Clear();

                InkRenderItem.Dispose();
                InkRenderItem = null;

                ViewModel = null;
                Camera = null;

                base.Dispose();
            });
        }

        public async override Task Load()
        {
            GameLoopSynchronizationContext.RunOnGameLoopThreadAsync(_canvas, async () =>
            {
                if (IsDisposed)
                    return;
                
                await base.Load();

                var collectionController = (ElementCollectionController) ViewModel.Controller;
                if (!collectionController.LibraryElementController.ContentLoaded)
                {
                    await collectionController.LibraryElementController.LoadContentDataModelAsync();
                }

                InkRenderItem = new InkRenderItem(this, _canvas);
                InkRenderItem.Load();
                _renderItems0.Add(InkRenderItem);

                var strokes = collectionController.LibraryElementController.ContentDataController.ContentDataModel.Strokes;
                foreach (var inkModel in strokes)
                {
                    InkRenderItem.AddInkModel(inkModel);
                }
                
                foreach (var elementViewModel in ViewModel.Elements.ToArray())
                {
                    AddItem(elementViewModel);
                }

                foreach (var linkViewModel in ViewModel.Links.ToArray())
                {
                    AddItem(linkViewModel);
                }


                foreach (var tailViewModel in ViewModel.Trails.ToArray())
                {
                    AddItem(tailViewModel);
                }

            });
        }

        public override void AddChild(BaseRenderItem child)
        {
            child.Transform.SetParent(Camera);
            _children.Add(child);
        }

        public void RemoveLink(string libraryElementId)
        {
            var soughtLinks = ViewModel.Links.Where(l => l.Controller.LibraryElementController.LibraryElementModel.LibraryElementId == libraryElementId);
            if (!soughtLinks.Any())
            {
                return;
            }

            ViewModel.Links.Remove(soughtLinks.First());
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _allRenderItems = _renderItems3.Concat(_renderItems2).Concat(_renderItems1).Concat(_renderItems0).ToList();
            });
        }

        public void BringForward(ElementRenderItem item)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _renderItems2.Remove(item);
                _renderItems2.Insert(_renderItems2.Count, item);
            });
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            /*
             * Update dimensions of listview and its mask, if applicable
             */
            if (_listView!=null && ViewModel!=null && _listViewMask!=null)
            {  
                _listView.Width = (float) ViewModel.Width;
                _listView.Height = (float) ViewModel.Height;
                _listViewMask.Width = (float)ViewModel.Width;
                _listViewMask.Height = (float)ViewModel.Height;
            }

            _elementSize = new Size(ViewModel.Width, ViewModel.Height);

            Transform.Update(parentLocalToScreenTransform);
            base.Update(parentLocalToScreenTransform);
            Camera.Update(Transform.LocalToScreenMatrix);

            foreach (var item in _renderItems0.ToArray())
                item?.Update(Camera.LocalToScreenMatrix);

            foreach (var item in _renderItems1.ToArray())
                item?.Update(Camera.LocalToScreenMatrix);

            foreach (var item in _renderItems2.ToArray())
                item?.Update(Camera.LocalToScreenMatrix);

            foreach (var item in _renderItems3.ToArray())
                item?.Update(Camera.LocalToScreenMatrix);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            ds.Transform = Transform.LocalToScreenMatrix;

          

            var initialCollection = SessionController.Instance.SessionView.FreeFormViewer.InitialCollection;
            var currentCollection = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection;


            Color borderColor;
            Color bgColor;
            float borderWidth = 4f;

            var elementRect = new Rect(0, 0, _elementSize.Width, _elementSize.Height);
            if (!(ViewModel.IsFinite && ViewModel.IsShaped)) { 
                _strokeStyle.TransformBehavior = CanvasStrokeTransformBehavior.Hairline;
                ds.DrawRectangle(elementRect, Colors.Black, 1f, _strokeStyle);
            }

            if (ViewModel.IsShaped || ViewModel.IsFinite)
            {
                var model = (CollectionLibraryElementModel)ViewModel.Controller.LibraryElementModel;
                var pts = model.ShapePoints.Select(p => new Vector2((float)p.X, (float)p.Y)).ToArray();
                _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts);

            }


            if (this != initialCollection && ViewModel.IsFinite && ViewModel.IsShaped)
            {
                var scaleFactor = (float)_elementSize.Width / (float)_shape.ComputeBounds().Width;
                Camera.LocalScale = new Vector2(scaleFactor);
                ds.Transform = Camera.LocalToScreenMatrix;
                Mask = _shape;
            }
            else if (this != initialCollection && ViewModel.IsFinite && !ViewModel.IsShaped)
            {
                var bounds = _shape.ComputeBounds();
                var scaleFactor = (float)_elementSize.Height / (float)_shape.ComputeBounds().Height;
                Camera.LocalScale = new Vector2(scaleFactor);
                ds.Transform = Transform.LocalToScreenMatrix;
                Mask = CanvasGeometry.CreateRectangle(ResourceCreator, elementRect);
            }
            else
            {
                ds.Transform = Transform.LocalToScreenMatrix;
                Mask = CanvasGeometry.CreateRectangle(ResourceCreator, elementRect);
            }



            using (ds.CreateLayer(1f, Mask))
            {
                ds.Transform = Transform.LocalToScreenMatrix;
                ds.FillRectangle(GetLocalBounds(), Colors.White);

                ds.Transform = Camera.LocalToScreenMatrix;
                if (ViewModel.IsShaped)
                {
                    ds.FillGeometry(_shape, ViewModel.ShapeColor);
     
                }

                var screenRect = GetScreenRect();

                foreach (var item in _renderItems0.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems1?.ToArray() ?? new BaseRenderItem[0])
                    item.Draw(ds);

                foreach (var item in _renderItems2?.ToArray() ?? new BaseRenderItem[0])
                {
                    var element = item as ElementRenderItem;
                    if (element == null)
                        return;

                    if (element is PseudoElementRenderItem || screenRect.Intersects(element.GetScreenRect()))
                        item.Draw(ds);
                 }

                foreach (var item in _renderItems3?.ToArray() ?? new BaseRenderItem[0])
                    item.Draw(ds);

               
                if (initialCollection != currentCollection && this == initialCollection)
                {
                    var canvasSize = (ResourceCreator as CanvasAnimatedControl).Size;
                    CanvasGeometry crop;

                    Matrix3x2 tt;

                    if (currentCollection.ViewModel.IsFinite && currentCollection.ViewModel.IsShaped)
                    {
                        tt = currentCollection.Camera.LocalToScreenMatrix;
                    }
                    else
                    {
                        tt = currentCollection.Transform.LocalToScreenMatrix;
                    }

                    crop = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, canvasSize.Width, canvasSize.Height)).CombineWith(currentCollection.Mask, tt, CanvasGeometryCombine.Xor);

                    ds.Transform = Matrix3x2.Identity;
                    ds.FillGeometry(crop, Color.FromArgb(0x77, 0, 0, 0));
                    ds.Transform = Camera.LocalToScreenMatrix;
                    currentCollection.Draw(ds);
                }
            }

            base.Draw(ds);


        }

        public override void CreateResources()
        {
            foreach (var item in _renderItems0)
                item.CreateResources();

            foreach (var item in _renderItems1)
                item.CreateResources();

            foreach (var item in _renderItems2)
                item.CreateResources();

            foreach (var item in _renderItems3)
                item.CreateResources();
        }

        public void Remove(BaseRenderItem item)
        {
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                if (_renderItems0.Contains(item))
                    _renderItems0.Remove(item);
                if (_renderItems1.Contains(item))
                    _renderItems1.Remove(item);
                if (_renderItems2.Contains(item))
                    _renderItems2.Remove(item);
                if (_renderItems3.Contains(item))
                    _renderItems3.Remove(item);

                item.Dispose();

                _canvas.RunOnGameLoopThreadAsync(() =>
                {
                    _allRenderItems = _renderItems3.Concat(_renderItems2).Concat(_renderItems1).Concat(_renderItems0).ToList();
                });

            });
        }

        public Matrix3x2 GetCameraTransform()
        {
            return Win2dUtil.Invert(Camera.C) * Camera.S * Camera.C * Camera.T;
        }

        public override List<BaseRenderItem> GetChildren()
        {
            _allRenderItems = _renderItems0.Concat(_renderItems1).Concat(_renderItems2).Concat(_renderItems3).ToList();
            return _allRenderItems;
        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            /*
             * TODO: FIGURE THIS SHIT OUT, why can't i just override this?
             */
            if (_listView != null)
            {
                var ht = _listView.HitTest(screenPoint);
                if (ht != null)
                {
                    return ht;
                }
            }
            if (ViewModel is FreeFormViewerViewModel)
            {
                return this;
            }

            return base.HitTest(screenPoint);
        }

        private void OnCameraCenterChanged(float f, float f1)
        {
            Camera.LocalScaleCenter = new Vector2(f, f1);
        }

        private void OnCameraPositionChanged(float f, float f1)
        {
            Camera.LocalPosition = new Vector2(f,f1);
        }

        private async void OnElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _canvas.RunOnGameLoopThreadAsync(async () =>
            {
                if (IsDisposed)
                    return;

                /*
                 * RenderItems0/1/2...those are just to make sure elements are on the proper layer
                 */
                if (e.Action == NotifyCollectionChangedAction.Reset)
                {
                    foreach (var baseRenderItem in _renderItems0)
                        Remove(baseRenderItem);
                    foreach (var baseRenderItem in _renderItems1)
                        Remove(baseRenderItem);
                    foreach (var baseRenderItem in _renderItems2)
                        Remove(baseRenderItem);
                    foreach (var baseRenderItem in _renderItems3)
                        Remove(baseRenderItem);

                }

                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var newItem in e.NewItems)
                    {
                        AddItem(newItem);
                    }
                }
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var oldItem in e.OldItems)
                    {
                        RemoveItem(oldItem);
                    }
                }
            
            });
        }

        private async Task AddItem(object vm)
        {
            BaseRenderItem item = null;
            if (vm is ToolFilterView || vm is BaseToolView || vm is MetadataToolView)
            {
                await UITask.Run(async delegate
                {
                    _renderItems2?.Add(new PseudoElementRenderItem((ITool)vm, this, ResourceCreator));
                });
            }
            else if (vm is TextNodeViewModel)
            {
                item = new TextElementRenderItem((TextNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is ImageElementViewModel)
            {
                item = new ImageElementRenderItem((ImageElementViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is PdfNodeViewModel)
            {
                item = new PdfElementRenderItem((PdfNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is AudioNodeViewModel)
            {
                item = new AudioElementRenderItem((AudioNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is VideoNodeViewModel)
            {
                item = new VideoElementRenderItem((VideoNodeViewModel)vm, this, ResourceCreator);
                item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is ElementCollectionViewModel)
            {
                item = new CollectionRenderItem((ElementCollectionViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
   
            else if (vm is LinkViewModel)
            {
                item = new LinkRenderItem((LinkViewModel) vm, this, ResourceCreator);
                _renderItems1.Add(item);
            }
            else if (vm is PresentationLinkViewModel)
            {
                item = new TrailRenderItem((PresentationLinkViewModel)vm, this, ResourceCreator);
                _renderItems1.Add(item);
            }
            /*
             * Zach's addition
             */
            else if (vm is RectangleUIElement)
            {
                /*
                 * TODO: WTF IS GOING ON, HOW TO DO I GET THE RECTANGLE TO SHOW UP!!!
                 */
                var rect = vm as RectangleUIElement;
                //item.Transform.Translate((float) ViewModel.X,(float)ViewModel.Y);
                rect.Height = 500000;
                rect.Width = 500000;
                //rect.Background = Colors.Blue;
                var foo = GetCenterOnScreen();
                var fa = this.ViewModel.Anchor;
                //rect.Transform.Translate((float)fa.X,(float)fa.Y);
                await rect.Load();
                _renderItems2?.Add(rect);
            }

            item?.Transform.SetParent(Camera);

            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _allRenderItems = _renderItems3.Concat(_renderItems2).Concat(_renderItems1).Concat(_renderItems0).ToList();
            });
        
        }

        private void RemoveItem(object item)
        {

            if (item is ElementViewModel) { 
                var renderItem = _renderItems2.OfType<ElementRenderItem>().Where(el => el.ViewModel == item);
                if (renderItem.Any())
                {
                    var element = renderItem.First();
                    Remove(element);
                }
            }

            if (item is LinkViewModel)
            {
                var linkItem = _renderItems1.OfType<LinkRenderItem>().Where(el => el.ViewModel == item);
                if (linkItem.Any())
                {
                    var link = linkItem.First();
                    Remove(link);
                }
            }

            if (item is PresentationLinkViewModel)
            {
                var trailItems = _renderItems1.OfType<TrailRenderItem>().Where(el => el.ViewModel == item);
                if (trailItems.Any())
                {
                    var trail = trailItems.First();
                    Remove(trail);
                }
            }
            if (item is ITool)
            {
                var toolItem = _renderItems2.OfType<PseudoElementRenderItem>().Where(el => el.Tool == item);
                if (toolItem.Any())
                {
                    Remove(toolItem.First());
                }
            }
        }

        /// <summary>
        /// Toggles the presence of the list view and its mask
        /// </summary>
        /// <param name="cids"></param>
        public void ToggleListView(List<string> cids)
        {
            if (_listView == null && _listViewMask==null)
            {
                SetUpListViewMask(_parent,_canvas);
                SetUpListView(_parent, _canvas, null, cids);
                
            }
            else
            {
               RemoveListViewAndMask();
            }
        }

        /// <summary>
        /// Enums to be used in column generation for the list view
        /// </summary>
        private enum ColumnType
        {
            Title,Type,Creator
        }

        /// <summary>
        /// Sets up the mask behind the list view
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        private void SetUpListViewMask(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            _listViewMask = new RectangleUIElement(parent, resourceCreator);
            _listViewMask.Width = (float)ViewModel.Width;
            _listViewMask.Height = (float)ViewModel.Height;
            //_listViewMask.Background = Colors.White;
            _listViewMask.Background = Colors.Fuchsia;
            AddChild(_listViewMask);
            //AddItem(_listViewMask);
        }

        /// <summary>
        /// Graphically removes list view and mask and sets variables to null
        /// </summary>
        private void RemoveListViewAndMask()
        {
            Remove(_listView);
            Remove(_listViewMask);
            _listView = null;
            _listViewMask = null;
        }

        /// <summary>
        /// Sets up the list UI element that will store information for all the children
        /// </summary>
        private void SetUpListView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller, List<string> childElementIds)
        {
            /*
             * Instanatiate new ListView UI component and set some basic properties
             */
            _listView = new ListViewUIElementContainer<string>(parent, resourceCreator);
            _listView.ShowHeader = true;
            _listView.Width = (float)ViewModel.Width;
            _listView.Height = (float)ViewModel.Height;         

            /*
             * Set up the columns by using a helper function
             */
            var cols = new List<ListColumn<string>>();
            cols.Add(this.GenerateListTextColumn("Title", 2, ColumnType.Title));
            cols.Add(this.GenerateListTextColumn("Type", 1, ColumnType.Type));
            cols.Add(this.GenerateListTextColumn("Creator", 1, ColumnType.Creator));
            _listView.AddColumns(cols);
            _listView.Transform.LocalPosition = new Vector2(0, 0);

            /*
             * Add the child element IDs as items so that the column functions can use them to get 
             * relevant information 
             */
            _listView.AddItems(childElementIds);

            /*
             * Add it as a child of the CRI and to a layout mgr to format 
             * its size and location (jk, no layout mgr here)
             */
            AddChild(_listView);
            //AddItem(_listView);

        }

        /// <summary>
        /// Returns a list text column based on the passed in specifications.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="relWidth"></param>
        /// <param name="colType"></param>
        /// <returns></returns>
        private ListTextColumn<string> GenerateListTextColumn(string title, float relWidth, ColumnType colType)
        {

            /*
             * Instantiate a new list text column, set some basic properties
             */
            var column = new ListTextColumn<string>();
            column.Title = title;
            column.RelativeWidth = relWidth;
           

            /*
             * Set up column function to take in an element ID, and returns something based on the
             * ColumnType...i.e. creator, title, etc
             */
            column.ColumnFunction = delegate (string id)
            {
                var ec = SessionController.Instance.IdToControllers[id];
             
                Debug.Assert(ec != null);
                switch (colType)
                {
                    case ColumnType.Creator:
                        var longID = ec.Model.CreatorId;
                        return SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[longID];
                    case ColumnType.Title:
                        return ec.Model.Title;
                    case ColumnType.Type:
                        return ec.Model.ElementType.ToString();
                    default:
                        return String.Empty;
                }
                
            };

            return column;
        }
        
    }
}
