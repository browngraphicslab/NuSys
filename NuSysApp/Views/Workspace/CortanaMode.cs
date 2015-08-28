using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    partial class CortanaContinuousRecognition
    {
        public class CortanaMode : AbstractWorkspaceViewMode
        {
            private readonly Point _defaultPlacementPos = new Point(500, 100);
            public bool IsRunning { get; set; }

            public CortanaMode(WorkspaceView view) : base(view)
            {
                IsRunning = false;
            }

            public override async Task Activate()
            {
                if (!IsRunning)
                {
                    Debug.WriteLine("Cortana activated");
                    IsRunning = true;
                    WorkspaceView.CortanaRunning = true;
                    while (WorkspaceView.CortanaRunning)
                    {
                        var dictation = await RunRecognizerAndReturnResult();
                        await ProcessCommand(dictation);
                    }
                }
            }

            public override async Task Deactivate()
            {
                Debug.WriteLine("Cortana deactivated");
                IsRunning = false;
                WorkspaceView.CortanaRunning = false;
            }

            private async Task ProcessCommand(string dictation)
            {
                switch (dictation)
                {
                    case null:
                    case "":
                        break;
                    case OpenDocumentCommand:
                        await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Document);
                        break;
                    case CreateTextCommand:
                        await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Text);
                        break;
                    case CreateInkCommand:
                        await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Ink);
                        break;
                    default:
                        await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Text, dictation);
                        break;
                }
            }
        }
    }
}
