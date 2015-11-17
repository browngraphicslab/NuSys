using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupTagNodeViewModel : GroupViewModel
    {
        public GroupTagNodeViewModel(GroupModel model) : base(model)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
        }

        public override async void OnChildAdded(object source, Sendable nodeModel)
        {
            var view = await _nodeViewFactory.CreateFromSendable(nodeModel, AtomViewList.ToList());
            AtomViewList.Add(view);
            view.IsHitTestVisible = false;
        }

        public string Title
        {
            get { return ((NodeModel)Model).Title; }
            set
            {
                ((NodeModel) Model).Title = value;
                RaisePropertyChanged("Title");
            }
           
        }
    }
}
