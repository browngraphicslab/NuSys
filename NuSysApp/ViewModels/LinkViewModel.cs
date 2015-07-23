using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;


namespace NuSysApp
{
    /// <summary>
    /// link view model class
    /// 
    /// parameters: node1 and node2 are the two nodes that the link connects, 
    /// and workspace is main workspace.
    /// 
    /// </summary>
    public class LinkViewModel : BaseINPC
    {
        #region Private Members

        private UserControl _view;
        private NodeViewModel _node1, _node2;

        #endregion Private members

        public LinkViewModel(NodeViewModel node1,
            NodeViewModel node2, WorkspaceViewModel workspace)
        {
            this.Node1 = node1;
            this.Node2 = node2;
            this.Node1.UpdateAnchor();
            this.Node2.UpdateAnchor();

            switch (workspace.CurrentLinkMode)
            {
                case WorkspaceViewModel.LinkMode.BEZIERLINK:
                    this.View = new BezierLinkView(this);
                    break;
                default:
                    this.View = new LineLinkView(this);
                    break;
            }
        }

        public void DeleteLink()
        {
            this.Node1.LinkList.Remove(this);
            this.Node2.LinkList.Remove(this);
        }

        #region Public Properties

        public UserControl View
        {
            get { return _view; }
            set
            {
                if (_view == value)
                {
                    return;
                }

                _view = value;

                RaisePropertyChanged("View");
            }
        }

        public NodeViewModel Node1
        {
            get { return _node1; }
            set
            {
                if (_node1 == value)
                {
                    return;
                }
                _node1 = value;
                RaisePropertyChanged("Node1");
            }
        }

        public NodeViewModel Node2
        {
            get { return _node2; }
            set
            {
                if (_node2 == value)
                {
                    return;
                }
                _node2 = value;
                RaisePropertyChanged("Node2");
            }
        }

        public Line LineRepresentation
            => new Line() {X1 = Node1.AnchorX, X2 = Node2.AnchorX, Y1 = Node1.AnchorY, Y2 = Node2.AnchorY};

        #endregion Public Properties
    }
}