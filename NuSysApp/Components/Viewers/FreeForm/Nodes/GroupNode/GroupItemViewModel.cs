using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupItemViewModel
    {

        public GroupItemViewModel(ElementModel model )
        {
            Model = model;
            Id = model.Id;
        }

        public ElementModel Model { get; set; }
        public string Id { get; set; }

    }
}
