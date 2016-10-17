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
    public class BaseHomePage : RectangleUIElement
    {
        // This is the layout manager for the content of the home page
        private StackLayoutManager _contentStackLayoutManager;

        // This is the content that the home page displays. Will change depending on element type.
        private RectangleUIElement _content;
        public BaseHomePage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // initialize the layout manager
            _contentStackLayoutManager = new StackLayoutManager();

            // Placeholder for testing.
            _content = new RectangleUIElement(parent, resourceCreator)
            {
                Background = Colors.BurlyWood,
                Height = 300,
                Width = 300
            };

            // Add placeholder to layout manager and base.
            _contentStackLayoutManager.AddElement(_content);
            AddChild(_content);

        }

        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
        }

        /// <summary>
        /// Replaces the current content displayed on the home page with the element being passed in.
        /// By default, replaces the place holder with element.
        /// </summary>
        /// <param name="element"></param>
        public void SetContent(RectangleUIElement element)
        {
            RemoveChild(_content);
            _contentStackLayoutManager.Remove(_content);
            _content = element;
            AddChild(_content);
            _contentStackLayoutManager.AddElement(_content);

        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // Rearrange the items in the layout manager.
            _contentStackLayoutManager.SetMargins(25);
            _contentStackLayoutManager.TopMargin = 150;
            _contentStackLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _contentStackLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _contentStackLayoutManager.Width = Width;
            _contentStackLayoutManager.Height = Height;
            _contentStackLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }


    }
}
