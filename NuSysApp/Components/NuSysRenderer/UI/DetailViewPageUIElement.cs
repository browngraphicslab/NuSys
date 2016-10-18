using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class DetailViewPageUIElement : IComparable<DetailViewPageUIElement>
    {

        public DetailViewPageType Type;

        public string LibraryElementId;

        public DetailViewPageUIElement(string libraryElementId, DetailViewPageType type)
        {
            Type = type;
            LibraryElementId = libraryElementId;
        }



        public int CompareTo(DetailViewPageUIElement other)
        {
            if (Type == other.Type && LibraryElementId == other.LibraryElementId)
            {
                return 0;
            }
            return 1;
        }
    }
}
