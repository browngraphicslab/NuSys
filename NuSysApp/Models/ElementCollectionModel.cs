using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ElementCollectionModel
    {
        public List<ElementInstanceModel> Children { get; set; } 

        public ElementCollectionModel()
        {
            Children = new List<ElementInstanceModel>();
        }
    }
}
