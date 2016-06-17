using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class AudioRegionBound : UserControl
    {
        public static readonly DependencyProperty X1Dp = DependencyProperty.Register
    (
         "X1",
         typeof(double),
         typeof(AudioRegionBound),
         new PropertyMetadata(0)
    );
        public static readonly DependencyProperty X2Dp = DependencyProperty.Register
    (
         "X2",
         typeof(double),
         typeof(AudioRegionBound),
         new PropertyMetadata(0)
    );
        public static readonly DependencyProperty Y1Dp = DependencyProperty.Register
    (
         "Y1",
         typeof(double),
         typeof(AudioRegionBound),
         new PropertyMetadata(0)
    );
        public static readonly DependencyProperty Y2Dp = DependencyProperty.Register
    (
         "Y2",
         typeof(double),
         typeof(AudioRegionBound),
         new PropertyMetadata(0)
    );

        private Line _handle;
        private Ellipse _ellipse;
        public AudioRegionBound()
        {
            this.InitializeComponent();
            X1 = System.Convert.ToDouble( GetValue(X1Dp));
            X2 = System.Convert.ToDouble(GetValue(X2Dp));
            Y1 = System.Convert.ToDouble(GetValue(Y1Dp));
            Y2 = System.Convert.ToDouble(GetValue(Y2Dp));
        }
        public double X1
        {
            get { return Handle.X1; }
            set { Handle.X1 = value; }
        }

        public double X2
        {
            get { return Handle.X2; }
            set { Handle.X2 = value; }
        }
        public double Y1
        {
            get { return Handle.Y1; }
            set { Handle.Y1 = value; }
        }
        public double Y2
        {
            get { return Handle.Y2; }
            set { Handle.Y2 = value; }
        }

        


    }
}
