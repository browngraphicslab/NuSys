using System;
using System.Collections.Generic;

namespace NuSysApp2
{
    public class MetadataToolModel : ToolModel
    {

        public ToolFilterTypeTitle Filter { get; private set; }
        public Tuple<string, HashSet<string>> Selection { get; protected set; }

        public MetadataToolModel()
        {
            Selection = new Tuple<string, HashSet<string>>(null, new HashSet<string>());
        }

        public void SetSelection(Tuple<string, HashSet<string>> selection)
        {
            Selection = selection;
        }
        public void SetFilter(ToolFilterTypeTitle filter)
        {
            Filter = filter;
        }
    }
}