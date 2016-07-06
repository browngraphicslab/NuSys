using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class MultiMode : AbstractWorkspaceViewMode
    {
        private readonly List<AbstractWorkspaceViewMode> _modes = new List<AbstractWorkspaceViewMode>();

        public MultiMode(FrameworkElement view, params AbstractWorkspaceViewMode[] modes):base(view)
        {
            _modes.AddRange(modes);
        }

        public override async Task Activate()
        {
            _modes.ForEach(async (m) => await m.Activate());
        }

        public override async Task Deactivate()
        {
            _modes.ForEach(async (m) => await m.Deactivate());
        }
    }
}
