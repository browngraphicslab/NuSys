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
    public sealed partial class GroupCircleView : AnimatableUserControl
    {
        public GroupCircleView()
        {
            RenderTransform = new CompositeTransform();
            this.InitializeComponent();
            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                IC.Clip = new RectangleGeometry { Rect = new Rect(0, 50, args.NewSize.Width, args.NewSize.Height- 100) };
            };

        }
    }
}
