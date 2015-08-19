using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    partial class Cortana
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
                    IsRunning = true;
                    WorkspaceView.CortanaRunning = true;
                    while (WorkspaceView.CortanaRunning)
                    {
                        var dictation = await CortanaContinuousRecognition.RunRecognizerAndReturnResult();
                        await ProcessCommand(dictation);
                    }
                }
            }

            public override async Task Deactivate()
            {
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
