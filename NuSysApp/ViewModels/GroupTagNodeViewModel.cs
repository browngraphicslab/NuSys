using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class GroupTagNodeViewModel : GroupViewModel
    {
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

        private async Task UpdateGroup()
        {
            var searchResults = SessionController.Instance.IdToSendables.Values.Where(m =>
            {
                var mm = m as AtomModel;
                if (mm == null)
                    return false;
                return mm.GetMetaData("tags").ToLower().Contains(((GroupModel) Model).Title.ToLower());
            });

            if (searchResults == null)
                return;

            foreach (var searchResult in searchResults.ToList())
            {
                var view = await _nodeViewFactory.CreateFromSendable(searchResult, AtomViewList.ToList());
                AtomViewList.Add(view);
                view.IsHitTestVisible = false;
            }

            RaisePropertyChanged("DONE_LOADING");
        }

        public override async Task Init(UserControl v)
        {
            await base.Init(v);
            UpdateGroup();
        }

        public override async void OnChildAdded(object source, Sendable nodeModel)
        {
            // Do nothing.
            RaisePropertyChanged("NumChildren");
        }
    }
}