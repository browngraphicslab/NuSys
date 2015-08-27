using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PinView : UserControl
    {
        public PinView(PinViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
        }
    }
}
