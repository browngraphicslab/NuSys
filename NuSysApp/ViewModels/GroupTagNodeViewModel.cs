using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupTagNodeViewModel : NodeViewModel
    {
      
        public GroupTagNodeViewModel(NodeModel model) : base(model)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            
        }

        public string Title
        {
            get { return ((NodeModel)Model).Title; }
            set { ((NodeModel) Model).Title = value; }
           
        }
    }
}
