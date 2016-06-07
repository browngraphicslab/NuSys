using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Components.Viewers.Detail.Views
{

    /// <summary>
    /// Wrapper important for metadeta editor
    /// </summary>
    public class Entry
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public bool Mutability { get; set; }
    }
    
}
