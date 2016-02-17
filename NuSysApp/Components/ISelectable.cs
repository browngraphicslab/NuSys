using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Components
{
    public interface ISelectable { 
    
        void select();
        void deselect();
        bool isSelected();
        PointCollection ReferencePoints { get; }
        bool Selected { get; set; }
        bool ContainsLink { get; }

        void Translate(double x, double y);
    }
}
