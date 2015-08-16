using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class Atom
    {
        public Atom(string id)
        {
        }

        public SolidColorBrush Color { get; set; }

        public string ID { get; set; }
    } 
}
