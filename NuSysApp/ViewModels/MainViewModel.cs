using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class MainViewModel : AtomViewModel
    {
        public MainViewModel(MainModel model) : base(model)
        {
            Model = model;
        }

        public MainModel Model { get; set; }

        public override void Remove() { }

        public override void UpdateAnchor() { }
    }
}
