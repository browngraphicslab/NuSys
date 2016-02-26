using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class GroupNodeViewModel : ElementCollectionInstanceViewModel
    {
        public GroupNodeViewModel(ElementCollectionInstanceController controller) : base(controller)
        {

            _nodeViewFactory = new GroupItemThumbFactory(); 
        }

  
    }
}
