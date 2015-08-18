using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class CortanaMode : AbstractWorkspaceViewMode
    {
        private readonly Point _defaultPlacementPos = new Point(500, 100);
        private bool _isActive;

        public CortanaMode(WorkspaceView view) : base(view)
        {
            _isActive = false;
        }

        public override async Task Activate()
        {
            if (!_isActive)
            {
                var dictation = await CortanaContinuousRecognition.RunContinuousRecognizerAndReturnResult();
                await ProcessCommand(dictation);
                _isActive = true;
            }
        }

        public override async Task Deactivate()
        {
            _isActive = false;
        }

        private async Task ProcessCommand(string dictation) // bug: sometimes dictation is ""
        {
            switch (dictation.ToLower())
            {
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
