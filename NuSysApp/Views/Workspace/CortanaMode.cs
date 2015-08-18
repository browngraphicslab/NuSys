using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
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
                while (true)
                {
                    var dictation = await CortanaContinuousRecognition.RunContinuousRecognizerAndReturnResult();
                    await ProcessCommand(dictation);
                }
            }
            //Deactivate();
        }

        public override async Task Deactivate()
        {
            IsRunning = false;
            WorkspaceView.CortanaRunning = false;
        }

        private async Task ProcessCommand(string dictation) // bug: sometimes dictation is ""
        {
            switch (dictation.ToLower())
            {
                case null:
                    break;
                case "open document":
                    await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Document);
                    break;
                case "create text":
                    await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Text);
                    break;
                case "create ink":
                    await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Ink);
                    break;
                default:
                    await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Text, dictation);
                    break;
            }
        }
    }
}
