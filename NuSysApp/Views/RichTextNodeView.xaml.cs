using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class RichTextNodeView : UserControl
    {
        private bool _isEditing; //bool used to enable and disable editing (texblock vs textbox) (see more in NodeViewModel.cs)
        //editing is handeled using methods: IsEditing, ToggleEditing (all in NodeViewModel.cs), Edit_Click (in this file)
        public RichTextNodeView(RichTextNodeViewModel vm)
        {
            this.DataContext = vm;
            _isEditing = false; //sets the text block to be in front of textbox so no editing is possible
            this.InitializeComponent();
            
           
            this.SetUpBindings();
        }

        #region Helper Methods
        private void SetUpBindings()
        {
            Binding leftBinding = new Binding() { Path = new PropertyPath("X") };
            leftBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.LeftProperty, leftBinding);

            Binding topBinding = new Binding() { Path = new PropertyPath("Y") };
            topBinding.Mode = BindingMode.TwoWay;
            this.SetBinding(Canvas.TopProperty, topBinding);
        }

        #endregion Helper Methods

        #region Event Handlers
        private void UserControl_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Resizer_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }
        
        private void UserControl_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.ToggleSelection();
            e.Handled = true;

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.ToggleEditing();
           
            #endregion Event Handlers
        }

        private void UserControl_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void delete_Click(object sender, RoutedEventArgs e)
        {
            RichTextNodeViewModel vm = (RichTextNodeViewModel)this.DataContext;
            vm.Remove();
        }

    }
}