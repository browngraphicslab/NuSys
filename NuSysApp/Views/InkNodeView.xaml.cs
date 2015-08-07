using System.ComponentModel;
using System.Diagnostics;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class InkNodeView : UserControl
    {
        InkDrawingAttributes _drawingAttributes;
        public InkNodeView(InkNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.SetUpBindings();   
            this.SetUpInk();
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
        }

        #region Helper Methods

        private void SetUpInk()
        {
            _drawingAttributes = new InkDrawingAttributes();
            _drawingAttributes.Color = Windows.UI.Colors.Black;
            _drawingAttributes.Size = new Windows.Foundation.Size(2, 2);
            _drawingAttributes.IgnorePressure = false;
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(_drawingAttributes);
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch;
            inkCanvas.InkPresenter.IsInputEnabled = ((InkNodeViewModel)this.DataContext).IsEditing;//only accept input if node is currently being edited
        }
        private void SetUpBindings()
        {
            var leftBinding = new Binding
            {
                Path = new PropertyPath("X"),
                Mode = BindingMode.TwoWay
            };
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            var topBinding = new Binding
            {
                Path = new PropertyPath("Y"),
                Mode = BindingMode.TwoWay
            };

            this.SetBinding(Canvas.TopProperty, topBinding);
        }

        #endregion Helper Methods

        #region Event Handlers
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.ToggleSelection();
            if (vm.IsSelected == true)
            {
                slideout.Begin();
            }
            else
            {
                slidein.Begin();
            }
            e.Handled = true;

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            InkNodeViewModel vm = (InkNodeViewModel)this.DataContext;
            vm.ToggleEditing();
            inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditing;   
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            var vm = (InkNodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (InkNodeViewModel)this.DataContext;
            vm.CreateAnnotation();
            if (vm.IsAnnotation)
            {
                this.MyGrid.Background = new SolidColorBrush(Color.FromArgb(100, 255, 235, 205));
            }
        }

        public void UpdateInk()
        {
            var vm = (InkNodeViewModel)this.DataContext;
            if (!this.inkCanvas.InkPresenter.StrokeContainer.CanPasteFromClipboard())
            {
                Debug.WriteLine("Could not promote ink");
                vm.WorkSpaceViewModel.DeleteNode(vm);
                return;
            }
            var rect = this.inkCanvas.InkPresenter.StrokeContainer.PasteFromClipboard(new Point(0,0));
            
            vm.Width = rect.Width;
            vm.Height = rect.Height;
            
        }

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (InkNodeViewModel)this.DataContext;
                if (vm.IsSelected)
                {
                    slideout.Begin();
                }
                else
                {
                    slidein.Begin();
                }
            }
        }

        #endregion Event Handlers
    }
}
