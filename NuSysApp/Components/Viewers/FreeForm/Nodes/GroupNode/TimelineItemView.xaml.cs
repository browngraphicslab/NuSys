using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Nodes.GroupNode
{
    public sealed partial class TimelineItemView : UserControl
    {
        public TimelineItemView(FrameworkElement atom, Object sortElement)
        {
            this.InitializeComponent();
            atom.RenderTransform = null;
            TimelineNode.Children.Add(atom); // add node
            atom.VerticalAlignment = VerticalAlignment.Bottom;
        
            TextBlock tb = new TextBlock();
            tb.Name = "TextBlock";
            tb.Text = sortElement.ToString();
            tb.TextAlignment = TextAlignment.Center;
            tb.FontSize = 11;
            tb.FontWeight = FontWeights.ExtraBold;
            tb.VerticalAlignment = VerticalAlignment.Bottom;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Foreground = new SolidColorBrush(Colors.Navy);
            //tb.Width = 120;

            Grid tbGrid = new Grid();
            //tbGrid.Width = 120;
            //tbGrid.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            //tbGrid.BorderThickness = new Thickness(2, 0, 2, 2);
            tbGrid.Children.Add(tb);
            TimelinePanel.Children.Add(tbGrid);
        }

        public void clearChild()
        {
            TimelineNode.Children.Clear();
        }
    }
}
