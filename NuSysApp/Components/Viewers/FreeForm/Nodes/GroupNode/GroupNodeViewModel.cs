using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;
using NuSysApp.Tools;

namespace NuSysApp
{
    public class GroupNodeViewModel : ElementCollectionViewModel, ToolStartable, ToolLinkable
    {

        private Point2d _anchor;
        public event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;
        
        private ElementCollectionController _controller;

        /// <summary>
        ///The anchor that the tool link uses
        /// </summary>
        public Point2d ToolAnchor { get { return _anchor; } }
        public event EventHandler<Point2d> ToolAnchorChanged;
        //never fired because the collection can never change to or from metadata tool view or basic tool view.
        public event EventHandler<ToolLinkable> ReplacedToolLinkAnchorPoint;


        public CollectionElementModel.CollectionViewType ActiveCollectionViewType { get; set; }
       
        public GroupNodeViewModel(ElementCollectionController controller) : base(controller)
        {
            _nodeViewFactory = new GroupItemThumbFactory();
            CalculateToolAnchorPoint();
            ActiveCollectionViewType = (controller.Model as CollectionElementModel).ActiveCollectionViewType;
            Controller.SizeChanged += Controller_SizeChanged;
            Controller.PositionChanged += Controller_PositionChanged;
            Controller.Disposed += Controller_Disposed;
            _controller = controller;
        }

        public bool Finite
        {
            get
            {
                return (SessionController.Instance.ContentController.GetLibraryElementModel(Controller.Model.LibraryId) as CollectionLibraryElementModel).IsFinite;
            }
        }

        private void Controller_Disposed(object sender, EventArgs e)
        {
            Controller.SizeChanged -= Controller_SizeChanged;
            Controller.PositionChanged -= Controller_PositionChanged;
            Controller.Disposed -= Controller_Disposed;
        }

        /// <summary>
        /// Update tool anchor point when position changes
        /// </summary>
        private void Controller_PositionChanged(object source, double x, double y, double dx = 0, double dy = 0)
        {
            CalculateToolAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);
        }

        /// <summary>
        /// Update tool anchor point when size changes
        /// </summary>
        private void Controller_SizeChanged(object source, double width, double height)
        {
            CalculateToolAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);
        }

        /// <summary>
        /// Calculates the tool anchor point of as the top center of the node
        /// </summary>
        public void CalculateToolAnchorPoint()
        {
            _anchor = new Point2d(Anchor.X, Anchor.Y - Height/2 + 30);
        }



        public ToolStartable GetToolStartable()
        {
            return this;
        }

        /// <summary>
        /// Will either add this tool as a parent if dropped on top of an existing tool, or create a brand new tool filter chooser view. 
        /// </summary>
        public void FilterIconDropped(IEnumerable<UIElement> hitsStart, FreeFormViewerViewModel wvm, double x, double y)
        {
            if (hitsStart.Where(uiElem => (uiElem is FrameworkElement) && (uiElem as FrameworkElement).DataContext is ToolViewModel).ToList().Any())
            {
                var hitsStartList = hitsStart.Where(uiElem => (uiElem is AnimatableUserControl) && (uiElem as AnimatableUserControl).DataContext is ToolViewModel).ToList();
                AddFilterToExistingTool(hitsStartList, wvm);
            }
            else if (hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList().Any())
            {
                var hitsStartList = hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList();
                AddFilterToFilterToolView(hitsStartList, wvm);
            }
            else
            {
                AddNewFilterTool(x, y, wvm);
            }
        }

        /// <summary>
        ///creates new filter tool at specified location
        /// </summary>
        public void AddNewFilterTool(double x, double y, FreeFormViewerViewModel wvm)
        {


            var toolFilter = new ToolFilterView(x, y, this);

            var linkviewmodel = new ToolLinkViewModel(this, toolFilter);
            var link = new ToolLinkView(linkviewmodel);

            Canvas.SetZIndex(link, Canvas.GetZIndex(toolFilter) - 1);
            wvm.AtomViewList.Add(toolFilter);
            wvm.AtomViewList.Add(link);
        }

        /// <summary>
        ///Adds tool as parent to existing filter picker tool. 
        /// </summary>
        public void AddFilterToFilterToolView(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        {

            var linkviewmodel = new ToolLinkViewModel(this, (hitsStartList.First() as ToolFilterView));
            var linkView = new ToolLinkView(linkviewmodel);
            Canvas.SetZIndex(linkView, Canvas.GetZIndex(hitsStartList.First()) - 1);
            (hitsStartList.First() as ToolFilterView).AddParentTool(this);
            wvm.AtomViewList.Add(linkView);
        }

        /// <summary>
        ///Adds tool as parent to existing tool. 
        /// </summary>
        public void AddFilterToExistingTool(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        {
            ToolViewModel toolViewModel = (hitsStartList.First() as AnimatableUserControl).DataContext as ToolViewModel;
            if (toolViewModel != null)
            {
                var linkviewmodel = new ToolLinkViewModel(this, toolViewModel);
                var link = new ToolLinkView(linkviewmodel);
                Canvas.SetZIndex(link, Canvas.GetZIndex(hitsStartList.First()) - 1);
                wvm.AtomViewList.Add(link);
                toolViewModel.Controller.AddParent(this);
                
            }
        }
    }
}
