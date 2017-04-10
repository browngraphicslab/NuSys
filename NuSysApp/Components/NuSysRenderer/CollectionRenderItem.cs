using System;
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
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using MyToolkit.Utilities;
using NusysIntermediate;
using Wintellect.PowerCollections;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    public class CollectionRenderItem : ElementRenderItem
    {
        private CanvasAnimatedControl _canvas;
        private Size _elementSize;
        private CanvasGeometry _shape;
        private ICanvasImage _shapeImage = null;
        private CanvasStrokeStyle _strokeStyle = new CanvasStrokeStyle
        {
            TransformBehavior = CanvasStrokeTransformBehavior.Fixed,
        };

        public event EventHandler<LibraryElementController> CameraOnCentered;

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

        private enum ShapedStatus
        {
            None,
            Image,
            Points
        }

        private ShapedStatus _shapeStatus;

        private List<Rect> _boundingRects = new List<Rect>();

        public void AddRect(Rect rect)
        {
            var array = _renderItems2.OfType<VariableElementRenderItem>().ToArray();
            var overlap = new List<VariableElementRenderItem>();
            var rectsToFix = new List<Rect>();

            foreach (var ri in array)
            {
                var tempRect = new Rect(new Point(ri.ViewModel.X, ri.ViewModel.Y),
                    new Point(ri.ViewModel.X + ri.ViewModel.Width, ri.ViewModel.Y + ri.ViewModel.Height));
                if (rect.Intersects(tempRect))
                {
                    overlap.Add(ri);
                    rectsToFix.Add(tempRect);
                }
            }

            List<Rect> fixedRects;
            try
            {
                var cluster = new ClusterUtil();
                fixedRects = cluster.FitRects(rectsToFix, 35, 5);
            }
            catch (Exception e)
            {
                fixedRects = rectsToFix;
            }


            int j = 0;
            foreach (var renderItem in overlap)
            {
                var r = fixedRects[j];
                var elController = renderItem.ViewModel.Controller as VariableElementController;
                elController.SetSize(r.Width,r.Height);
                elController.SetPosition(r.X,r.Y);
                j++;
            }
            var fx = fixedRects;

            var b = 5;

            var minx = fx.Min(i => i.X);
            var miny = fx.Min(i => i.Y);
            _boundingRects.Add(new Rect(minx - b, miny - b, fx.Max(i => i.X + i.Width) - minx + b +b, fx.Max(i => i.Y + i.Height) - miny +b+ b));
        }

        public void SetNewController(LibraryElementController controller)
        {
            var array = _renderItems2.OfType<VariableElementRenderItem>().ToArray();
            var overlap = new List<VariableElementRenderItem>();
            foreach (var rect in _boundingRects.ToArray())
            {
                foreach (var ri in array)
                {
                    if (rect.Intersects(new Rect(new Point(ri.ViewModel.X, ri.ViewModel.Y),
                        new Point(ri.ViewModel.X + ri.ViewModel.Width, ri.ViewModel.Y + ri.ViewModel.Height))))
                    {
                        overlap.Add(ri);
                    }
                }
            }

            foreach (var renderItem in overlap)
            {
                var elController = renderItem.ViewModel.Controller as VariableElementController;
                elController.SetStoredLibraryId(controller.LibraryElementModel.LibraryElementId);
            }
        }

        private bool doOverlap(Point l1, Point r1, Point l2, Point r2)
        {
            // If one rectangle is on left side of other
            if (l1.X > r2.X || l2.X > r1.X)
                return false;

            // If one rectangle is above other
            if (l1.Y < r2.Y || l2.Y < r1.Y)
                return false;

            return true;
        }

        public CollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi canvas, bool interactionEnabled = false) : base(vm, parent, canvas)
        {
            _canvas = (CanvasAnimatedControl)ResourceCreator;
            ViewModel = vm;
            var collectionController = (ElementCollectionController)vm.Controller;
            collectionController.CameraPositionChanged += OnCameraPositionChanged;
            collectionController.CameraCenterChanged += OnCameraCenterChanged;
            collectionController.CameraScaleChanged += CollectionControllerOnCameraScaleChanged;

            if (!interactionEnabled)
                Transform.LocalPosition = new Vector2((float)vm.X, (float)vm.Y);

            Camera.SetParent(Transform);
            Camera.LocalPosition = vm.CameraTranslation;
            Camera.LocalScaleCenter = vm.CameraCenter;
            Camera.LocalScale = new Vector2(vm.CameraScale, vm.CameraScale);

            vm.Elements.CollectionChanged += OnElementsChanged;
            vm.Links.CollectionChanged += OnElementsChanged;
            vm.ToolLinks.CollectionChanged += OnElementsChanged;
            vm.Trails.CollectionChanged += OnElementsChanged;
            vm.AtomViewList.CollectionChanged += OnElementsChanged;
            vm.Controller.LibraryElementController.ContentDataController.ContentDataUpdated += ContentDataControllerOnContentDataUpdated;
            UpdateShapeStatus();
        }



        /// <summary>
        /// event handler called whenevr the element controller changes scale
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sx"></param>
        /// <param name="sy"></param>
        private void ControllerOnScaleChanged(object source, double sx, double sy)
        {
            Camera.CameraScale = (float)sx;
        }

        private void ContentDataControllerOnContentDataUpdated(object sender, string s)
        {
            UpdateShapeStatus();
        }

        private void UpdateShapeStatus()
        {
            //TEST END

            if (!ViewModel.IsShaped)
            {
                _shapeStatus = ShapedStatus.None;
                return;
            }
            var controller = ViewModel.Controller.LibraryElementController.ContentDataController as CollectionContentDataController;
            Debug.Assert(controller != null);

            var shape = controller.CollectionModel.Shape;
            Debug.Assert(shape != null);
            if (shape.ImageUrl == null)
            {
                _shapeStatus = ShapedStatus.Points;
            }
            else
            {
                _shapeStatus = ShapedStatus.Image;
                Task.Run(async delegate {
                    _shapeImage = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri(shape.ImageUrl));
                });
            }
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
                collectionController.CameraScaleChanged -= CollectionControllerOnCameraScaleChanged;

                ViewModel.Elements.CollectionChanged -= OnElementsChanged;
                ViewModel.Links.CollectionChanged -= OnElementsChanged;
                ViewModel.ToolLinks.CollectionChanged -= OnElementsChanged;
                ViewModel.Trails.CollectionChanged -= OnElementsChanged;
                ViewModel.AtomViewList.CollectionChanged -= OnElementsChanged;
                ViewModel.Controller.LibraryElementController.ContentDataController.ContentDataUpdated -= ContentDataControllerOnContentDataUpdated;
                foreach (var item in _renderItems0.ToArray())
                    Remove(item);

                foreach (var item in _renderItems1.ToArray())
                    Remove(item);

                foreach (var item in _renderItems2.ToArray())
                    Remove(item);

                foreach (var item in _renderItems3.ToArray())
                    Remove(item);

                _renderItems0?.Clear();
                _renderItems1?.Clear();
                _renderItems2?.Clear();
                _renderItems3?.Clear();

                InkRenderItem?.Dispose();
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
                foreach (var elementViewModel in ViewModel.ToolLinks.ToArray())
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

        /// <summary>
        /// Removes all instances of the passed in link from the current collection
        /// </summary>
        /// <param name="libraryElementId"></param>
        public void RemoveLink(string libraryElementId)
        {
            // get all the links we want to remove
            var soughtLinks = ViewModel.Links.Where(l => l.Controller.LibraryElementController.LibraryElementModel.LibraryElementId == libraryElementId);

            // if no instances of the link exist just return
            if (!soughtLinks.Any())
            {
                return;
            }

            // otherwise remove all the links from the collection
            soughtLinks.ToArray().ForEach(i => ViewModel.Links.Remove(i));
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                // refresh the list of render items
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

            _elementSize = new Size(ViewModel.Width, ViewModel.Height);

            var b = this == SessionController.Instance.SessionView.FreeFormViewer.InitialCollection;

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

        private void DrawBackgroundShapeImage(CanvasDrawingSession ds, IEnumerable<Vector2> pts)
        {
            if (_shapeStatus == ShapedStatus.Image)
            {
                if (_shapeImage != null)
                {
                    Debug.Assert(pts != null && pts.Any());
                    var bounds = _shape.ComputeBounds();
                    var ratio = bounds.Width/bounds.Height;

                    var multiplier =_shapeImage.GetBounds(ResourceCreator).Width/(pts.Max(l => l.X) - pts.Min(l => l.X)) ;

                    bounds.X = pts.Min(l => l.X);
                    bounds.Y = pts.Min(l => l.Y);
                    bounds.Width = (pts.Max(l => l.X) - bounds.X)*multiplier;
                    bounds.Height = (bounds.Width/ratio);

                    ds.DrawImage((CanvasBitmap)_shapeImage, bounds);
                }
            }
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

            var controller = ViewModel.Controller.LibraryElementController.ContentDataController as CollectionContentDataController;
            Debug.Assert(controller != null);
            var pts = controller?.CollectionModel?.Shape?.ShapePoints?.Select(p => new Vector2((float)p.X, (float)p.Y))?.ToArray() ?? new Vector2[0];

            if (controller?.CollectionModel?.Shape?.ShapePoints == null &&
                controller?.CollectionModel?.Shape?.ImageUrl != null && controller?.CollectionModel?.Shape?.AspectRatio != null
                && controller?.CollectionModel?.Shape?.AspectRatio != 0 && _shapeImage != null)
            {
                var bounds = _shapeImage.GetBounds(ResourceCreator);
                pts = new Vector2[]
                {
                    new Vector2(50000,50000),
                    new Vector2((float)(50000+bounds.Width), 50000),
                    new Vector2((float)(50000+bounds.Width),50000+(float)bounds.Height),
                    new Vector2(50000,50000+(float)bounds.Height),
                };
            }

            if (ViewModel.IsShaped || ViewModel.IsFinite)
            {
                if (_shapeStatus == ShapedStatus.Image)
                {
                    if (_shapeImage != null)
                    {
                        _shape = CanvasGeometry.CreateRectangle(ResourceCreator, _shapeImage.GetBounds(ResourceCreator));
                    }
                }
                else if (_shapeStatus == ShapedStatus.Points)
                {
                    _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts);
                }
                else
                {
                    _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts);
                }

            }
            if ((pts == null || pts.Count() == 0 )&& ViewModel.IsFinite && !ViewModel.IsShaped)
            {
                var bounds = new Rect(50000,50000,5000,5000);
                pts = new Vector2[]
                {
                    new Vector2(50000,50000),
                    new Vector2((float)(50000+bounds.Width), 50000),
                    new Vector2((float)(50000+bounds.Width),50000+(float)bounds.Height),
                    new Vector2(50000,50000+(float)bounds.Height),
                };
                _shape = CanvasGeometry.CreateRectangle(ResourceCreator, bounds);
            }
            if (_shape == null)
            {
                _shape = CanvasGeometry.CreatePolygon(ResourceCreator, pts);
            }

            if (this != initialCollection && ViewModel.IsFinite && ViewModel.IsShaped)
            {
                var scaleFactor = (float)_elementSize.Width / (float)_shape.ComputeBounds().Width;
                Camera.LocalScale = new Vector2(scaleFactor);
                if (_shapeStatus == ShapedStatus.Image)
                {
                    if (_shapeImage != null)
                    {
                        var rect = _shapeImage.GetBounds(ResourceCreator);
                        rect.Height *= scaleFactor;
                        rect.Width *= scaleFactor;
                        Mask = CanvasGeometry.CreateRectangle(ResourceCreator, rect);
                    }
                    else
                    {
                        Mask = _shape;
                    }
                }
                else
                {
                    ds.Transform = Camera.LocalToScreenMatrix;
                    Mask = _shape;
                }
            }
            else if (this != initialCollection && ViewModel.IsFinite && !ViewModel.IsShaped)
            {
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
            if (this == initialCollection)
            {
                this.DrawBackgroundShapeImage(ds, pts);
            }

            using (ds.CreateLayer(1, Mask))
            {

                ds.Transform = Transform.LocalToScreenMatrix;
                ds.FillRectangle(GetLocalBounds(), Colors.White);

                ds.Transform = Camera.LocalToScreenMatrix;
                if (ViewModel.IsShaped && _shapeStatus != ShapedStatus.Image)
                {
                    ds.FillGeometry(_shape, ViewModel.ShapeColor);
                }
                DrawBackgroundShapeImage(ds, pts);
                var screenRect = GetScreenRect();

                // draw the ink render item
                foreach (var item in _renderItems0.ToArray())
                {
                    item.Draw(ds);
                }

                //draw trails first
                var trails = _renderItems1?.OfType<TrailRenderItem>()?.ToArray();
                trails?.ForEach(t => t.Draw(ds));

                // not sure what is on the layer if anything yet
                foreach (var item in _renderItems1?.Except(trails)?.ToArray() ?? new BaseRenderItem[0])
                {
                    item.Draw(ds);
                }

                var old = ds.Transform;
                ds.Transform = Matrix3x2.Identity;

                foreach (var rect in _boundingRects.ToArray())
                {
                    var p1 = new Point(rect.X, rect.Y);
                    var p2 = new Point(rect.X + rect.Width, rect.Y + rect.Height);
                    var v1 = Vector2.Transform(p1.ToSystemVector2(), Camera.LocalToScreenMatrix).ToPoint();
                    var v2 = Vector2.Transform(p2.ToSystemVector2(), Camera.LocalToScreenMatrix).ToPoint();
                    ds.FillRectangle(new Rect(v1.X, v1.Y, v2.X - v1.X, v2.Y - v1.Y), Color.FromArgb(100, 14, 233, 143));
                }
                ds.Transform = old;

                // this layer contains tools and element render items
                foreach (var item in _renderItems2?.ToArray() ?? new BaseRenderItem[0])
                {
                    // if the item is a tool window, draw the tool window
                    if (item is ToolWindow)
                    {
                        (item as ToolWindow).Draw(ds);
                    }

                    // check to see if the item is an element render item
                    var element = item as ElementRenderItem;
                    if (element == null)
                        return;

                    // if the item is a tool, or the rectangle the screen takes up intersects with the rectangle that
                    // bounds the element on the screen, then draw it
                    if (element is PseudoElementRenderItem || screenRect.Intersects(element.GetScreenRect()))
                        item.Draw(ds);
                 }

                // not sure what is on the layer if anything yet
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
            _allRenderItems = _renderItems0.ToArray().Concat(_renderItems1.ToArray()).Concat(_renderItems2.ToArray()).Concat(_renderItems3.ToArray()).ToList();
            return _allRenderItems;
        }

        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
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

        /// <summary>
        /// event fired when the camera scale changes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="f"></param>
        private void CollectionControllerOnCameraScaleChanged(object source, float f)
        {
            Camera.CameraScale = f;
        }

        private async void OnElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _canvas.RunOnGameLoopThreadAsync(async () =>
            {
                if (IsDisposed)
                    return;
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
            /*
            if (vm is ToolFilterView || vm is BaseToolView || vm is MetadataToolView)
            {
                await UITask.Run(async delegate
                {
                    _renderItems2?.Add(new PseudoElementRenderItem((ITool)vm, this, ResourceCreator));
                });
            }*/
            if (vm is BasicToolViewModel)
            {
                var toolvm = (BasicToolViewModel)vm;
                var tool = new BasicToolWindow(this, ResourceCreator, (BasicToolViewModel) vm);

                tool.Load();
                _renderItems3?.Add(tool);
            }
            else if (vm is MetadataToolViewModel)
            {
                var tool = new MetadataToolWindow(this, ResourceCreator, (MetadataToolViewModel)vm);
                tool.Load();
                _renderItems3?.Add(tool);
            }
            else if (vm is ToolLinkViewModelWin2d)
            {
                var link = new ToolLinkRenderitem((ToolLinkViewModelWin2d)vm, this, ResourceCreator);
                _renderItems1?.Add(link);
            }
            else if (vm is VariableNodeViewModel)
            {
                item = new VariableElementRenderItem((VariableNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is TextNodeViewModel)
            {
                item = new TextElementRenderItem((TextNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }

            else if (vm is UnknownFileViewModel)
            {
                item = new UnknownFileElementRenderItem((UnknownFileViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is HtmlNodeViewModel)
            {
                item = new HtmlElementRenderItem((HtmlNodeViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is ImageElementViewModel)
            {
                item = new ImageElementRenderItem((ImageElementViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is WordNodeViewModel)
            {
                item = new WordElementRenderItem((WordNodeViewModel)vm, this, ResourceCreator);
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

            item?.Transform.SetParent(Camera);

            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _allRenderItems = _renderItems3.Concat(_renderItems2).Concat(_renderItems1).Concat(_renderItems0).ToList();
            });
        
        }

        private void RemoveItem(object item)
        {

            if (item is ElementViewModel)
            {
                IEnumerable<BaseRenderItem> renderItem;
                if (item is ToolViewModel)
                {
                    renderItem = _renderItems3.OfType<ToolWindow>().Where(el => el.Vm == item);
                }
                else
                {
                    renderItem = _renderItems2.OfType<ElementRenderItem>().Where(el => el.ViewModel == item);
                }
                if (renderItem.Any())
                {
                    var element = renderItem.First();
                    Remove(element);
                }
            }
            if (item is ToolLinkViewModelWin2d)
            {
                var renderItem = _renderItems1.OfType<ToolLinkRenderitem>().Where(el => el.ViewModel == item);
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
        /// Zooms the camera to the passed in elementModelId
        /// </summary>
        /// <param name="elementModelId"></param>
        public void CenterCameraOnElement(string elementModelId)
        {
            UITask.Run(delegate
            {
                var elementToBeFullScreened =
                    SessionController.Instance.ActiveFreeFormViewer.Elements.FirstOrDefault(
                        elem => elem.Id == elementModelId);
                Debug.Assert(elementToBeFullScreened != null);

                // Define some variables that will be used in future translation/scaling
                var nodeWidth = elementToBeFullScreened.Width;
                var nodeHeight = elementToBeFullScreened.Height; // 40 for title adjustment

                var x = elementToBeFullScreened.X + nodeWidth/2;
                var y = elementToBeFullScreened.Y + nodeHeight/2;
                var widthAdjustment = ViewModel.Width/2;
                var heightAdjustment = ViewModel.Height/2;

                // Reset the scaling and translate the free form viewer so that the passed in element is at the center
                var scaleX = 1;
                var scaleY = 1;
                var translateX = widthAdjustment - x;
                var translateY = heightAdjustment - y;
                double scale;


                // Scale based on the width and height proportions of the current node
                if (nodeWidth > nodeHeight)
                {
                    scale = ViewModel.Width/nodeWidth;
                    if (nodeWidth - nodeHeight <= 20)
                        scale = scale*.50;
                    else
                        scale = scale*.55;
                }


                else
                {
                    scale = ViewModel.Height/nodeHeight;
                    scale = scale*.7;
                }

                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition =
                    new Vector2((float) translateX, (float) translateY);
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter =
                    new Vector2((float) x, (float) y);
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale =
                    new Vector2((float) scale, (float) scale);
                //SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.ViewModel.CameraTranslation = new Vector2((float)translateX, (float)translateY);
                //SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.ViewModel.CameraCenter = new Vector2((float)x, (float)y);
                //SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.ViewModel.CameraScale = (float)scale;
                SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.InkRenderItem?
                    .UpdateDryInkTransform();
                SessionController.Instance.SessionView.FreeFormViewer._minimap?.Invalidate();
                CameraOnCentered?.Invoke(this, SessionController.Instance.ContentController.GetLibraryElementController(elementToBeFullScreened.LibraryElementId));

            });
        }
    }
}
