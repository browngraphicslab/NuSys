using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;


namespace NuSysApp
{
    /// <summary>
    /// link view model class
    /// 
    /// parameters: ints x1, x2, y1, and y2 are coordinates of two nodes a link is connecting, node1 and node2 are those two nodes, 
    /// and workspace is main workspace.
    /// 
    /// </summary>
    public class LinkViewModel : BaseINPC
    {
        #region Private Members

        private int _x1, _x2, _y1, _y2;
        private UserControl _view;
        private NodeViewModel _node1, _node2;

        #endregion Private members

        public LinkViewModel(int x1, int x2, int y1, int y2, NodeViewModel node1,
            NodeViewModel node2, WorkspaceViewModel workspace)
        {
            this.X1 = x1;
            this.X2 = x2;
            this.Y1 = y1;
            this.Y2 = y2;
            this.Node1 = node1;
            this.Node2 = node2;
            this.Node1.UpdateAnchor();
            this.Node2.UpdateAnchor();

            if (workspace.CurrentLinkMode == WorkspaceViewModel.LinkMode.BEZIERLINK)
            {
                this.View = new BezierLink(this);
            }
            else
            {
                this.View = new LinkView(this);
            }
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

        public int X1
        {
            get { return _x1; }
            set
            {
                if (_x1 == value)
                {
                    return;
                }
                _x1 = value;
                RaisePropertyChanged("X1");
            }
        }

        public int Y1
        {
            get { return _y1; }
            set
            {
                if (_y1 == value)
                {
                    return;
                }
                _y1 = value;
                RaisePropertyChanged("Y1");
            }
        }

        public int X2
        {
            get { return _x2; }
            set
            {
                if (_x2 == value)
                {
                    return;
                }
                _x2 = value;
                RaisePropertyChanged("X2");
            }
        }

        public int Y2
        {
            get { return _y2; }
            set
            {
                if (_y2 == value)
                {
                    return;
                }
                _y2 = value;
                RaisePropertyChanged("Y2");
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

        public Line Line => new Line() {X1 = Node1.AnchorX , X2 = Node2.AnchorX, Y1 = Node1.AnchorY, Y2 = Node2.AnchorY};

        #endregion Public Properties
    }
}