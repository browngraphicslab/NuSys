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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ToolView : UserControl
    {
        public ObservableCollection<ToolModel.FilterTitle> Filters { get; set; }
        public ObservableCollection<string> MetaDataToDisplay { get; set; } 
        private Image _dragItem;
        //public Canvas xCanvas;
        private enum DragMode { Filter };
        private DragMode _currenDragMode = DragMode.Filter;
        public ToolView(ToolViewModel vm, double x, double y)
        {
           _dragItem = new Image();
            Filters  = new ObservableCollection<ToolModel.FilterTitle>()
            { ToolModel.FilterTitle.Type, ToolModel.FilterTitle.Title,  ToolModel.FilterTitle.Creator,  ToolModel.FilterTitle.Date,  ToolModel.FilterTitle.MetadataKeys,  ToolModel.FilterTitle.MetadataValues };
            this.InitializeComponent();
            this.DataContext = vm;
            Canvas.SetTop(this, y);
            Canvas.SetLeft(this, x);
            xFilterElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xFilterElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
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
                ToolView view = new ToolView(viewmodel, r.X, r.Y);
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

        private void XList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //let controller fire an event
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            MetaDataToDisplay = new ObservableCollection<string>();
            //populate with new selection choices{ };
            ToolModel.FilterTitle selection = (ToolModel.FilterTitle)(xList.SelectedItems[0]);
            var toolViewModel = DataContext as ToolViewModel;
            if (toolViewModel != null)
            {
                toolViewModel.Filter = selection;
            }
            bottompanel.Children.Remove(xChooseFilter);
            xList.ItemsSource = MetaDataToDisplay;
        }
    }
}
