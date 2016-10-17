using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class UserBubbles : RectangleUIElement
    {
        private List<ButtonUIElement> Bubbles;

        public UserBubbles(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            base.Width = 50;
            base.Height = 100;

            Bubbles = new List<ButtonUIElement>();
            
        }
        
    }
}
