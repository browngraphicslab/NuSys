using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class GroupNodeViewModel : NodeContainerViewModel
    {
        public GroupNodeViewModel(NodeContainerModel model) : base(model)
        {
            var count = 0;
            base.ChildAdded += async delegate(object source, AnimatableUserControl node)
            {
                var vm = ((AtomViewModel)node.DataContext);
                vm.Height = 50;
                vm.X = Width/2 - 100;
                vm.Y = 80 * (++count) + 10;


            };

       
        }
        
    }
}
