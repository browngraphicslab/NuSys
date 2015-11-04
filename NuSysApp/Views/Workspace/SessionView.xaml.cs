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

namespace NuSysApp
{

    public sealed partial class SessionView : Page
    {
        #region Private Members

        private int _penSize = Constants.InitialPenSize;
        private bool _cortanaInitialized;
        private CortanaMode _cortanaModeInstance;

        #endregion Private Members

        public SessionView()
        {
            this.InitializeComponent();
     
            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                Clip = new RectangleGeometry { Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height) };
            };

            // InqCanvasModel inqCanvasModel = new InqCanvasModel("WORKSPACE_ID");
            //   new InqCanvasViewModel(inqCanvas, inqCanvasModel);
            // var vm = new WorkspaceViewModel(new WorkSpaceModel(inqCanvasModel));
            // this.DataContext = vm;

          //  await xWorkspace.SetViewMode(new MultiMode(xWorkspace, new PanZoomMode(xWorkspace), new SelectMode(xWorkspace), new FloatingMenuMode(xWorkspace)));

            _cortanaInitialized = false;
            xFloatingMenu.SessionView = this;
            xFloatingMenu.ModeChange += xWorkspace.SwitchMode;
        }

        
        public Canvas MainCanvas
        {
            get { return mainCanvas; }
        }

        public void RemoveLoading()
        {
            //TODO remove a loading screen
        }
        
        private async void OnDrop(object sender, DragEventArgs e)
        {
            string text = await e.Data.GetView().GetTextAsync();
            var pos = e.GetPosition(this);
            var vm = (WorkspaceViewModel)this.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            var props = new Dictionary<string, string>();
            props["width"] = "400";
            props["height"] = "300";
            await NetworkConnector.Instance.RequestMakeNode(p.X.ToString(), p.Y.ToString(), NodeType.Text.ToString(), text, null, props);
        }
    }
}