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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioRegionView : UserControl
    {
        private bool _toggleManipulation;
        public delegate void RegionSelectedEventHandler(object sender, bool selected);
        public event RegionSelectedEventHandler OnSelected;
        public AudioRegionView(AudioRegionViewModel vm)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            _toggleManipulation = false;
        }

        private void Handle_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = true;
        }


        private void Handle_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            
        }

        private void Handle_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _toggleManipulation = false;
        }
    }
}
