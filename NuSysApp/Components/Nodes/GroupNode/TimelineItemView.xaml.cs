﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
        public TimelineItemView(FrameworkElement atom, DateTime date)
        {
            this.InitializeComponent();

            TimelineNode.Children.Add(atom); // add node
            TimelineNode.Height = 80;
            atom.VerticalAlignment = VerticalAlignment.Bottom;

            Line line = new Line()
            {
                Y1 = TimelineNode.ActualHeight,
                Y2 = TimelineNode.ActualHeight + 30,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 3,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            TimelinePanel.Children.Add(line); // add line

            TextBlock tb = new TextBlock();
            tb.Text = date.ToString("MM/dd/yy H:mm:ss");
            tb.FontSize = 15;
            tb.VerticalAlignment = VerticalAlignment.Bottom;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            //TimelinePanel.Children.Add(tb); // add label   

            Grid tbGrid = new Grid();
            tbGrid.BorderBrush = new SolidColorBrush(Colors.Black);
            tbGrid.BorderThickness = new Thickness(2);
            tbGrid.Children.Add(tb);
            TimelinePanel.Children.Add(tbGrid);
        }

        public void clearChild()
        {
            TimelineNode.Children.Clear();
        }
    }
}
