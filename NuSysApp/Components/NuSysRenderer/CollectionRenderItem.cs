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
        private CanvasAnimatedControl _canvas;

        protected List<BaseRenderItem> _renderItems0 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems1 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems2 = new List<BaseRenderItem>();
        protected List<BaseRenderItem> _renderItems3 = new List<BaseRenderItem>();

        public InkRenderItem InkRenderItem { get; set; }
        public ElementCollectionViewModel ViewModel;
        private List<BaseRenderItem> _allRenderItems = new List<BaseRenderItem>();

        public Transformable Camera { get; set; } = new Transformable();
        
        public int NumLinks => ViewModel.Links.Count;
        
        public CollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi canvas, bool interactionEnabled = false) : base(vm, parent, canvas)
        {
            _canvas = (CanvasAnimatedControl)ResourceCreator;
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
        }

        public void RemoveLink(string libraryElementId)
        {
            var soughtLinks = ViewModel.Links.Where( l => l.Controller.LibraryElementController.LibraryElementModel.LibraryElementId == libraryElementId);
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

                Debug.WriteLine("Loading done.");
            });
        }

        public override void Dispose()
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
            _canvas.RunOnGameLoopThreadAsync(() =>
            {
                _renderItems2.Remove(item);
                _renderItems2.Insert(_renderItems2.Count, item);
            });
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
            BaseRenderItem item;
            if (vm is ToolFilterView || vm is BaseToolView)
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
                item = new ShapedCollectionRenderItem((ElementCollectionViewModel)vm, this, ResourceCreator);
                await item.Load();
                _renderItems2?.Add(item);
            }
            else if (vm is LinkViewModel)
            {
                _renderItems1.Add(new LinkRenderItem((LinkViewModel)vm, this, ResourceCreator));
            }
            else if (vm is PresentationLinkViewModel)
            {
                _renderItems1.Add(new TrailRenderItem((PresentationLinkViewModel)vm, this, ResourceCreator));
            }

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
            return Win2dUtil.Invert(Camera.C)*Camera.S*Camera.C*Camera.T;
        }

        public List<BaseRenderItem> GetRenderItems()
        {
            return _allRenderItems;
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
