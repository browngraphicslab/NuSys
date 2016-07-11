using System.Collections.Generic;

namespace NuSysApp.Tools
{
    public interface ToolViewable
    {
        void SetProperties(List<string> propertiesList);

        void Dispose();

        void SetViewSelection(List<string> selection);

    }
}