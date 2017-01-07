using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class DynamicTextboxUIElement : TextboxUIElement
    {
        public override float Width { get; set; }

        public override float Height { get; set; }

        public DynamicTextboxUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }


        ///// <summary>
        ///// Update text layout to the current text
        ///// </summary>
        ///// <param name="resourceCreator"></param>
        ///// <returns></returns>
        //public virtual CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator)
        //{
        //    var textLayout = _scrollVert ? new CanvasTextLayout(resourceCreator, Text, TextFormat,
        //                                   Width - 2 * (BorderWidth + UIDefaults.XTextPadding), float.MaxValue) :
        //                                   new CanvasTextLayout(resourceCreator, Text, TextFormat, float.MaxValue,
        //                                   Height - 2 * (BorderWidth + UIDefaults.YTextPadding));

        //    return textLayout;
        //}
    }
}
