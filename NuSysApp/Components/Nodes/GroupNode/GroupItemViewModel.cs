using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupItemViewModel
    {

        public GroupItemViewModel(AtomModel model )
        {
            Model = model;
            Id = model.Id;
        }

        public AtomModel Model { get; set; }
        public string Id { get; set; }

    }
}
