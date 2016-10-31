using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    internal class DetailViewAliasesPage : RectangleUIElement
    {
        private StackLayoutManager _layoutManager;
        private RectangleUIElement _aliasesList;

        public DetailViewAliasesPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :
            base(parent, resourceCreator)
        {
            _aliasesList = new RectangleUIElement(parent, resourceCreator);
            _aliasesList.Background = Colors.Aqua;
            _layoutManager = new StackLayoutManager(StackAlignment.Vertical);
            AddChild(_aliasesList);
            _layoutManager.AddElement(_aliasesList);

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _layoutManager.SetSize(Width, Height);
            _layoutManager.VerticalAlignment = VerticalAlignment.Center;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _layoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}