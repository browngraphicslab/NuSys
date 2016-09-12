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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;
using Wintellect.PowerCollections;

namespace NuSysApp
{
    public class CollectionRenderItem : ElementRenderItem, I2dTransformable
    {
        protected List<BaseRenderItem> _renderItems0 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems1 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems2 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems3 = new List<BaseRenderItem>();

        public InkRenderItem InkRenderItem { get; set; }
        public ElementCollectionViewModel ViewModel;

        public Transformable Camera { get; set; } = new Transformable();

        public CollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi canvas, bool interactionEnabled = false) : base(vm, parent, canvas)
        {            
            ViewModel = vm;
            var collectionController = (ElementCollectionController)vm.Controller;
            collectionController.CameraPositionChanged += OnCameraPositionChanged;
            collectionController.CameraCenterChanged += OnCameraCenterChanged;

            if (!interactionEnabled)
                T = Matrix3x2.CreateTranslation((float)vm.X, (float)vm.Y);

            Camera.T = Matrix3x2.CreateTranslation(vm.CameraTranslation);
            Camera.C = Matrix3x2.CreateTranslation(vm.CameraCenter);
            Camera.S = Matrix3x2.CreateScale(vm.CameraScale);
            

            vm.Elements.CollectionChanged += OnElementsChanged;
            vm.Links.CollectionChanged += OnElementsChanged;
            vm.Trails.CollectionChanged += OnElementsChanged;
            vm.AtomViewList.CollectionChanged += OnElementsChanged;

            InkRenderItem = new InkRenderItem(this, canvas);
            InkRenderItem.Load();
            _renderItems0.Add(InkRenderItem);
        }

        public override void Dispose()
        {
            base.Dispose();

            var collectionController = (ElementCollectionController)ViewModel.Controller;
            collectionController.CameraPositionChanged -= OnCameraPositionChanged;
            collectionController.CameraCenterChanged -= OnCameraCenterChanged;

            ViewModel.Elements.CollectionChanged -= OnElementsChanged;
            ViewModel.Links.CollectionChanged -= OnElementsChanged;
            ViewModel.Trails.CollectionChanged -= OnElementsChanged;
            ViewModel.AtomViewList.CollectionChanged -= OnElementsChanged;

            foreach (var item in _renderItems0.ToArray())
                item.Dispose();

            foreach (var item in _renderItems1.ToArray())
                item.Dispose();

            foreach (var item in _renderItems2.ToArray())
                item.Dispose();

            foreach (var item in _renderItems3.ToArray())
                item.Dispose();

            _renderItems0.Clear();
            _renderItems0 = null;
            _renderItems1.Clear();
            _renderItems1 = null;
            _renderItems2.Clear();
            _renderItems2 = null;
            _renderItems3.Clear();
            _renderItems3 = null;

            InkRenderItem.Dispose();
            InkRenderItem = null;

            ViewModel.Dispose();
            ViewModel = null;

            Camera = null;

            
        }

        private void OnCameraCenterChanged(float f, float f1)
        {
            Camera.C = Matrix3x2.CreateTranslation(new Vector2(f, f1));
        }

        private void OnCameraPositionChanged(float f, float f1)
        {
            Camera.T = Matrix3x2.CreateTranslation(new Vector2(f,f1));
        }

        public void BringForward(ElementRenderItem item)
        {
            _renderItems2.Remove(item);
            _renderItems2.Insert(_renderItems2.Count,item);
        }

        public override void Update()
        {
            if (IsDisposed)
                return;

            base.Update();

            foreach (var item in _renderItems0.ToArray())
                item.Update();

            foreach (var item in _renderItems1.ToArray())
                item.Update();

            foreach (var item in _renderItems2.ToArray())
                item?.Update();

            foreach (var item in _renderItems3.ToArray())
                item.Update();

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            /*
            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;
            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);

            Color borderColor;
            float borderWidth = 4f;

            if (SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection == this)
            {
                borderColor = Color.FromArgb(255, 0, 102, 255);
                borderWidth = 6f;
            }
            else
            {
                borderColor = Colors.Black;
                borderWidth = 4f;
            }

            if (this != SessionController.Instance.SessionView.FreeFormViewer.InitialCollection)
                ds.DrawRectangle(boundaries, borderColor, borderWidth);

            var boundariesGeom = CanvasGeometry.CreateRectangle(ds, boundaries);
            using (ds.CreateLayer(1, boundariesGeom))
            {
                ds.Transform = GetCameraTransform() * ds.Transform;
           
                foreach (var item in _renderItems0.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems1.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems2.ToArray())
                    item.Draw(ds);

                foreach (var item in _renderItems3.ToArray())
                    item.Draw(ds);



                ds.Transform = orgTransform;
            }
            */
        }

        private async void OnElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    BaseRenderItem item;
                    var vm = newItem;
                    if (vm is ToolFilterView || vm is BaseToolView)
                    {
                        _renderItems2.Add(new PseudoElementRenderItem((ITool)vm, this, ResourceCreator));
                    }
                    else if (vm is TextNodeViewModel)
                    {
                        item = new TextElementRenderItem((TextNodeViewModel) vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is ImageElementViewModel)
                    {
                        item = new ImageElementRenderItem((ImageElementViewModel) vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is PdfNodeViewModel)
                    {
                        item = new PdfElementRenderItem((PdfNodeViewModel) vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is AudioNodeViewModel)
                    {
                        item = new AudioElementRenderItem((AudioNodeViewModel) vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is VideoNodeViewModel)
                    {
                        item = new VideoElementRenderItem((VideoNodeViewModel) vm, this, ResourceCreator);
                        item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is ElementCollectionViewModel)
                    {
                        var collectionVm = (ElementCollectionViewModel) vm;
                        item = new ShapedCollectionRenderItem((ElementCollectionViewModel)vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is LinkViewModel)
                    {
                        AddLink((LinkViewModel)vm);
                    }
                    else if (vm is PresentationLinkViewModel)
                    {
                        AddTrail((PresentationLinkViewModel)vm);
                    }
                }
            }

            if (e.OldItems == null)
                return;

            var allItems = _renderItems0.Concat(_renderItems1).Concat(_renderItems2).Concat(_renderItems3);
            foreach (var oldItem in e.OldItems)
            {
                var renderItem = allItems.OfType<ElementRenderItem>().Where(el => el.ViewModel == oldItem);
                if (renderItem.Any())
                {
                    var element = renderItem.First();
                    Remove(element);
                    element.Dispose();
                }
                var linkItem = allItems.OfType<LinkRenderItem>().Where(el => el.ViewModel == oldItem);
                if (linkItem.Any())
                {
                    var link = linkItem.First();
                    Remove(link);
                    link.Dispose();
                }

                var trailItems = allItems.OfType<TrailRenderItem>().Where(el => el.ViewModel == oldItem);
                if (trailItems.Any())
                {
                    var trail = trailItems.First();
                    Remove(trail);
                    trail.Dispose();
                }

                var toolItem = allItems.OfType<PseudoElementRenderItem>().Where(el => el.Tool == oldItem);
                if (toolItem.Any())
                {
                    Remove(toolItem.First());
                }
            }
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
            if (_renderItems0.Contains(item))
                _renderItems0.Remove(item);
            if (_renderItems1.Contains(item))
                _renderItems1.Remove(item);
            if (_renderItems2.Contains(item))
                _renderItems2.Remove(item);
            if (_renderItems3.Contains(item))
                _renderItems3.Remove(item);
        }

        public void AddStroke(InkStroke stroke)
        {
            InkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm, this, ResourceCreator));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems1.Add(new TrailRenderItem(vm, this, ResourceCreator));
        }

        public Matrix3x2 GetCameraTransform()
        {
            return Win2dUtil.Invert(Camera.C)*Camera.S*Camera.C*Camera.T;
        }

        public List<BaseRenderItem> GetRenderItems()
        {
            return _renderItems3.Concat(_renderItems2).Concat(_renderItems1).Concat(_renderItems0).ToList();
        }

        public override BaseRenderItem HitTest(Vector2 point)
        {
            if (ViewModel is FreeFormViewerViewModel)
            {
                return this;
            }

            return base.HitTest(point);
        }
    }
}
