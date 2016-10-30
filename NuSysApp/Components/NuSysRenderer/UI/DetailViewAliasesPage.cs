using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using SharpDX;

namespace NuSysApp
{
    class DetailViewAliasesPage : RectangleUIElement
    {
        private StackLayoutManager _layoutManager;
        private RectangleUIElement _aliasesList;

        public DetailViewAliasesPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :
            base(parent, resourceCreator)
        {
            _aliasesList = new RectangleUIElement(parent, resourceCreator);
            _aliasesList.Background = Colors.Aquamarine;
            AddChild(_aliasesList);
            _layoutManager.AddElement(_aliasesList);
        }

        public override void Update(System.Numerics.Matrix3x2 parentLocalToScreenTransform)
        {
            _layoutManager.SetSize(Width, Height);
            _layoutManager.VerticalAlignment = VerticalAlignment.Center;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _layoutManager.ItemWidth = Width - 50;
            _layoutManager.ItemHeight = Height - 50;
            _layoutManager.TopMargin = 25;
            _layoutManager.LeftMargin = 25;
            _layoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
