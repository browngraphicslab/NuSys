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

namespace NuSysApp
{
    public class TextElementRenderItem : ElementRenderItem
    {
        private TextNodeViewModel _vm;
        private HTMLParser _htmlParser;
        private CanvasTextLayout _textItemLayout;
        private string _textboxtext = string.Empty;

        public TextElementRenderItem(TextNodeViewModel vm, CollectionRenderItem parent, CanvasAnimatedControl resourceCreator):base(vm, parent, resourceCreator)
        {
            _vm = vm;
            _htmlParser = new HTMLParser(resourceCreator);
            (_vm.Controller as TextNodeController).LibraryElementController.ContentChanged += LibraryElementControllerOnContentChanged;
        }

        private void LibraryElementControllerOnContentChanged(object source, string contentData)
        {
            _textboxtext = contentData;
            IsDirty = true;
        }


        public override void Update()
        {
            if (!IsDirty)
                return;
            _textItemLayout = _htmlParser.GetParsedText(_textboxtext, _vm.Height, _vm.Width);
            IsDirty = false;
        }

        public override void Dispose()
        {
            base.Dispose();
            _vm = null;
        }

        public async override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Win2dUtil.Invert(C) * S * C * T * ds.Transform;

            ds.FillRectangle( new Rect {X = 0, Y = 0, Width = _vm.Width, Height=_vm.Height}, Colors.White);

            if (_textItemLayout == null)
                return;
            //_textItemLayout = _htmlParser.GetParsedText(_vm.Controller.LibraryElementModel.Data, _vm.Height, _vm.Width);
            //_textItemLayout.HorizontalAlignment = CanvasHorizontalAlignment.Center;
            ds.DrawTextLayout(_textItemLayout, 0, 0, Colors.Black);
            ds.DrawText("wooo", Vector2.Zero, Colors.Aqua);
                
            ds.Transform = orgTransform;

        }
    }
}
