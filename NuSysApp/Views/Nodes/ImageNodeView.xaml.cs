using System.ComponentModel;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : UserControl
    {
        public ImageNodeView(ImageNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.SetUpBindings();
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
            inkCanvas.InkPresenter.IsInputEnabled = false;
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
           Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink
        }
        private void SetUpBindings()
        {
            Binding leftBinding = new Binding() { Path = new PropertyPath("X") };
            leftBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            Binding topBinding = new Binding() { Path = new PropertyPath("Y") };
            topBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.TopProperty, topBinding);
        }


        #region Event Handlers
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            ImageNodeViewModel vm = (ImageNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
       //     vm.Resize((e.Delta.Scale - 1) * vm.Width, (e.Delta.Scale - 1) * vm.Height);//TO DO: POSSIBLY REMOVE THIS FEATURE FOR LACK OF CONSISTENCY
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ImageNodeViewModel vm = (ImageNodeViewModel)this.DataContext;
            Debug.WriteLine(ICB.ActualHeight+" ICB" + inkCanvas.ActualHeight);
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ImageNodeViewModel vm = (ImageNodeViewModel)this.DataContext;
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

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            ImageNodeViewModel vm = (ImageNodeViewModel)this.DataContext;
            vm.Remove();
        }
        private void EditC_Click(object sender, RoutedEventArgs e)
        {
            var vm = (ImageNodeViewModel)this.DataContext;
            vm.ToggleEditingInk();
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

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (ImageNodeViewModel)this.DataContext;
                if (vm.IsSelected)
                {
                    slideout.Begin();
                }
                else
                {
                    slidein.Begin();
                    if (vm.IsEditingInk == true)
                    {
                        vm.ToggleEditingC();
                        inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;
                    }
                }
                if (ManipulationMode == ManipulationModes.All)
                {
                    ManipulationMode = ManipulationModes.None;
                }
                else
                {
                    ManipulationMode = ManipulationModes.All;
                }
            }
        }

        #endregion Event Handlers
    }
}
