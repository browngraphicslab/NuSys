using System;

namespace NuSysApp
{
    public class MetadataToolModel : ToolModel
    {

        public ToolFilterTypeTitle Filter { get; private set; }
        public Tuple<string, string> Selection { get; protected set; }

        public void SetSelection(Tuple<string, string> selection)
        {

            Selection = selection;
        }
        public void SetFilter(ToolFilterTypeTitle filter)
        {
            Filter = filter;
        }
    }
}