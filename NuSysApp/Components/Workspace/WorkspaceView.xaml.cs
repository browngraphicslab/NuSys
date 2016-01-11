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
        private InqCanvasView _inqCanvas;

        public WorkspaceView(WorkspaceViewModel vm)
        {
            this.InitializeComponent();
            var wsModel = (WorkspaceModel)vm.Model;


            var inqCanvasModel = wsModel.InqCanvas;
            var inqCanvasViewModel = new InqCanvasViewModel(inqCanvasModel, new Size(Constants.MaxCanvasSize, Constants.MaxCanvasSize));
            //SessionController.Instance.IdToSendables[inqCanvasModel.Id] = inqCanvasModel;
            _inqCanvas = new InqCanvasView(inqCanvasViewModel);
            _inqCanvas.Width = Constants.MaxCanvasSize;
            _inqCanvas.Height = Constants.MaxCanvasSize;
            xWrapper.Children.Add(_inqCanvas);
            //wsModel.InqCanvas = inqCanvasModel;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                SwitchMode(Options.SelectNode, false);
            };

            wsModel.InqCanvas.LineFinalized += async delegate (InqLineModel model)
            {
                var gestureType = GestureRecognizer.testGesture(model);
                switch (gestureType)
                {
                    case GestureRecognizer.GestureType.None:
                        break;
                    case GestureRecognizer.GestureType.Scribble:
                        vm.CheckForInkNodeIntersection(model);
                        model.Delete();
                        break;
                }

                await CheckForGroupCreation(model);
            };
        }

        private async Task<bool> CheckForGroupCreation(InqLineModel line)
        {
            var wsmodel = (DataContext as WorkspaceViewModel).Model as WorkspaceModel;
            var Model = wsmodel.InqCanvas;
            var outerRect = Geometry.PointCollecionToBoundingRect(line.Points.ToList());

            if (outerRect.Width * outerRect.Height < 100 * 100)
                return false;

            var idsToDelete = new List<string>();
            var encompassedLines = new List<InqLineModel>();
            foreach (var otherLine in Model.Lines.Where(l => l != line))
            {
                var innerRect = Geometry.PointCollecionToBoundingRect(otherLine.Points.ToList());
                var innerRect2 = new Rect(innerRect.X, innerRect.Y, innerRect.Width, innerRect.Height);
                innerRect.Intersect(outerRect);
                if (Math.Abs(innerRect2.Width - innerRect.Width) < 70 && Math.Abs(innerRect2.Height - innerRect.Height) < 70)
                {

                    idsToDelete.Add(otherLine.Id);
                    InqLineModel newModel = new InqLineModel(DateTime.UtcNow.Ticks.ToString())
                    {
                        Stroke = otherLine.Stroke,
                        StrokeThickness = otherLine.StrokeThickness
                    };

                    foreach (var point in otherLine.Points)
                    {
                        newModel.AddPoint(new Point2d(point.X - outerRect.X, point.Y - outerRect.Y));
                    }
                    encompassedLines.Add(newModel);
                }
            }


            var first = line.Points.First();
            var last = line.Points.Last();
            if (encompassedLines.Count == 0 || (Math.Abs(first.X - last.X) > 40 && Math.Abs(first.Y - last.Y) > 40))
            {
                return false;
            }

            /*
            foreach (var idToDelete in idsToDelete)
            {
                NetworkConnector.Instance.RequestDeleteSendable(idToDelete);
            }
           


            var title = await InkToText(encompassedLines);
            var dict = new Dictionary<string, string>();
            dict["title"] = title;
            Action<string> addCallback = delegate (string s)
            {
                NetworkConnector.Instance.RequestDeleteSendable(line.Id);
                var v = SessionController.Instance.IdToSendables[s] as TextNodeModel;
                if (v != null)
                {
                    Debug.Assert(encompassedLines.Count > 0);
                    foreach (var model in encompassedLines)
                    {
                        UITask.Run(async () =>
                        {
                            //NetworkConnector.Instance.RequestLock(v.ID);
                            NetworkConnector.Instance.RequestFinalizeGlobalInk(model.Id, v.InqCanvas.ID, model.GetString());
                            //is the model being deleted and then trying to be added? is the canvas fully there when we try to add?
                        });

                    }
                }
            };
            var tagNodePos = new Point(outerRect.X + outerRect.Width / 6, outerRect.Y + outerRect.Height / 6);
            await NetworkConnector.Instance.RequestNewGroupTag(tagNodePos.X.ToString(), tagNodePos.Y.ToString(), title, dict, addCallback);
            return true;
             */
            return false;
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
            get { return _inqCanvas; }
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
            SessionController.Instance.SessionView.HideRecorder();
            //SessionController.Instance.SessionView.FloatingMenu.Reset();
            switch (mode)
            {
                case Options.SelectNode:
                    var nodeManipulationMode = new NodeManipulationMode(this);
                    await
                        SetViewMode(new MultiMode(this, nodeManipulationMode, new DuplicateNodeMode(this), new PanZoomMode(this), new SelectMode(this),
                            new FloatingMenuMode(this), new CreateGroupMode(this, nodeManipulationMode)));
                    break;
                case Options.SelectMarquee:
                    await SetViewMode(new MultiMode(this, new MultiSelectMode(this), new FloatingMenuMode(this)));
                    break;
                case Options.PenGlobalInk:
                    await SetViewMode(new MultiMode(this, new GlobalInkMode(this), new LinkMode(this)));
                    // TODO: delegate to workspaceview
                    //InqCanvas.SetErasing(false);
                    break;
                case Options.AddTextNode:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Text, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddWeb:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Web, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddAudioCapture:
                    await
                        SetViewMode(new MultiMode(this, new AddNodeMode(this, NodeType.Audio, isFixed),new FloatingMenuMode(this)));
                    break;
                case Options.AddMedia:
                    await
                        SetViewMode(new MultiMode(this, new SelectMode(this), new AddNodeMode(this, NodeType.Document, isFixed), new FloatingMenuMode(this)));
                    break;
                case Options.AddRecord:
                    var sessionView = SessionController.Instance.SessionView;
                    sessionView.ShowRecorder();
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
                    SessionController.Instance.SessionView.FloatingMenu.Reset();
                    break;
                case Options.MiscLoad:
                    SessionController.Instance.LoadWorkspace();
                    SessionController.Instance.SessionView.FloatingMenu.Reset();
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