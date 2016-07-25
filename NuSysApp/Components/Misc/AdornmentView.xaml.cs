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

namespace NuSysApp.Components.Misc
{
    public sealed partial class AdornmentView : AnimatableUserControl
    {
        public AdornmentView(List<Windows.Foundation.Point> shapePoints)
        {
            this.InitializeComponent();
            (Adornment.RenderTransform as CompositeTransform).TranslateX = 50000;
            (Adornment.RenderTransform as CompositeTransform).TranslateY = 50000;

            

            // Search for the left and top most point
            var leftMost = Double.PositiveInfinity;
            var topMost = Double.PositiveInfinity;
            foreach (var point in shapePoints)
            {
                if (point.X < leftMost)
                {
                    leftMost = point.X;
                }
                if (point.Y < topMost)
                {
                    topMost = point.Y;
                }

            }

            // Adjust and add points to the collection
            var col = new PointCollection();
            foreach (var point in shapePoints)
            {
                var adjustedPt = new Point(point.X-leftMost, point.Y-topMost);
                col.Add(adjustedPt);
            }
            Adornment.Points = col;
        }
    }
}
