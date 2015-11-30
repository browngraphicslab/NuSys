using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupTagNodeViewModel : GroupViewModel
    {

        public delegate void ChildAddedHandler(object source, AnimatableUserControl node);
        public event ChildAddedHandler ChildAdded;

        public GroupTagNodeViewModel(GroupModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
        }

        public string Title
        {
            get { return ((NodeModel) Model).Title; }
            set
            {
                ((NodeModel) Model).Title = value;
                RaisePropertyChanged("Title");
            }
        }

        public bool IsTemporary
        {
            get { return ((GroupModel)Model).IsTemporary; }
            set
            {
                ((GroupModel)Model).IsTemporary = value;
            }
        }

        public string NumChildren
        {
            get
            {
                return ((GroupModel) Model).Children.Keys.Count.ToString();
            }
            set { RaisePropertyChanged("NumChildren");}
        }


        public override void Translate(double dx, double dy)
        {
            base.Translate(dx, dy);
            foreach (var sendable in GetChildren())
            {
                var nodeVm = (NodeViewModel)sendable;
                nodeVm.Translate(dx, dy);
            }
        }

        private List<NodeViewModel> GetChildren()
        {
            var groupNodeModel = (GroupModel)Model;

            var children = new List<NodeViewModel>();
            foreach (var atomView in SessionController.Instance.ActiveWorkspace.AtomViewList)
            {
                var atomId = ((AtomViewModel)atomView.DataContext).ID;
                if (groupNodeModel.Children.ContainsKey(atomId))
                    children.Add((NodeViewModel)atomView.DataContext);
            }
            return children;
        }
        
        public override async Task OnChildAdded(object source, Sendable nodeModel)
        {
            // Do nothing.
            Debug.WriteLine("VIEWMODEL = CHILD ADDED");
            var child = (AnimatableUserControl)SessionController.Instance.ActiveWorkspace.AtomViewList.Where( atom => ((AtomViewModel)atom.DataContext).Model == nodeModel).ElementAt(0);
            var x = (NodeTemplate)child.FindName("nodeTpl");
           // x.tags.Visibility = Visibility.Collapsed;
            ChildAdded?.Invoke(this, (AnimatableUserControl)child);
            RaisePropertyChanged("NumChildren");
            //var v = (GroupTagNodeView) View;
           // v.ChildAdded(child);
        }
    }
}