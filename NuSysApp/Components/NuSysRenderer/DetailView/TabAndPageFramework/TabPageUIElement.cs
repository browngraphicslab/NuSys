using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// An individual page that is one tab in a TabControl
    /// </summary>
    class TabPageUIElement : RectangleUIElement
    {
        /// <summary>
        /// The name of the TabPage
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Contruct a tab page with a given name
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="name">The name of the tab page</param>
        public TabPageUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string name) : base(parent, resourceCreator)
        {
            Name = name;
        }

        /// <summary>
        /// Construct a tab page with a given name and default content.
        /// Useful if you already have a RectangleUIElement and you want to put it in a tab.
        /// Alternatively, you can also just add the RectangleUIElement as a child.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="name">The name of the tab page</param>
        /// <param name="content">The RectangleUIElement set the content to</param>
        public TabPageUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, string name,
            RectangleUIElement content) : base(parent, resourceCreator)
        {
            Name = name;
            AddChild(content);
        }
    }
}
