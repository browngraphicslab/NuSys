using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ToolWindow : ResizeableWindowUIElement
    {
        private ButtonUIElement _deleteButton;
        private ButtonUIElement _refreshButton;
        private const int BUTTON_MARGIN = 10;
        public ToolWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            var deleteCircleShape = new EllipseUIElement(this, resourceCreator)
            {
                Background = Colors.Red,
                Width = 50,
                Height = 50,
            };
            _deleteButton = new ButtonUIElement(this, resourceCreator, deleteCircleShape);
            _deleteButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width/2), _deleteButton.Height/2 + BUTTON_MARGIN);
            _children.Add(_deleteButton);

            var refreshCircleShape = new EllipseUIElement(this, resourceCreator)
            {
                Background = Colors.Blue,
                Width = 50,
                Height = 50,
            };
            _refreshButton = new ButtonUIElement(this, resourceCreator, refreshCircleShape);
            _refreshButton.Transform.LocalPosition = new Vector2(-(BUTTON_MARGIN + _deleteButton.Width / 2), _deleteButton.Transform.LocalY + _deleteButton.Height + BUTTON_MARGIN);
            _children.Add(_refreshButton);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
        }
    }
}