using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class GroupItemViewModel : ElementViewModel
    {

        public GroupItemViewModel(ElementController controller):base(controller)
        {
            Model = controller.Model;
            Id = controller.Model.Id;
        }

        public ElementModel Model { get; set; }
        public string Id { get; set; }

    }
}
