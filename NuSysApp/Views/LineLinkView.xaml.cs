using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

    

namespace NuSysApp
{
    /// <summary>
    /// this class is for the line representation of links.
    /// </summary>
    public sealed partial class LineLinkView : UserControl
    {
        public LineLinkView(LinkViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            this.SetUpBindings();
        }

        /// <summary>
        /// sets up bindings for line links with the linkviewmodel.
        /// TO DO: THIS NO LONGER WORKS BECAUSE THE BINDINGS X, Y NO LONGER EXIST
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
