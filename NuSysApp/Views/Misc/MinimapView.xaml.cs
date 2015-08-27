using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MinimapView : UserControl
    {
        public MinimapView()
        {
            this.InitializeComponent();
            
       //     this.DataContext = new MinimapViewModel();
       //     ((MinimapViewModel)DataContext).PinList = PinList;
        }

        public static readonly DependencyProperty pinList = DependencyProperty.Register(
          "PinList",
          typeof(ObservableCollection<PinViewModel>),
          typeof(MinimapView),
          new PropertyMetadata(new ObservableCollection<PinViewModel>())
        );
        public ObservableCollection<PinViewModel> PinList
        {
            get { return (ObservableCollection<PinViewModel>)GetValue(pinList); }
            set
            {
                SetValue(pinList, value);
       //         ((MinimapViewModel)DataContext).PinList = value;
            }
        }
        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var pinvm = ((TextBlock)sender).DataContext as PinViewModel;
            var vm = (WorkspaceViewModel)this.DataContext;
            
            var c = new CompositeTransform {
                ScaleX = 1,
                ScaleY = 1,
                TranslateX = -pinvm.Transform.Matrix.OffsetX + ((Canvas)this.Parent).ActualWidth / 2,
                TranslateY = -pinvm.Transform.Matrix.OffsetY + ((Canvas)this.Parent).ActualHeight / 2
            };
            vm.CompositeTransform = c;
        }

        private void ShowPins(object sender, TappedRoutedEventArgs e)
        {
            if (IC2.Visibility == Visibility.Collapsed)
            {
                IC2.Visibility = Visibility.Visible;
                PinButton.Content = "pins -";
            }
            else
            {
                IC2.Visibility = Visibility.Collapsed;
                PinButton.Content = "pins +";
            }
            
        }
    }
}
