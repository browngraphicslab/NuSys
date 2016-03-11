using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AreaNodeViewModel : NodeContainerViewModel
    {

        public PointCollection Points { get; set; }

        public AreaNodeViewModel(AreaModel model):base(model)
        {
            Points = new PointCollection();
            model.Points.ForEach((p => Points.Add(p)));
        }

    }
}
