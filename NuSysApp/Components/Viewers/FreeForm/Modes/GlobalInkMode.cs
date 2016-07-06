
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {
        private FreeFormViewer _cview;

        public GlobalInkMode(FreeFormViewer view) : base(view)
        {
            _cview = (FreeFormViewer)view;
          //  _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;

            /*
            _cview.PointerPressed += delegate(object sender, PointerRoutedEventArgs e )
            {
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                    _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
                //     _cview.Wrapper.IsHitTestVisible = false;
            };

            _cview.PointerReleased += delegate (object sender, PointerRoutedEventArgs e)
            {
                if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
                    _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Ink;
                //      _cview.Wrapper.IsHitTestVisible = true;
            };
            */
        }

        public override async Task Activate()
        {
          //  _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Ink;
            
        }

        public override async Task Deactivate()
        {
          //  _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
        }
    }
}
