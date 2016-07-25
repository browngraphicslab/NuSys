using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BarChartItem : UserControl
    {
        private BarChartItemViewModel _vm;

        public BarChartItem(BarChartItemViewModel vm)
        {
            this.InitializeComponent();
            _vm = vm;
            this.DataContext = _vm;
            _vm.IsSelected = false;

        }

        public Rectangle Rectangle
        {
            get { return xRectangle; }
        }

        private void XRectangle_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            _vm.Color = _vm.SelectedColor;
        }

        private void XRectangle_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_vm.IsSelected == false)
            {
                _vm.Color = _vm.NotSelectedColor;
            }
        }

        private void XRectangle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void XRectangle_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //throw new NotImplementedException();
        }
    }
}
