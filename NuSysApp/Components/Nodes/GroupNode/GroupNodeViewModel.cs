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
    public class GroupNodeViewModel : NodeContainerViewModel
    {
        public GroupNodeViewModel(NodeContainerModel model) : base(model)
        {
            var count = 0;
            base.ChildAdded += async delegate(object source, FrameworkElement node)
            {
                //node.Margin = new Thickness(0, -70 * Math.Min(0,count), 0, 0);
              //  node.RenderTransform = new TranslateTransform {Y = -70*Math.Min(count,1)};
                //node.HorizontalAlignment = HorizontalAlignment.Center;
                //node.VerticalAlignment = VerticalAlignment.Center;
                count++;
            };

            _nodeViewFactory = new NodeContentThumbViewFactory(); 
        }
    }
}
