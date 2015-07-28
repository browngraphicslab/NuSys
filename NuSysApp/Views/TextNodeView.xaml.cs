using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView : UserControl
    {
        private bool _isEditing; //bool used to enable and disable editing (texblock vs textbox) (see more in NodeViewModel.cs)
        //editing is handeled using methods: IsEditing, ToggleEditing (all in NodeViewModel.cs), Edit_Click (in this file)
        public TextNodeView(TextNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _isEditing = false; //sets the text block to be in front of textbox so no editing is possible
            this.SetUpBindings();
            inkCanvas.InkPresenter.IsInputEnabled = false;
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink

        }

        #region Helper Methods
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
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
        
        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            vm.ToggleSelection();
            e.Handled = true;

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            vm.ToggleEditing();
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }
            #endregion Event Handlers
        }
        private void EditC_Click(object sender, RoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            vm.ToggleEditingC();
            inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;   
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }
        }
        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void UserControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            vm.CreateAnnotation();
            if (vm.IsAnnotation)
            {
                this.MyGrid.Background = new SolidColorBrush(Color.FromArgb(100, 255, 235, 205));
                this.textBlock.Foreground = new SolidColorBrush(Colors.Black);
                this.textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }
    }
}