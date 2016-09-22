using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class InkOptionsRenderItem : BaseRenderItem
    {
        private Rect _bg;
        private WrapRenderItem _colorContainer;
        private WrapRenderItem _widthContainer;
        private CanvasTextFormat _textformat;
        private InkOptionsColorRenderItem _activeColor;
        private InkOptionsWidthRenderItem _activeWidth;
        private CloseButtonRenderItem _btnClose;

        private Color[] _colors = {Colors.IndianRed, Colors.DarkOrange, Colors.Yellow, Colors.LightGreen, Colors.MediumPurple, Colors.LightBlue, Colors.HotPink, Colors.Black, Colors.White, Colors.Gray};
    
        public InkOptionsRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _textformat = new CanvasTextFormat {FontSize = 12};
        }

        public async override Task Load()
        {
            _btnClose = new CloseButtonRenderItem(Parent, ResourceCreator);
            await _btnClose.Load();
            _btnClose.Transform.LocalPosition = new Vector2(260,13);
            _btnClose.Tapped += BtnCloseOnTapped;
            AddChild(_btnClose);

            _colorContainer = new WrapRenderItem(280, Parent, ResourceCreator);
            _colorContainer.Transform.LocalX = 20;
            _colorContainer.Transform.LocalY = 50;

            for (int i  = 0; i < 10; i++)
            {
                var colorItem = new InkOptionsColorRenderItem(_colors[i], Parent, ResourceCreator);
                colorItem.Tapped += ColorItemOnTapped;
                _colorContainer.AddChild(colorItem);
            }
            AddChild(_colorContainer);

            _widthContainer = new WrapRenderItem(280, Parent, ResourceCreator);
            _widthContainer.Transform.LocalX = 20;
            _widthContainer.Transform.LocalY = 150;
            for (int i = 0; i < 5; i++)
            {
                var widthItem = new InkOptionsWidthRenderItem((1 + i)*2, Parent, ResourceCreator);
                widthItem.Tapped += WidthItemOnTapped;
                _widthContainer.AddChild(widthItem);
            }

            AddChild(_widthContainer);

            base.Load();
        }

        private void BtnCloseOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            IsVisible = false;
        }

        private void WidthItemOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            foreach (var child in _widthContainer.GetChildren())
            {
                var width = (InkOptionsWidthRenderItem) child;
                width.IsActive = false;
            }
            _activeWidth = (InkOptionsWidthRenderItem)item;
            _activeWidth.IsActive = true;

            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.InkRenderItem.InkSize = _activeWidth.Radius;
        }

        private void ColorItemOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            foreach (var child in _colorContainer.GetChildren())
            {
                var color = (InkOptionsColorRenderItem)child;
                color.IsActive = false;
            }
            _activeColor = (InkOptionsColorRenderItem)item;
            _activeColor.IsActive = true;

            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.InkRenderItem.InkColor = _activeColor.Color;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
            _bg = new Rect(0,0, 290, 200);

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed || !IsVisible)
                return;

            ds.Transform = Transform.LocalToScreenMatrix;
            _textformat = new CanvasTextFormat { FontSize = 16 };
            ds.FillRectangle(_bg, Colors.DarkSlateGray);
            ds.FillRectangle(_bg, Color.FromArgb(0xFF, 0xC7, 0xDE, 0xDE)); 
            ds.DrawRectangle(_bg, Color.FromArgb(0xFF, 0x6B, 0x93, 0x97));
            ds.DrawText("Color", new Vector2(20, 20), Color.FromArgb(0xFF,0x11,0x3D,0x40), _textformat);
            ds.DrawText("Size", new Vector2(20, 120), Color.FromArgb(0xFF, 0x11, 0x3D, 0x40), _textformat);

            base.Draw(ds);
        }

        public override Rect GetMeasure()
        {
            return _bg;
        }
    }
}
