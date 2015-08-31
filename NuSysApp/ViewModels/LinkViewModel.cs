using System;
using System.Linq;
using System.Xml;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    /// <summary>
    /// link view model class
    /// 
    /// parameters: Atom1 and Atom2 are the two atoms that the link connects, 
    /// and workspace is main workspace.
    /// 
    /// </summary>
    public class LinkViewModel : AtomViewModel
    {
        #region Private Members

        private AtomViewModel _atom1, _atom2;
        #endregion Private members

        public LinkViewModel(Link model, AtomViewModel atom1,
            AtomViewModel atom2, WorkspaceViewModel workspace) : base(model, workspace)
        {
            this.Atom1 = atom1;
            this.Atom2 = atom2;
            this.AtomType = Constants.Link;
            this.Atom1.UpdateAnchor();
            this.Atom2.UpdateAnchor();
            this.IsVisible = true;

            var line = this.LineRepresentation;

            this.AnchorX = (int)(line.X2 + (Math.Abs(line.X2 - line.X1) / 2));
            this.AnchorY = (int)(line.Y1 + (Math.Abs(line.Y2 - line.Y1) / 2));
            this.Anchor = new Point(this.AnchorX, this.AnchorY);
            this.View = new BezierLinkView(this);  
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(150,189,204,212));
            ((Link)this.Model).OnDeletion += DeletionHappend;
        }
        private void DeletionHappend(object source, DeleteEventArgs e)
        {
            this.WorkSpaceViewModel.DeleteLink(this);
        }

        #region Link Manipulation Methods
        public override void Remove()
        {
            NetworkConnector.Instance.RequestDeleteSendable(ID);
            if (this.IsSelected)
            {
                WorkSpaceViewModel.ClearSelection();
            }
            //this.Atom1.LinkList.Remove(this);
            //this.Atom2.LinkList.Remove(this);
            //var toDelete = this.LinkList.ToList();
            //foreach (var link in toDelete)
            //{
            //    link.Remove();
            //    WorkSpaceViewModel.AtomViewList.Remove(link.View);
            //}
            //this.WorkSpaceViewModel.LinkViewModelList.Remove(this);
            //this.Annotation?.Remove();
        }

        #endregion Link Manipulation Methods

        #region Public Properties
        public NodeViewModel Annotation { get; set; }
        public AtomViewModel Atom1
        {
            get { return _atom1; }
            set
            {
                if (_atom1 == value)
                {
                    return;
                }
                _atom1 = value;
                RaisePropertyChanged("Atom1");
            }
        }

        public AtomViewModel Atom2
        {
            get { return _atom2;}
            set
            {
                if (_atom2 == value)
                {
                    return;
                }
                _atom2 = value;
                RaisePropertyChanged("Atom2");
            }
        }

        public Line LineRepresentation
            => new Line() {X1 = Atom1.AnchorX, X2 = Atom2.AnchorX, Y1 = Atom1.AnchorY, Y2 = Atom2.AnchorY};

        #endregion Public Properties

        public override void UpdateAnchor()
        {
            var line = this.LineRepresentation;
            var dx = (line.X2 - line.X1)/2;
            var dy = (line.Y2 - line.Y1)/2;
            this.AnchorX = (int)(line.X1 + dx);
            this.AnchorY = (int)(line.Y1 + dy);
            this.Anchor = new Point(this.AnchorX, this.AnchorY);

            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

        public bool IsVisible 
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value)
                {
                    return;
                }
                _isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public XmlElement WriteXML(XmlDocument doc)
        {
            return ((Link)Model).WriteXML(doc);
        }
    }
}