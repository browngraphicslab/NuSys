using System.Collections.Generic;

namespace NuSysApp.Tools
{
    public interface ToolViewable
    {
        void SetProperties(List<string> propertiesList);

        void Dispose();

        void SetVisualSelection(HashSet<string> selection);

    }
}