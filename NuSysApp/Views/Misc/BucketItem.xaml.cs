using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace NuSysApp {
    public sealed partial class BucketItem : UserControl
    {
        public BucketItem(string rtf)
        {
            this.InitializeComponent();
            
            textBox.TextChanged += delegate
            {
                var contentHeight = textBox.ComputeRtfHeight();
                textBox.Height = contentHeight;
                //var scale = 300.0 / contentHeight;
                var scale = 0.8;
                textBox.RenderTransform = new ScaleTransform { ScaleX = scale, ScaleY = scale };
            };

            textBox.Rtf = rtf;

        }
    }
}
