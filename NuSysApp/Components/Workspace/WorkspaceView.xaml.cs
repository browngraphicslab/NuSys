using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Animation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NuSysApp
{
    /// <summary>
    /// This is the view for the entire workspace. It instantiates the WorkspaceViewModel. 
    /// </summary>
    public sealed partial class WorkspaceView
    {
        private AbstractWorkspaceViewMode _mode;

        public WorkspaceView()
        {
            this.InitializeComponent();
        }

        public MultiSelectMenuView MultiMenu
        {
            get { return multiMenu; }
        }

        public Canvas Wrapper
        {
            get { return xWrapper; }
        }


        public InqCanvasView InqCanvas
        {
            get { return xInqCanvas; }
        }

        public async Task SetViewMode(AbstractWorkspaceViewMode mode, bool isFixed = false)
        {
            var deactivate = _mode?.Deactivate();
            if (deactivate != null) await deactivate;
            _mode = mode;
            await _mode.Activate();
        }

        public async void SwitchMode(Options mode, bool isFixed)
        {
            switch (mode)
            {
                case Options.SelectNode:
                    await
                        SetViewMode(new MultiMode(this, new IntersectGroupNodeMode(this), new DuplicateNodeMode(this), new GroupNodeMode(this), new PanZoomMode(this), new SelectMode(this),
                            new FloatingMenuMode(this)));
                    break;
                case Options.SelectMarquee:
                    await SetViewMode(new MultiMode(this, new MultiSelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.PenGlobalInk:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    // TODO: delegate to workspaceview
                    //InqCanvas.SetErasing(false);
                    break;
                case Options.AddTextNode:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Text, isFixed),
                            new FloatingMenuMode(this)));
                    break;
                case Options.AddAudioCapture:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Audio, isFixed),
                            new FloatingMenuMode(this)));
                    break;
                case Options.AddMedia:
                    await
                        SetViewMode(new MultiMode(this, new SelectMode(this),
                            new AddNodeMode(this, NodeType.Document, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.PenErase:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    // TODO: delegate to workspaceview
                    //InqCanvas.SetErasing(true);
                    break;
                case Options.PenHighlight:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new FloatingMenuMode(this)));
                    // TODO: delegate to workspaceview
                    //                    InqCanvas.SetHighlighting(true);
                    break;
                case Options.MiscSave:
                    SessionController.Instance.SaveWorkspace();
                    break;
                case Options.MiscLoad:
                    SessionController.Instance.LoadWorkspace();
                    break;
                case Options.MiscPin:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this), new PinMode(this)));
                    break;
                case Options.AddBucket:
                    await SetViewMode(new MultiMode(this, new PanZoomMode(this)));
                    break;
                case Options.AddVideo:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Video, isFixed),
                            new FloatingMenuMode(this)));
                    break;
            }
        }
    }
}