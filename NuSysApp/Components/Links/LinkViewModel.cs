using System;
using System.Diagnostics;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class LinkViewModel : ElementViewModel
    {

        private ElementController InElementController;
        private ElementController OutElementController;

        public LinkViewModel(LinkElementController controller) : base(controller)
        {
            var linkModel = (LinkModel)controller.Model;
            InElementController = SessionController.Instance.IdToControllers[linkModel.InAtomId]; 
            OutElementController = SessionController.Instance.IdToControllers[linkModel.OutAtomId];

            Anchor = new Point2d((int) (OutElementController.Model.X + (Math.Abs(OutElementController.Model.X - InElementController.Model.X)/2)),
                (int) (InElementController.Model.Y + (Math.Abs(OutElementController.Model.Y - InElementController.Model.Y)/2)));


            AnnotationText = controller.Model.Title;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(150, 189, 204, 212));
   

            InElementController.PositionChanged += InElementControllerOnPositionChanged;
            OutElementController.PositionChanged += InElementControllerOnPositionChanged;
            InElementController.SizeChanged += OutElementControllerOnSizeChanged;
            OutElementController.SizeChanged += OutElementControllerOnSizeChanged;

        }

        private void OutElementControllerOnSizeChanged(object source, double width, double height)
        {
            UpdateAnchor();
        }

        private void InElementControllerOnSizeChangedInElementControllerOnSizeChanged(object source, double width, double height)
        {
            UpdateAnchor();
        }

        private void OutElementControllerOnPositionChanged(object source, double d, double d1, double x, double y)
        {
            UpdateAnchor();
        }

        private void InElementControllerOnPositionChanged(object source, double d, double d1, double x, double y)
        {
            UpdateAnchor();
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
            Anchor = new Point2d((int)(OutElementController.Model.X + (Math.Abs(OutElementController.Model.X - InElementController.Model.X) / 2)),
                (int)(InElementController.Model.Y + (Math.Abs(OutElementController.Model.Y - InElementController.Model.Y) / 2)));

            foreach (var link in LinkList)
            {
                link.UpdateAnchor();
            }
            RaisePropertyChanged("Anchor");
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


        private string _annotationText;

        #endregion Private members


    }
}