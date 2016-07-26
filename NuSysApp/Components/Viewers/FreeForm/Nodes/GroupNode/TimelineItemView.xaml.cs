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
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Nodes.GroupNode
{
    public sealed partial class TimelineItemView : UserControl
    {
        private FrameworkElement _atom;
        public readonly NusysConstants.ElementType EType;
        public TimelineItemView(FrameworkElement image, Object sortElement, FrameworkElement atom, NusysConstants.ElementType type)
        {
            this.InitializeComponent();
            EType = type;
            _atom = atom;

            //image.RenderTransform = null;

            TimelineNode.Children.Add(image); // add node
                                              //image.VerticalAlignment = VerticalAlignment.Bottom;

            //titleTb.Text = title;

            TextBlock tb = new TextBlock();
            tb.Name = "TextBlock";
            tb.Text = sortElement.ToString();
            tb.TextAlignment = TextAlignment.Center;
            tb.FontSize = 11;
            tb.FontWeight = FontWeights.Bold;
            tb.VerticalAlignment = VerticalAlignment.Bottom;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            tb.Foreground = new SolidColorBrush(Colors.Black);
            //tb.Width = 120;

            Grid tbGrid = new Grid();
            //tbGrid.Width = 120;
            //tbGrid.BorderBrush = new SolidColorBrush(Colors.AliceBlue);
            //tbGrid.BorderThickness = new Thickness(2, 0, 2, 2);
            tbGrid.Children.Add(tb);
            TimelinePanel.Children.Add(tbGrid);
        }

        //TODO refactor
        public FrameworkElement getAtom()
        {
            return _atom;
        }

        public void clearChild()
        {
            TimelineNode.Children.Clear();
        }
    }
}
