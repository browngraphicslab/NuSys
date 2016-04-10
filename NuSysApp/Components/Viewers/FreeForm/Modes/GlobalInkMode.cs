
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GlobalInkMode : AbstractWorkspaceViewMode
    {
        private FreeFormViewer _cview;

        public GlobalInkMode(FreeFormViewer view) : base(view)
        {
            _cview = (FreeFormViewer)view;
            _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
        }

        public override async Task Activate()
        {
            _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Ink;
            
        }

        public override async Task Deactivate()
        {
            _cview.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
        }
    }
}
