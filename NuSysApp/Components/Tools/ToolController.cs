using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ToolController
    {
        public delegate void FilterChangedEventHandler(object sender, ToolModel.FilterTitle filter);
        public delegate void SelectionChangedEventHandler(object sender, string selection);
        public delegate void LibraryIdsChangedEventHandler(object sender, List<string> libraryIds);

        public event FilterChangedEventHandler FilterChanged;
        public event SelectionChangedEventHandler SelectionChanged;
        public event LibraryIdsChangedEventHandler LibraryIdsChanged;

        public ToolModel Model { get; private set; }
        public ToolController(ToolModel model)
        {
            Debug.Assert(model != null);
            Model = model;
        }

        public void SetFilter(ToolModel.FilterTitle filter)
        {
            Model.SetFilter(filter);
            FilterChanged?.Invoke(this,filter);
        }

        public void SetSelection(string selection)
        {
            Model.SetSelection(selection);

        }

    }
}
