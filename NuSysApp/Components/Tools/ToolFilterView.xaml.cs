using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ToolFilterView : AnimatableUserControl, ToolLinkable
    {

        /// <summary>
        /// The list of filter types available. Display list is bound to this.
        /// </summary>
        ObservableCollection<ToolModel.ToolFilterTypeTitle> Filters;

        /// <summary>
        /// The parents are just held in a list then passed into the new tool when a tool filter type has been chosen. 
        /// </summary>
        private HashSet<ToolLinkable> _parentToolStartables;
        private Point2d _anchor;

        /// <summary>
        /// Anchor point for tool link
        /// </summary>
        public Point2d ToolAnchor { get { return _anchor; } }
        public event EventHandler<Point2d> ToolAnchorChanged;
        public event EventHandler<string> Disposed;
        private const double MinWidth = 100;
        private const double MinHeight = 300;

        public double X {
            get { return (RenderTransform as CompositeTransform).TranslateX; }
        }
        public double Y
        {
            get { return (RenderTransform as CompositeTransform).TranslateY; }
        }

        public ToolFilterView(double x, double y, ToolLinkable parentToolStartable = null)
        {
            this.InitializeComponent();
            SetUp(x, y);
            if (parentToolStartable != null)
            {
                AddParentTool(parentToolStartable);
            }
            CalculateAnchorPoint();
        }

        /// <summary>
        /// Since the tool FILTER view can never create a new link, it cannot be started from. So this returns null
        /// </summary>
        public ToolStartable GetToolStartable()
        {
            return null;
        }

        /// <summary>
        /// Calculates the tool anchor point of as the top center of the node
        /// </summary>
        public void CalculateAnchorPoint()
        {
            _anchor = new Point2d(X + Width / 2 + 60, Y + 20);
        }

        /// <summary>
        /// Sets location of filter, recalculates anchorpoint and invokes anchorpoint changed
        /// </summary>
        public void SetLocation(double x, double y)
        {
            (RenderTransform as CompositeTransform).TranslateX = x;
            (RenderTransform as CompositeTransform).TranslateY = y;
            CalculateAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);
        }

        /// <summary>
        /// Sets size of filter, recalculates anchorpoint and invokes anchorpoint changed
        /// </summary>
        public void SetSize(double width, double height)
        {
            Height = height;
            Width = width;
            xFilterList.Height = height;
            xCanvas.Height = height;
            xGrid.Height = height;
            Grid1.Height = height;
            xFilterList.Width = width;
            xTitle.Width = width;
            xCanvas.Width = width;
            xGrid.Width = width;
            Grid1.Width = width;
            CalculateAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);
        }

        public void Dispose()
        {
            Disposed?.Invoke(this, "ToolFilterView");
        }

        /// <summary>
        /// Adds a parent to the filter's parentToolStartable list
        /// </summary>
        public void AddParentTool(ToolLinkable parentToolStartable)
        {
            if (parentToolStartable != null)
            {
                parentToolStartable.Disposed += ParentToolStartable_Disposed;
                _parentToolStartables.Add(parentToolStartable);
            }
        }

        /// <summary>
        /// Remove parent from parent list and remove any event listeners
        /// </summary>
        private void ParentToolStartable_Disposed(object sender, string e)
        {
            _parentToolStartables.Remove(sender as ToolLinkable);
            (sender as ToolLinkable).Disposed -= ParentToolStartable_Disposed;
        }

        /// <summary>
        /// Removes parent tool from list of parents
        /// </summary>
        public void RemoveParentTool(ToolLinkable parentToolViewModel)
        {
            _parentToolStartables.Remove(parentToolViewModel);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            this.Dispose();
        }


 

        /// <summary>
        /// Instatiate variables, set up size, location, and filter types.
        /// </summary>
        private void SetUp(double x, double y)
        {
            _parentToolStartables = new HashSet<ToolLinkable>();
            this.RenderTransform = new CompositeTransform();
            SetSize(300, 400);
            SetLocation(x, y);
            Filters = new ObservableCollection<ToolModel.ToolFilterTypeTitle>()
            { ToolModel.ToolFilterTypeTitle.Type, ToolModel.ToolFilterTypeTitle.Title,  ToolModel.ToolFilterTypeTitle.Creator,  ToolModel.ToolFilterTypeTitle.Date, ToolModel.ToolFilterTypeTitle.LastEditedDate,  ToolModel.ToolFilterTypeTitle.MetadataKeys, ToolModel.ToolFilterTypeTitle.AllMetadata};
        }

        /// <summary>
        /// For moving the tool filter chooser
        /// </summary>
        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// For moving the tool filter chooser
        /// </summary>
        private void Tool_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var x = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var y = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;

            SetLocation(X + x,Y + y);
            e.Handled = true;
        }

        private void xFilterList_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            //keep this method.
            e.Handled = true;
        }

        private void xFilterList_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void xFilterList_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        /// <summary>
        /// Creates and sets up a new metadata tool. Occurs when All Metadata is selected
        /// </summary>
        private void CreateMetadataTool(FreeFormViewerViewModel wvm)
        {
            MetadataToolModel model = new MetadataToolModel();
            MetadataToolController controller = new MetadataToolController(model);
            MetadataToolViewModel viewmodel = new MetadataToolViewModel(controller);
            viewmodel.Filter = ToolModel.ToolFilterTypeTitle.AllMetadata;
            MetadataToolView view = new MetadataToolView(viewmodel, (RenderTransform as CompositeTransform).TranslateX, (RenderTransform as CompositeTransform).TranslateY);
            SetUpParents(controller, viewmodel, wvm);
            wvm.AtomViewList.Add(view);
        }

        /// <summary>
        /// Creates and sets up a new basic tool. Occurs when anything except All Metadata is selected
        /// </summary>
        private void CreateBasicTool(ToolModel.ToolFilterTypeTitle selection, FreeFormViewerViewModel wvm)
        {
            BasicToolModel model = new BasicToolModel();
            BasicToolController controller = new BasicToolController(model);
            BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
            viewmodel.Filter = selection;
            BaseToolView view = new BaseToolView(viewmodel, (RenderTransform as CompositeTransform).TranslateX, (RenderTransform as CompositeTransform).TranslateY);
            SetUpParents(controller, viewmodel, wvm);
            wvm.AtomViewList.Add(view);
        }

        /// <summary>
        /// Adds the each of the parents in parentToolStartables to the new toolcontroller that was just created
        /// </summary>
        private void SetUpParents(ToolController controller, ToolViewModel viewmodel, FreeFormViewerViewModel wvm)
        {
            if (_parentToolStartables.Count != 0)
            {
                foreach (var tool in _parentToolStartables)
                {
                    controller.AddParent(tool.GetToolStartable());
                    var linkviewmodel = new ToolLinkViewModel(tool, viewmodel);
                    var link = new ToolLinkView(linkviewmodel);
                    Canvas.SetZIndex(link, -1);
                    wvm.AtomViewList.Add(link);
                }
            }
        }

        /// <summary>
        /// When an item on the filter list is selected, create either a new metadata or basic tool and remove the filter chooser.
        /// </summary>
        private void XFilterList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xFilterList.SelectedItems.Count() < 1)
            {
                return;
            }
            ToolModel.ToolFilterTypeTitle selection = (ToolModel.ToolFilterTypeTitle)(xFilterList.SelectedItems[0]);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var x = Canvas.GetZIndex(this) - 1;


            if (selection == ToolModel.ToolFilterTypeTitle.AllMetadata)            
            {
                CreateMetadataTool(wvm);
            }
            else
            {
                CreateBasicTool(selection, wvm);
            }
            Disposed?.Invoke(this, "ToolFilterView");
            wvm.AtomViewList.Remove(this);
        }

        /// <summary>
        /// For resizing the filter chooser
        /// </summary>
        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = this.Width + e.Delta.Translation.X / zoom;
            var resizeY = this.Height + e.Delta.Translation.Y / zoom;

            

            xFilterList.Width = resizeX;
            if (resizeX > MinWidth && resizeY > MinHeight)
            {
                this.SetSize(resizeX, resizeY);

            }
            else if (resizeX > MinWidth)
            {
                this.SetSize(resizeX, this.Height);
            }
            else if (resizeY > MinHeight)
            {
                SetSize(this.Width, resizeY);
            }
            e.Handled = true;
        }


    }
}
