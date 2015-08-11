using System.Collections.Generic;

namespace NuSysApp.Views.Workspace
{
    public class MultiMode : AbstractWorkspaceViewMode
    {
        private readonly List<AbstractWorkspaceViewMode> _modes = new List<AbstractWorkspaceViewMode>();

        public MultiMode(WorkspaceView view, params AbstractWorkspaceViewMode[] modes):base(view)
        {
            _modes.AddRange(modes);
        }

        public override void Activate()
        {
            _modes.ForEach((m) => m.Activate());
        }

        public override void Deactivate()
        {
            _modes.ForEach((m) => m.Deactivate());
        }
    }
}
