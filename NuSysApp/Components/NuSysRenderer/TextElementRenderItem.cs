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
        private HTMLParser _htmlParser;
        private CanvasTextLayout _textItemLayout;
        private string _textboxtext = string.Empty;
        private CanvasGeometry _clippingRect;
        private CanvasStrokeStyle _strokeStyle;
        private float _margin = 10;

        public TextElementRenderItem(TextNodeViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _htmlParser = new HTMLParser(resourceCreator);
            (_vm.Controller as TextNodeController).LibraryElementController.ContentDataController.ContentDataUpdated += LibraryElementControllerOnContentChanged;
            _vm.Controller.SizeChanged += Controller_SizeChanged;
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, _vm.Width, _vm.Height));
            _strokeStyle = new CanvasStrokeStyle {TransformBehavior = CanvasStrokeTransformBehavior.Fixed};
            _textboxtext = vm.Text;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _vm = null;
            _htmlParser = null;
            _textItemLayout.Dispose();
            _textItemLayout = null;
            _textboxtext = null;
            _clippingRect.Dispose();
            _clippingRect = null;
            _strokeStyle.Dispose();
            _strokeStyle = null;
            base.Dispose();
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            IsDirty = true;
            Update();
        }

        private void LibraryElementControllerOnContentChanged(object source, string contentData)
        {
            _textboxtext = contentData;
            IsDirty = true;
        }


        public override void Update()
        {
            if (IsDisposed)
                return;

            base.Update();
            if (!IsDirty)
                return;
            _textItemLayout = _htmlParser.GetParsedText(_textboxtext, _vm.Height - 2* _margin, _vm.Width - 2*_margin);
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, _vm.Width, _vm.Height));
            IsDirty = false;
        }



        public async override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;

            ds.FillRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Colors.White);
            ds.DrawRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Constants.color1, 1f, _strokeStyle);

            if (_textItemLayout == null)
                return;

            using (ds.CreateLayer(1f, _clippingRect))
            {
                ds.DrawTextLayout(_textItemLayout, _margin, _margin, Colors.Black);
            }

            ds.Transform = orgTransform;
        }
    }
}
