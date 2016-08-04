using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public abstract class DetailHomeTabView: UserControl
    {

        public event ContentLoadedEventHandler TimeChanged;
        public delegate void ContentLoadedEventHandler(object sender);

        //public DetailHomeTabView();
    }
}
