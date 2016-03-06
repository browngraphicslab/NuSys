using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LibraryElementCollectionModel
    {
        public List<ElementModel> Children { get; set; } 

        public LibraryElementCollectionModel()
        {
            Children = new List<ElementModel>();
        }
    }
}
