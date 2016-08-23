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

namespace NuSysApp2
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
            this.RenderTransform = new CompositeTransform();
            X1 = System.Convert.ToDouble( GetValue(X1Dp));
            X2 = System.Convert.ToDouble(GetValue(X2Dp));
            Y1 = System.Convert.ToDouble(GetValue(Y1Dp));
            Y2 = System.Convert.ToDouble(GetValue(Y2Dp));
        }
        public double X1
        {
            get { return System.Convert.ToDouble( GetValue(X1Dp)); }
            set
            {
                SetValue(X1Dp, value);
       //         (this.RenderTransform as CompositeTransform).TranslateX = value;
            }
        }
        public double X2
        {
            get { return System.Convert.ToDouble( GetValue(X2Dp)); }
            set
            {
                SetValue(X2Dp, value);
            }
        }
        public double Y1
        {
            get { return System.Convert.ToDouble( GetValue(Y1Dp)); }
            set
            {
                SetValue(Y1Dp, value);
                Handle.Y1 = value;
            }
        }
        public double Y2
        {
            get { return System.Convert.ToDouble( GetValue(Y2Dp)); }
            set
            {
                SetValue(Y2Dp, value);
                Handle.Y2 = value;
            }
        }
    }
}
