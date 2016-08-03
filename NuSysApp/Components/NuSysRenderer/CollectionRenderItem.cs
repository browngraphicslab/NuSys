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
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp.Components.NuSysRenderer
{
    public class CollectionRenderItem : ElementRenderItem, I2dTransformable
    {

        private ConcurrentBag<BaseRenderItem> _renderItems0 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems1 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems2 = new ConcurrentBag<BaseRenderItem>();
        private ConcurrentBag<BaseRenderItem> _renderItems3 = new ConcurrentBag<BaseRenderItem>();

        private InkRenderItem _inkRenderItem;
        private CollectionInteractionManager _interactionManager;
        public ElementCollectionViewModel ViewModel;

        public Transformable Camera = new Transformable();

        private bool _interactionEnabled;


        public CollectionRenderItem(ElementCollectionViewModel vm, CanvasAnimatedControl canvas, bool interactionEnabled = false) : base(vm, canvas)
        {
            _interactionEnabled = interactionEnabled;

            ViewModel = vm;

            T = Matrix3x2.CreateTranslation((float)vm.X, (float)vm.Y);

            Camera.T = Matrix3x2.CreateTranslation(-Constants.MaxCanvasSize / 2f, -Constants.MaxCanvasSize / 2f);
            Camera.C = Matrix3x2.CreateTranslation(Constants.MaxCanvasSize / 2f, Constants.MaxCanvasSize / 2f);
            Camera.S = Matrix3x2.CreateScale(1f, 1f);
            

            vm.Elements.CollectionChanged += ElementsChanged;

            _inkRenderItem = new InkRenderItem(canvas);
            _renderItems0.Add(_inkRenderItem);

            if (interactionEnabled)
            {
                _interactionManager = new CollectionInteractionManager(this);
                _interactionManager.ItemSelected += OnItemSelected;
            }


        }

        private void OnItemSelected(BaseRenderItem element, PointerDeviceType device)
        {
            var elementRenderItem = element as ElementRenderItem;
            var vm = (ElementCollectionViewModel)ViewModel;
            if (elementRenderItem == null)
            {
                vm.ClearSelection();
            }
            else
            {
                if (device == PointerDeviceType.Mouse)
                {
                    var keyState = CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift);
                    if (keyState != CoreVirtualKeyStates.Down)
                        vm.ClearSelection();
                    vm.AddSelection(elementRenderItem.ViewModel);
                }

                if (device == PointerDeviceType.Touch)
                {
                  //  if (_activePointers.Count == 0)
                      vm.ClearSelection();
                    vm.AddSelection(elementRenderItem.ViewModel);
                }
            }
        }

        public override void Update()
        {
            base.Update();

            foreach (var item in _renderItems0)
                item.Update();

            foreach (var item in _renderItems1)
                item.Update();

            foreach (var item in _renderItems2)
                item.Update();

            foreach (var item in _renderItems3)
                item.Update();

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;
            ds.DrawRectangle(new Rect(0, 0, ViewModel.Width, ViewModel.Height), Colors.Blue, 3f);

            ds.Transform = Win2dUtil.Invert(Camera.C) * Camera.S * Camera.C * Camera.T * ds.Transform;
           
            foreach (var item in _renderItems0)
                item.Draw(ds);

            foreach (var item in _renderItems1)
                item.Draw(ds);

            foreach (var item in _renderItems2)
                item.Draw(ds);

            foreach (var item in _renderItems3)
                item.Draw(ds);

            ds.Transform = orgTransform;
        }

        private async void ElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var newItem in e.NewItems)
            {
                BaseRenderItem item;
                var vm = (ElementViewModel) newItem;
                if (vm is TextNodeViewModel)
                {
                    item = new TextElementRenderItem((TextNodeViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems0.Add(item);
                }
                else if (vm is ImageElementViewModel)
                {
                    item = new ImageElementRenderItem((ImageElementViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else if (vm is PdfNodeViewModel)
                {
                    item = new PdfElementRenderItem((PdfNodeViewModel) vm, ResourceCreator);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else if (vm is ElementCollectionViewModel)
                {
                    item = new CollectionRenderItem((ElementCollectionViewModel)vm, ResourceCreator);
                    await item.Load();
                    _renderItems1.Add(item);
                }
                else
                {
                    item = new ElementRenderItem(vm, ResourceCreator);
                    await item.Load();
                    _renderItems2.Add(item);
                }
            }
        }

        public Vector2 ScreenPointToObjectPoint(Vector2 sp)
        {
            var inverse = Win2dUtil.Invert(Win2dUtil.Invert(Camera.C) * Camera.S * Camera.C * Camera.T * Win2dUtil.Invert(C) * S * C * T);
            return Vector2.Transform(sp, inverse);
        }

        public Vector2 ObjectPointToScreenPoint(Vector2 op)
        {
            var invTransform = Matrix3x2.Identity;
            var inverse = Win2dUtil.Invert(Camera.C) * Camera.S * Camera.C * Camera.T * Win2dUtil.Invert(C) * S * C * T;
            return Vector2.Transform(op, inverse);

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


        public void AddAdornment(InkStroke stroke)
        {
            _renderItems0.Add(new AdornmentRenderItem(stroke, ResourceCreator));
        }

        public void AddStroke(InkStroke stroke)
        {
            _inkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm, ResourceCreator));
        }

        public void AddTrail(PresentationLinkViewModel vm)
        {
            _renderItems1.Add(new TrailRenderItem(vm, ResourceCreator));
        }

        public List<BaseRenderItem> GetRenderItems()
        {
            return _renderItems0.Concat(_renderItems1).Concat(_renderItems2).Concat(_renderItems3).ToList();
        } 
    }
}
