using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class CortanaMode : AbstractWorkspaceViewMode
    {
        private readonly Point _placementPos = new Point(500, 100);
        public CortanaMode(WorkspaceView view) : base(view)
        {
        }

        public override async Task Activate()
        {
            var command = await Cortana.RunRecognizer();
            await ProcessCommand(command);
        }

        public override async Task Deactivate()
        {
        }

        private async Task ProcessCommand(string command)
        {
            switch (command)
            {
                case "open document":
                    await AddNodeMode.AddNode(_view, _placementPos, NodeType.Document);
                    break;
                case "create text":
                    await AddNodeMode.AddNode(_view, _placementPos, NodeType.Text);
                    break;
                case "create ink":
                    await AddNodeMode.AddNode(_view, _placementPos, NodeType.Ink);
                    break;
            }
        }
    }
}
