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
        private LinkLibraryElementController _linkLibraryElementController;

        public LinkModel LinkModel { get; }
        private SolidColorBrush _defaultColor;
        public LinkViewModel(LinkElementController controller) : base(controller)
        {
            LinkModel = (LinkModel)controller.Model;

            InElementController = SessionController.Instance.IdToControllers[LinkModel.InAtomId]; 
            OutElementController = SessionController.Instance.IdToControllers[LinkModel.OutAtomId];

            Anchor = new Point2d((int) (OutElementController.Model.X + (Math.Abs(OutElementController.Model.X - InElementController.Model.X)/2)),
                (int) (InElementController.Model.Y + (Math.Abs(OutElementController.Model.Y - InElementController.Model.Y)/2)));

            Color = new SolidColorBrush(Constants.color2);

            _linkLibraryElementController =
                SessionController.Instance.LinkController.GetLinkLibraryElementController(LinkModel.LibraryId);
            _linkLibraryElementController.TitleChanged += LinkLibraryElementController_TitleChanged;

            Annotation = _linkLibraryElementController.Title;

            /*
            if (LinkModel.InFineGrain != null)
            {
                LinkModel.InFineGrain.OnTimeChange += InFineGrain_OnTimeChange;
            }
            */

            InElementController.PositionChanged += InElementControllerOnPositionChanged;
            OutElementController.PositionChanged += InElementControllerOnPositionChanged;
            InElementController.SizeChanged += OutElementControllerOnSizeChanged;
            OutElementController.SizeChanged += OutElementControllerOnSizeChanged;
            controller.ColorChanged += Controller_ColorChanged;
            
        }

        private void LinkLibraryElementController_TitleChanged(object sender, string title)
        {
            Title = title;
        }

        private void Controller_ColorChanged(SolidColorBrush color)
        {
            Color = color;
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
            ((LinkElementController)Controller).ColorChanged -= Controller_ColorChanged;
            _linkLibraryElementController.TitleChanged -= LinkLibraryElementController_TitleChanged;

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
                if (value)
                {
                    _defaultColor = Color;
                }
                Color = value
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0xAA, 0x2D))
                    : _defaultColor;
                RaisePropertyChanged("Color");
                RaisePropertyChanged("IsSelected");
            }
        }

        private string _annotation;
        public string Annotation
        {
            get { return _annotation; }
            set
            {
                _annotation = value;
                RaisePropertyChanged("Annotation");
            }
        }

        public override PointCollection ReferencePoints
        {
             get
             {
                 PointCollection pts = new PointCollection();
                 pts.Add(new Point2d(Anchor.X, Anchor.Y));
                 pts.Add(new Point2d(OutElementController.Model.X, OutElementController.Model.Y));
                 pts.Add(new Point2d(InElementController.Model.X, InElementController.Model.Y));
                 return pts;
 
             }
         }

        public void UpdateTitle(string title)
        {
            _linkLibraryElementController.TitleChanged -= LinkLibraryElementController_TitleChanged;
            _linkLibraryElementController.SetTitle(title);
            _linkLibraryElementController.TitleChanged += LinkLibraryElementController_TitleChanged;

        }
    }
}