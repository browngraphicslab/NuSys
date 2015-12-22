using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class LabelNodeViewModel : NodeContainerViewModel
    {

        public delegate void ChildAddedHandler(object source, AnimatableNodeView node);
        public event ChildAddedHandler ChildAdded;

        public LabelNodeViewModel(GroupModel model) : base(model)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            EnableChildMove = true;
        }

        public bool IsTemporary
        {
            get { return ((GroupModel)Model).IsTemporary; }
            set
            {
                ((GroupModel)Model).IsTemporary = value;
            }
        }

        private List<NodeViewModel> GetChildren()
        {
            var groupNodeModel = (GroupModel)Model;

            var children = new List<NodeViewModel>();
            foreach (var atomView in SessionController.Instance.ActiveWorkspace.AtomViewList)
            {
                var atomId = ((AtomViewModel)atomView.DataContext).Id;
                if (groupNodeModel.Children.ContainsKey(atomId))
                    children.Add((NodeViewModel)atomView.DataContext);
            }
            return children;
        }
        
        protected override async Task OnChildAdded(object source, Sendable nodeModel)
        {
            // Do nothing.
            Debug.WriteLine("VIEWMODEL = CHILD ADDED");
            var child = (AnimatableNodeView)SessionController.Instance.ActiveWorkspace.AtomViewList.Where( atom => ((AtomViewModel)atom.DataContext).Model == nodeModel).ElementAt(0);
            var x = (NodeTemplate)child.FindName("nodeTpl");
           // x.tags.Visibility = Visibility.Collapsed;
            ChildAdded?.Invoke(this, (AnimatableNodeView)child);
            RaisePropertyChanged("NumChildren");
            //var v = (LabelNodeViewModel) View;
           // v.ChildAdded(child);
        }
    }
}