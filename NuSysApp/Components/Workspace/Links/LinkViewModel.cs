using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class LinkViewModel : AtomViewModel
    {
        public LinkViewModel(LinkModel model, AtomViewModel atom1, AtomViewModel atom2) : base(model)
        {
            Atom1 = atom1;
            Atom2 = atom2;
            var line = LineRepresentation;
            Anchor = new Point2d((int) (line.X2 + (Math.Abs(line.X2 - line.X1)/2)),
                (int) (line.Y1 + (Math.Abs(line.Y2 - line.Y1)/2)));
            Atom1.AddLink(this);
            Atom2.AddLink(this);

            AnnotationText = model.Title;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
        }


        public string AnnotationText
        {
            get { return Model.Title; }
            set { Model.Title = value; }
        }

        public override void Dispose()
        {
            var model = (LinkModel) Model;
            base.Dispose();
        }

        #region Link Manipulation Methods

        public override void Remove()
        {
            //NetworkConnector.Instance.RequestDeleteSendable(Id);
            if (IsSelected)
            {
                //TODO: re-add
                SessionController.Instance.ActiveWorkspace.ClearSelection();
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

        public override void SetPosition(double x, double y)
        {
            // Position is set through connecting atoms.
        }

        protected override void OnPositionChanged(object source, PositionChangeEventArgs e)
        {
            // Position is set through connecting atoms.
        }

        public override void Translate(double dx, double dy)
        {
            Atom1.Translate(dx, dy);
            Atom2.Translate(dx, dy);
        }


        public override void UpdateAnchor()
        {
            var line = this.LineRepresentation;
            var dx = (line.X2 - line.X1)/2;
            var dy = (line.Y2 - line.Y1)/2;
            Anchor.X = (int) (line.X1 + dx);
            Anchor.Y = (int) (line.Y1 + dy);
            Anchor = new Point2d(this.Anchor.X, this.Anchor.Y);

            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
        }

        public override void SetSelected(bool val)
        {
            _isSelected = val;
            Color = val
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0xAA, 0x2D))
                : new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
            RaisePropertyChanged("Color");
            RaisePropertyChanged("IsSelected");
        }

        // returns the anchor point to be used as the reference point in multi select
        public override PointCollection ReferencePoints
        {
            get
            {
                PointCollection pts = new PointCollection();
                pts.Add(new Point2d(Anchor.X,Anchor.Y));
                return pts;
            }
        }


        #region Private Members

        private AtomViewModel _atom1, _atom2;
        private string _annotationText;

        #endregion Private members

        #region Public Properties

        public AtomViewModel Atom1
        {
            get { return _atom1; }
            set
            {
                _atom1 = value;
                RaisePropertyChanged("Atom1");
            }
        }

        public AtomViewModel Atom2
        {
            get { return _atom2; }
            set
            {
                _atom2 = value;
                RaisePropertyChanged("Atom2");
            }
        }

        public Line LineRepresentation
            => new Line() {X1 = Atom1.Anchor.X, X2 = Atom2.Anchor.X, Y1 = Atom1.Anchor.Y, Y2 = Atom2.Anchor.Y};

        #endregion Public Properties
    }
}