using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionController : LibraryElementController
    {
        public delegate void SelectHandler(RegionController regionController);
        public event SelectHandler OnSelect;

        public delegate void DeselectHandler(RegionController regionController);
        public event DeselectHandler OnDeselect;

        private bool _selected;
        public RegionController(Region model) : base(model)
        {
        }

        public void Select()
        {
            _selected = true;
            OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            _selected = false;
            OnDeselect?.Invoke(this);
        }
    }
}
