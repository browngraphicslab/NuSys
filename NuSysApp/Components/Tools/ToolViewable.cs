using System.Collections.Generic;
using NuSysApp.Components.Tools;

namespace NuSysApp.Tools
{
    public interface ToolViewable
    {
        void SetProperties(List<string> propertiesList);

        void Dispose();

        void SetViewSelection(string selection);

        void SetSize(double width, double height);
    }
}