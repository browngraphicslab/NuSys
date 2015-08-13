using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    public abstract class Atom
    {
        public Atom()
        { }

        public Color Color { get; set; }

        public int ID { get; set; }
    } 
}
