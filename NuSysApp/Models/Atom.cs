using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class Atom
    {
        public Atom()
        {

        }

        public int ID { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public MatrixTransform Transform { get; set; } 

        public double Width { get; set; }

        public double Height { get; set; }
    }
}
