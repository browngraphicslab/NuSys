using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class LinkViewModel : ElementViewModel
    {
        public LinkViewModel(ElementController controller, ElementViewModel atom1, ElementViewModel atom2) : base(controller)
        {
            Atom1 = atom1;
            Atom2 = atom2;
            var line = LineRepresentation;
            Anchor = new Point2d((int) (line.X2 + (Math.Abs(line.X2 - line.X1)/2)),
                (int) (line.Y1 + (Math.Abs(line.Y2 - line.Y1)/2)));
            Atom1.AddLink(this);
            Atom2.AddLink(this);

            AnnotationText = controller.Model.Title;

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

  

        #endregion Link Manipulation Methods

  
      

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

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                base.IsSelected = value;
                Color = value
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0xAA, 0x2D))
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
                RaisePropertyChanged("Color");
                RaisePropertyChanged("IsSelected");
            }
        }

        public override PointCollection ReferencePoints
        {
             get
             {
                 PointCollection pts = new PointCollection();
                 pts.Add(new Point2d(Anchor.X, Anchor.Y));
                 return pts;
 
             }
         }

#region Private Members

private ElementViewModel _atom1, _atom2;
        private string _annotationText;

        #endregion Private members

        #region Public Properties

        public ElementViewModel Atom1
        {
            get { return _atom1; }
            set
            {
                _atom1 = value;
                RaisePropertyChanged("Atom1");
            }
        }

        public ElementViewModel Atom2
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