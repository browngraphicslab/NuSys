using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace NuSysApp.Components.Viewers.FreeForm
{
    class FocusManager
    {

        public BaseRenderItem ActiveFocusElement { get; set; }

        public FocusManager(CanvasInteractionManager cim)
        {

        }

        // Fired when a key is pressed (CoreApplication.MainView.CoreWindow.KeyDown)
        public void OnKeyPressed(object sender, KeyRoutedEventArgs e)
        {

        }

        // Calls LostFocus on the previous base render item, and calls GotFocus on the new BaseRenderItem
        public void ClearFocus()
        {

        }

        // Call LostFocus on previous base render item and set ActiveFocusElement to null
        public void ChangeFocus(BaseRenderItem newBaseRenderItem)
        {

        }
    }
}
