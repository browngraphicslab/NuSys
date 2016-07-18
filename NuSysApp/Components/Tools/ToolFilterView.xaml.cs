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
    public sealed partial class ToolFilterView : AnimatableUserControl
    {


        ObservableCollection<ToolModel.ToolFilterTypeTitle> Filters;
        private HashSet<ToolStartable> _parentToolStartables;

        private HashSet<ToolViewModel> _parentToolViewModels;

        public delegate void LocationChangedEventHandler(object sender, double x, double y);
        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public delegate void DisposedEventHandler(object sender, string id);


        public event LocationChangedEventHandler LocationChanged;
        public event SizeChangedEventHandler SizeChanged;
        public event DisposedEventHandler Disposed;


        private const double MinWidth = 100;
        private const double MinHeight = 300;

        private List<ToolFilterLinkView> _links; 

        public double X {
            get { return (RenderTransform as CompositeTransform).TranslateX; }
        }
        public double Y
        {
            get { return (RenderTransform as CompositeTransform).TranslateY; }
        }

        public void SetLocation(double x, double y)
        {
            (RenderTransform as CompositeTransform).TranslateX = x;
            (RenderTransform as CompositeTransform).TranslateY = y;
            LocationChanged?.Invoke(this, x, y);
        }

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
            SizeChanged?.Invoke(this, height, width);
        }

        public void Dispose()
        {
            Disposed?.Invoke(this, "ToolFilterView");
        }

        public void AddParentTool(ToolViewModel parentToolViewModel)
        {
            if (parentToolViewModel != null)
            {
                _parentToolViewModels.Add(parentToolViewModel);
            }
        }
        public void AddParentTool(ToolStartable parentToolStartable)
        {
            if (parentToolStartable != null)
            {
                _parentToolStartables.Add(parentToolStartable);
            }
        }

        public void RemoveParentTool(ToolViewModel parentToolViewModel)
        {
            _parentToolViewModels.Remove(parentToolViewModel);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            this.Dispose();
        }
        public ToolFilterView(double x, double y)
        {
            this.InitializeComponent();
            SetUp(x, y);
        }

        public ToolFilterView(double x, double y, ToolViewModel parentToolViewModel = null)
        {
            this.InitializeComponent();
            SetUp(x, y);
            AddParentTool(parentToolViewModel);

        }


        public ToolFilterView(double x, double y, ToolStartable parentToolStartable = null)
        {
            this.InitializeComponent();
            SetUp(x, y);
            AddParentTool(parentToolStartable);

        }

        private void SetUp(double x, double y)
        {
            _parentToolViewModels = new HashSet<ToolViewModel>();
            _parentToolStartables = new HashSet<ToolStartable>();
            this.RenderTransform = new CompositeTransform();
            SetSize(300, 400);
            _links = new List<ToolFilterLinkView>();
            SetLocation(x, y);
            Filters = new ObservableCollection<ToolModel.ToolFilterTypeTitle>()
            { ToolModel.ToolFilterTypeTitle.Type, ToolModel.ToolFilterTypeTitle.Title,  ToolModel.ToolFilterTypeTitle.Creator,  ToolModel.ToolFilterTypeTitle.Date, ToolModel.ToolFilterTypeTitle.LastEditedDate,  ToolModel.ToolFilterTypeTitle.MetadataKeys, ToolModel.ToolFilterTypeTitle.AllMetadata};
        }

        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

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

        private void CreateMetadataToolView(FreeFormViewerViewModel wvm)
        {
            MetadataToolModel model = new MetadataToolModel();
            MetadataToolController controller = new MetadataToolController(model);
            MetadataToolViewModel viewmodel = new MetadataToolViewModel(controller);
            viewmodel.Filter = ToolModel.ToolFilterTypeTitle.AllMetadata;
            MetadataToolView view = new MetadataToolView(viewmodel, (RenderTransform as CompositeTransform).TranslateX, (RenderTransform as CompositeTransform).TranslateY);
            SetUpParents(controller, viewmodel, wvm);


            wvm.AtomViewList.Add(view);
        }

        private void SetUpParents(ToolController controller, ToolViewModel viewmodel, FreeFormViewerViewModel wvm)
        {
            if (_parentToolViewModels.Count != 0)
            {
                foreach (var tool in _parentToolViewModels)
                {
                    controller.AddParent(tool.Controller);
                    var linkviewmodel = new ToolLinkViewModel(tool, viewmodel);
                    var link = new ToolLinkView(linkviewmodel);
                    Canvas.SetZIndex(link, -1);
                    wvm.AtomViewList.Add(link);
                }
            }
            if (_parentToolStartables.Count != 0)
            {
                foreach (var toolStartable in _parentToolStartables)
                {
                    controller.AddParent(toolStartable);
                }
            }
        }

        private void CreateBasicToolView(ToolModel.ToolFilterTypeTitle selection, FreeFormViewerViewModel wvm)
        {
            BasicToolModel model = new BasicToolModel();
            BasicToolController controller = new BasicToolController(model);
            BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
            viewmodel.Filter = selection;
            BaseToolView view = new BaseToolView(viewmodel, (RenderTransform as CompositeTransform).TranslateX, (RenderTransform as CompositeTransform).TranslateY);
            SetUpParents(controller, viewmodel, wvm);
            wvm.AtomViewList.Add(view);
        }

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
                CreateMetadataToolView(wvm);
            }
            else
            {
                CreateBasicToolView(selection, wvm);
            }
            foreach (var link in _links)
            {
                wvm.AtomViewList.Remove(link);
            }
            wvm.AtomViewList.Remove(this);
        }

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

        public void AddLink(ToolFilterLinkView toolFilterLink)
        {
            _links.Add(toolFilterLink);

        }
    }
}
