using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ChatBoxUIElement : ResizeableWindowUIElement
    {
        private RectangleUIElement _typingRect;
        private RectangleUIElement _readingRect;

        private StackLayoutManager _baseLayoutManager;

        public ChatBoxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the min height and min width
            MinWidth = 200;
            MinHeight = 200;
            Width = 300;
            Height = 300;
            KeepAspectRatio = false;


            // instantiate the typing and reading rectangles
            _typingRect = new RectangleUIElement(this, resourceCreator)
            {
                BorderWidth = 3,
                Bordercolor = Colors.DarkGray
            };
            AddChild(_typingRect);
            _readingRect = new RectangleUIElement(this, resourceCreator)
            {
                Background = Colors.Beige //maybe change this color just for differentiation right now
            };
            AddChild(_readingRect);

            // instantiate a base layout manager to take care of the general sizing of the typing and reading rectangles
            _baseLayoutManager = new StackLayoutManager(StackAlignment.Vertical);
            _baseLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _baseLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _baseLayoutManager.AddElement(_typingRect);
            _baseLayoutManager.AddElement(_readingRect);
            _baseLayoutManager.ArrangeItems();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // arrange the items in the base layout manager
            _baseLayoutManager.SetSize(Width, Height);
            _baseLayoutManager.ArrangeItems();

            // resize so the typing rect is smaller than the reading rect
            _typingRect.Height = 50;
            _readingRect.Height = Height - _typingRect.Height - TopBarHeight;
            _readingRect.Transform.LocalPosition = new Vector2(_readingRect.Transform.LocalPosition.X, TopBarHeight);
            _typingRect.Transform.LocalPosition = new Vector2(_typingRect.Transform.LocalPosition.X, TopBarHeight + _readingRect.Height);





            base.Update(parentLocalToScreenTransform);
        }
    }
}
