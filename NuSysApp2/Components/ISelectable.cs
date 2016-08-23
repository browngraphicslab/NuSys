using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp2
{
    public interface ISelectable
    {

        PointCollection ReferencePoints { get; }
        bool IsSelected { get; set; }
        bool ContainsSelectedLink { get; }
    }
}