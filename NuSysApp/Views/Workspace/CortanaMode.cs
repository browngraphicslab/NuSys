﻿using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class CortanaMode : AbstractWorkspaceViewMode
    {
        private readonly Point _defaultPlacementPos = new Point(500, 100);
        public CortanaMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            var command = await Cortana.RunRecognizer();
            //var command = await new CortanaContinuousRecognition().RunContinuousRecognizerAndReturnResult();
            await ProcessCommand(command);
        }

        public override async Task Deactivate() { }

        private async Task ProcessCommand(string command)
        {
            switch (command.ToLower())
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
                    await AddNodeMode.AddNode(_view, _defaultPlacementPos, NodeType.Text, command);
                    break;
            }
        }
    }
}
