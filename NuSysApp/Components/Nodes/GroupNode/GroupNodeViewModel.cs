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
                node.Margin = new Thickness(0, -35 * Math.Max(0,Math.Min(count,1)), 0, 0);
                node.VerticalAlignment = VerticalAlignment.Center;
                
                //node.RenderTransform = new TranslateTransform {Y = -70*Math.Min(count,1)};
                //node.HorizontalAlignment = HorizontalAlignment.Center;
                //node.VerticalAlignment = VerticalAlignment.Center;
                count++;
            };
            
            _nodeViewFactory = new GroupItemThumbFactory(); 
        }

        public override void SetSize(double width, double height)
        {
            base.SetSize(width, width);
        }
    }
}
