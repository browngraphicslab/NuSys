using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

    /// <summary>
    /// this class is for the line representation of links.
    /// </summary>

namespace NuStarterProject
{
    public sealed partial class LinkView : UserControl
    {
        public LinkView(LinkViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.SetUpBindings();
        }

        /// <summary>
        /// sets up bindings for line links with the linkviewmodel.
        /// </summary>
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
    }
}
