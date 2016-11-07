using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// a flyout list, similar to a menuflyout. 
    /// essentially has a list of options like menu drop downs.
    /// these options are buttons that trigger actions.
    /// </summary>
    public class FlyoutPopup : PopupUIElement
    {
        public FlyoutPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }
    }
}
