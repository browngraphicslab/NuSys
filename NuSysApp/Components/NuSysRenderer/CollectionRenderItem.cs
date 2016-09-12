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

        public int NumLinks => ViewModel.Links.Count;

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
        }

        public void RemoveLink(string libraryElementId)
        {
            var soughtLinks = ViewModel.Links.Where( l => l.Controller.LibraryElementController.LibraryElementModel.LibraryElementId == libraryElementId);
            if (!soughtLinks.Any())
            {
                return;
            }
            
            ViewModel.Links.Remove(soughtLinks.First());
        }

        public async override Task Load()
        {
            await base.Load();
            var collectionController = (ElementCollectionController)ViewModel.Controller;
            if (!collectionController.LibraryElementController.ContentLoaded)
            {
                await collectionController.LibraryElementController.LoadContentDataModelAsync();
            }
            var strokes = collectionController.LibraryElementController.ContentDataController.ContentDataModel.Strokes;
            foreach (var inkModel in strokes)
            {
                InkRenderItem.AddInkModel(inkModel);
            }

            InkRenderItem.Load();
            _renderItems0.Add(InkRenderItem);

        }

        public override void Dispose()
        {
            UITask.Run(() =>
            {
                if (IsDisposed)
                    return;

                base.Dispose();

                var collectionController = (ElementCollectionController) ViewModel.Controller;
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
            
        });
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
                item?.Update();

            foreach (var item in _renderItems1.ToArray())
                item?.Update();

            foreach (var item in _renderItems2.ToArray())
                item?.Update();

            foreach (var item in _renderItems3.ToArray())
                item?.Update();

        }

      
        private async void OnElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsDisposed)
                return;

            UITask.Run(async () =>
            {
                if (e.NewItems != null)
                {
                    foreach (var newItem in e.NewItems)
                    {
                        BaseRenderItem item;
                        var vm = newItem;
                        if (vm is ToolFilterView || vm is BaseToolView)
                        {
                            _renderItems2?.Add(new PseudoElementRenderItem((ITool) vm, this, ResourceCreator));
                        }
                        else if (vm is TextNodeViewModel)
                        {
                            item = new TextElementRenderItem((TextNodeViewModel) vm, this, ResourceCreator);
                            await item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is ImageElementViewModel)
                        {
                            item = new ImageElementRenderItem((ImageElementViewModel) vm, this, ResourceCreator);
                            await item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is PdfNodeViewModel)
                        {
                            item = new PdfElementRenderItem((PdfNodeViewModel) vm, this, ResourceCreator);
                            await item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is AudioNodeViewModel)
                        {
                            item = new AudioElementRenderItem((AudioNodeViewModel) vm, this, ResourceCreator);
                            await item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is VideoNodeViewModel)
                        {
                            item = new VideoElementRenderItem((VideoNodeViewModel) vm, this, ResourceCreator);
                            item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is ElementCollectionViewModel)
                        {
                            item = new ShapedCollectionRenderItem((ElementCollectionViewModel) vm, this, ResourceCreator);
                            await item.Load();
                            _renderItems2?.Add(item);
                        }
                        else if (vm is LinkViewModel)
                        {
                            AddLink((LinkViewModel) vm);
                        }
                        else if (vm is PresentationLinkViewModel)
                        {
                            AddTrail((PresentationLinkViewModel) vm);
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
            });
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
