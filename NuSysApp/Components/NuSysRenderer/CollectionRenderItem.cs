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
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class CollectionRenderItem : ElementRenderItem, I2dTransformable
    {

        private HashSet<BaseRenderItem> _renderItems0 = new HashSet<BaseRenderItem>();
        private HashSet<BaseRenderItem> _renderItems1 = new HashSet<BaseRenderItem>();
        private HashSet<BaseRenderItem> _renderItems2 = new HashSet<BaseRenderItem>();
        private HashSet<BaseRenderItem> _renderItems3 = new HashSet<BaseRenderItem>();

        public InkRenderItem InkRenderItem { get; set; }
        public CollectionInteractionManager InteractionManager;
        public ElementCollectionViewModel ViewModel;

        public Transformable Camera { get; set; } = new Transformable();

        public CollectionRenderItem(ElementCollectionViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl canvas, bool interactionEnabled = false) : base(vm, parent, canvas)
        {            
            ViewModel = vm;
            (vm.Controller as ElementCollectionController).CameraPositionChanged += OnCameraPositionChanged;
            (vm.Controller as ElementCollectionController).CameraCenterChanged += OnCameraCenterChanged;

            if (!interactionEnabled)
                T = Matrix3x2.CreateTranslation((float)vm.X, (float)vm.Y);

            Camera.T = Matrix3x2.CreateTranslation(vm.CameraTranslation);
            Camera.C = Matrix3x2.CreateTranslation(vm.CameraCenter);
            Camera.S = Matrix3x2.CreateScale(vm.CameraScale);
            

            vm.Elements.CollectionChanged += ElementsChanged;

            InkRenderItem = new InkRenderItem(this, canvas);
            _renderItems0.Add(InkRenderItem);
        }

        private void OnCameraCenterChanged(float f, float f1)
        {
            Camera.C = Matrix3x2.CreateTranslation(new Vector2(f, f1));
        }

        private void OnCameraPositionChanged(float f, float f1)
        {
            Camera.T = Matrix3x2.CreateTranslation(new Vector2(f,f1));
        }

        public override void Update()
        {
            base.Update();

            foreach (var item in _renderItems0.ToArray())
                item.Update();

            foreach (var item in _renderItems1.ToArray())
                item.Update();

            foreach (var item in _renderItems2.ToArray())
                item.Update();

            foreach (var item in _renderItems3.ToArray())
                item.Update();

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = GetTransform() * ds.Transform;
            var boundaries = new Rect(0, 0, ViewModel.Width, ViewModel.Height);

            Color borderColor;
            float borderWidth = 4f;

            if (NuSysRenderer.Instance.CurrentCollection == this)
            {
                borderColor = Color.FromArgb(255, 0, 102, 255);
                borderWidth = 6f;
            }
            else
            {
                borderColor = Colors.Black;
                borderWidth = 4f;
            }

            if (this != NuSysRenderer.Instance.InitialCollection)
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
        }

        private async void ElementsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    BaseRenderItem item;
                    var vm = (ElementViewModel) newItem;
                    if (vm is TextNodeViewModel)
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
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else if (vm is ElementCollectionViewModel)
                    {
                        item = new CollectionRenderItem((ElementCollectionViewModel) vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                    else
                    {
                        item = new ElementRenderItem(vm, this, ResourceCreator);
                        await item.Load();
                        _renderItems2.Add(item);
                    }
                }
            }

            if (e.OldItems == null)
                return;

            var allItems = _renderItems0.Concat(_renderItems1).Concat(_renderItems2).Concat(_renderItems3);
            foreach (var oldItem in e.OldItems)
            {
                var renderItem = allItems.OfType<ElementRenderItem>().Where(el => el.ViewModel == oldItem).First();
                Remove(renderItem);
            }
        }

        public Vector2 ScreenPointToObjectPoint(Vector2 sp)
        {
            var transform = Win2dUtil.Invert(NuSysRenderer.Instance.GetCollectionTransform(this));
            return Vector2.Transform(sp, transform);
        }

        public Vector2 ObjectPointToScreenPoint(Vector2 op)
        {
            var transform = NuSysRenderer.Instance.GetCollectionTransform(this);
            return Vector2.Transform(op, transform);

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


        public void AddAdornment(InkStroke stroke)
        {
            _renderItems0.Add(new AdornmentRenderItem(stroke, this, ResourceCreator));
        }

        public void AddStroke(InkStroke stroke)
        {
            InkRenderItem.AddStroke(stroke);
        }

        public void AddLink(LinkViewModel vm)
        {
            _renderItems1.Add(new LinkRenderItem(vm, this, ResourceCreator));
        }

        public void AddTempLink(TempLinkRenderItem tempLink)
        {
            _renderItems1.Add(tempLink);
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

        public override bool HitTest(Vector2 point)
        {
            if (ViewModel is FreeFormViewerViewModel)
            {
                return true;
            }

            return base.HitTest(point);
        }
    }
}
