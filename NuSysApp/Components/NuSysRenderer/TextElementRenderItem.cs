using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

using Windows.Graphics.DirectX;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using SharpDX.DirectWrite;
using WinRTXamlToolkit.IO.Serialization;

namespace NuSysApp
{
    public class TextElementRenderItem : ElementRenderItem
    {
        private TextNodeViewModel _vm;
        private string _textboxtext = string.Empty;
        private CanvasGeometry _clippingRect;
        private CanvasStrokeStyle _strokeStyle;
        private float _margin = 10;
        private MarkdownConvertingTextbox _textBox;

        public TextElementRenderItem(TextNodeViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            (_vm.Controller as TextNodeController).LibraryElementController.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;
            _vm.Controller.SizeChanged += Controller_SizeChanged;
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, _vm.Width, _vm.Height));
            _strokeStyle = new CanvasStrokeStyle {TransformBehavior = CanvasStrokeTransformBehavior.Fixed};
            _textboxtext = vm.Text;
            _textBox = new MarkdownConvertingTextbox(this,resourceCreator)
            {
                Wrapping = CanvasWordWrapping.WholeWord,
                TextVerticalAlignment = CanvasVerticalAlignment.Top,
                Text = _vm?.Controller?.LibraryElementController?.ContentDataController?.ContentDataModel.Data ?? "",
                Scrollable = false
            };
            AddChild(_textBox);
        }

        public override async Task Load()
        {
            await _textBox.Load();
            await base.Load();
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _vm = null;
            _textboxtext = null;
            _clippingRect?.Dispose();
            _clippingRect = null;
            _strokeStyle?.Dispose();
            _strokeStyle = null;
            base.Dispose();
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            IsDirty = true;
            Update(Transform.Parent.LocalMatrix);
        }

        private void LibraryElementControllerOnContentChanged(object source, string contentData)
        {
            _textboxtext = contentData;
            _textBox.Text = contentData;
            _textBox.Text = contentData;
            IsDirty = true;
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (IsDisposed)
                return;

            base.Update(parentLocalToScreenTransform);

            if (!IsDirty)
                return;

            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, _vm.Width, _vm.Height));

            _textBox.Width = (float)(_vm.Width);
            _textBox.Height = (float)(_vm.Height);
            IsDirty = false;
        }



        public async override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Colors.White);
            ds.DrawRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Constants.color1, 1f, _strokeStyle);
            
            ds.Transform = orgTransform;
            base.Draw(ds);

        }
    }
}
