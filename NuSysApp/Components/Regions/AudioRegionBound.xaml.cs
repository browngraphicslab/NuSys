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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Regions
{
    public sealed partial class AudioRegionBound : UserControl
    {
        private Line _handle;
        private Ellipse _ellipse;
        public AudioRegionBound()
        {
            this.InitializeComponent();
            _handle = (Line)GetTemplateChild("Handle");
            _ellipse = (Ellipse) GetTemplateChild("Bulb");

        }
        public double X1
        {
            get { return _handle.X1; }
            set { _handle.X1 = value; }
        }

        public double X2
        {
            get { return _handle.X2; }
            set { _handle.X2 = value; }
        }
        public double Y1
        {
            get { return _handle.Y1; }
            set { _handle.Y1 = value; }
        }
        public double Y2
        {
            get { return _handle.Y2; }
            set { _handle.Y2 = value; }
        }

        


    }
}
