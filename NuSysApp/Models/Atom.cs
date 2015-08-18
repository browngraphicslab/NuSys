using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public abstract class Atom
    {
        public Atom(int id)
        {
        }

        public SolidColorBrush Color { get; set; }

        public int ID { get; set; }

    } 
}
