using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class CortanaMode : AbstractWorkspaceViewMode
    {

        //public event OnCommandIssuedHandler CommandIssued;
        //public delegate void OnCommandIssuedHandler(string command);

        public CortanaMode(WorkspaceView view) : base(view)
        {
            
        }

        public override void Activate()
        {
            
        }

        public override void Deactivate()
        {
            
        }

        private async void ProcessCommand(string command)
        {
            //CommandIssued?.Invoke(command);
            switch (command)
            {
                case "open document":
                    await AddNode(_view, new Point(500, 100), NodeType.Document);
                    break;
                case "create text":
                    await AddNode(_view, new Point(500, 100), NodeType.Text);
                    break;
                case "create ink":
                    await AddNode(_view, new Point(500, 100), NodeType.Ink);
                    break;
            }
        }

        // copied and pasted from AddNodeMode.cs (this is temporary, will further abstract this later)
        private static async Task AddNode(WorkspaceView view, Point pos, NodeType nodeType)
        {
            var vm = (WorkspaceViewModel)view.DataContext;
            var p = vm.CompositeTransform.Inverse.TransformPoint(pos);
            await vm.CreateNewNode(nodeType, p.X, p.Y, "");
            vm.ClearSelection();
        }
    }

    public class CommandIssuedEventArgs : EventArgs
    {
        public string Command { get; set; }
    }
}
