using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupNodeView : AnimatableUserControl
    {
        public GroupNodeView( GroupNodeViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                IC.Clip = new RectangleGeometry {Rect = new Rect(0, 0, vm.Width-100, vm.Height-100)};
            };
           
        }
        
    }
}
