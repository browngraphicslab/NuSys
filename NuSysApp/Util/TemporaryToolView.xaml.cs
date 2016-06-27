using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class TemporaryToolView : AnimatableUserControl
    {
        public ObservableCollection<ToolModel.FilterTitle> Filters { get; set; }
        //public ObservableCollection<string> PropertiesToDisplay { get; set; }

        private Image _dragItem;
        private enum DragMode { Filter };
        private DragMode _currenDragMode = DragMode.Filter;
        public TemporaryToolView(ToolViewModel vm, double x, double y)
        {
            _dragItem = new Image();
            Filters = new ObservableCollection<ToolModel.FilterTitle>()
            { ToolModel.FilterTitle.Type, ToolModel.FilterTitle.Title,  ToolModel.FilterTitle.Creator,  ToolModel.FilterTitle.Date,  ToolModel.FilterTitle.MetadataKeys,  ToolModel.FilterTitle.MetadataValues };
            this.InitializeComponent();
            vm.Controller.SetSize(300, 300);
            vm.Controller.SetPosition(x,y);
            this.DataContext = vm;
            xFilterElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xFilterElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
        }

        private void Vm_PropertiesToDisplayChanged(string selection)
        {
            if ((DataContext as ToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedIndex = 1;
                //xPropertiesList.
                //xPropertiesList.SelectedItem = (DataContext as ToolViewModel).Selection;
                //xPropertiesList.SelectedIndex = 1;
            }
            
        }

        public void Dispose()
        {
            xFilterElement.PointerPressed -= BtnAddOnManipulationStarting;
            xFilterElement.PointerReleased -= BtnAddOnManipulationCompleted;
            xChooseFilter.Click -= ButtonBase_OnClick;
            xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
            xUniqueButton.Checked -= XUniqueButton_OnChecked;
            (DataContext as ToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
        }



        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            if (_currenDragMode == DragMode.Filter)
            {

                var vm = (ToolViewModel)DataContext;

                ToolModel model = new ToolModel();
                ToolController controller = new ToolController(model);
                ToolViewModel viewmodel = new ToolViewModel(controller);
                vm.AddChildFilter(controller);
                TemporaryToolView view = new TemporaryToolView(viewmodel, r.X, r.Y);


                var linkviewmodel = new ToolLinkViewModel(this, view);
                var link = new ToolLinkView(linkviewmodel);

                wvm.AtomViewList.Add(link);
                wvm.AtomViewList.Add(view);



            }

            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            CapturePointer(args.Pointer);

            if (sender == xFilterElement)
            {
                _currenDragMode = DragMode.Filter;
            }




            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);

        }

        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {

            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth / 2;
            t.TranslateY = p.Y - _dragItem.ActualHeight / 2;

        }
        

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (xFilterList.SelectedItems.Count() < 1)
            {
                return;
            }
            ToolModel.FilterTitle selection = (ToolModel.FilterTitle)(xFilterList.SelectedItems[0]);
            var toolViewModel = DataContext as ToolViewModel;
            if (toolViewModel != null)
            {
                toolViewModel.Filter = selection;
            }
            toolViewModel.reloadPropertiesToDisplay();
            //do i need this
            //PropertiesToDisplay = (DataContext as ToolViewModel).PropertiesToDisplay;
            xPropertiesList.ItemsSource = (DataContext as ToolViewModel).PropertiesToDisplay;
            
            bottompanel.Children.Remove(xChooseFilter);
            xGrid.Children.Remove(xFilterList);
            xTitle.Text = selection.ToString();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);

        }

        private void XPropertiesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (xPropertiesList.SelectedItems.Count == 1)
            {
                (DataContext as ToolViewModel).Selection  = ((string)(xPropertiesList.SelectedItems[0]));
            }
        }

        private void XUniqueButton_OnChecked(object sender, RoutedEventArgs e)
        {
            //PropertiesToDisplay = (DataContext as ToolViewModel).PropertiesToDisplay;
        }
    }
}
