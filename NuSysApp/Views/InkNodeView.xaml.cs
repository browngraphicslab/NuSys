using Windows.UI;
using Windows.UI.Input.Inking;
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
        #endregion Event Handlers
    }
}
