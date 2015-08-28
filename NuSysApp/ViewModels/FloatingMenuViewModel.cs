using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class FloatingMenuViewModel
    {
        public FloatingMenuViewModel(FloatingMenuModel model)
        {
            this.View = new FloatingMenuView();
        }

        public FloatingMenuView View { get; set; }
    }
}
