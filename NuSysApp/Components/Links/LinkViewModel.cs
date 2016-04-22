using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class LinkViewModel : ElementViewModel
    {

        private ElementController InElementController;
        private ElementController OutElementController;

        public LinkModel LinkModel { get; }
        public LinkViewModel(LinkElementController controller) : base(controller)
        {
            LinkModel = (LinkModel)controller.Model;

            controller.AnnotationChanged += ControllerOnAnnotationChanged;
            Annotation = LinkModel.Annotation;

            InElementController = SessionController.Instance.IdToControllers[LinkModel.InAtomId]; 
            OutElementController = SessionController.Instance.IdToControllers[LinkModel.OutAtomId];

            Anchor = new Point2d((int) (OutElementController.Model.X + (Math.Abs(OutElementController.Model.X - InElementController.Model.X)/2)),
                (int) (InElementController.Model.Y + (Math.Abs(OutElementController.Model.Y - InElementController.Model.Y)/2)));

            Color = new SolidColorBrush(Constants.color2);
            if (LinkModel.InFineGrain != null)
            {
                LinkModel.InFineGrain.OnTimeChange += InFineGrain_OnTimeChange;
            }

            InElementController.PositionChanged += InElementControllerOnPositionChanged;
            OutElementController.PositionChanged += InElementControllerOnPositionChanged;
            InElementController.SizeChanged += OutElementControllerOnSizeChanged;
            OutElementController.SizeChanged += OutElementControllerOnSizeChanged;

            
        }

        private void InFineGrain_OnTimeChange()
        {
            ((LinkElementController)Controller).SaveTimeBlock();
        }

        public override Task Init()
        {
            UpdateAnchor();
            return base.Init();
        }

        private void ControllerOnAnnotationChanged(string text)
        {
            Annotation = text;
            RaisePropertyChanged("Annotation");
        }

        private void OutElementControllerOnSizeChanged(object source, double width, double height)
        {
            UpdateAnchor();
        }

        private void InElementControllerOnPositionChanged(object source, double d, double d1, double x, double y)
        {
            UpdateAnchor();
        }


        public override void Dispose()
        {
            InElementController.PositionChanged -= InElementControllerOnPositionChanged;
            OutElementController.PositionChanged -= InElementControllerOnPositionChanged;
            InElementController.SizeChanged -= OutElementControllerOnSizeChanged;
            OutElementController.SizeChanged -= OutElementControllerOnSizeChanged;

            base.Dispose();
        }
      

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
                    : new SolidColorBrush(Constants.color2);
                RaisePropertyChanged("Color");
                RaisePropertyChanged("IsSelected");
            }
        }

        public string Annotation
        {
            get;set;
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
    }
}