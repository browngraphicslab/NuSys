﻿using System;
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
using MyToolkit.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupNodeExpandedDefaultView : AnimatableUserControl
    {
        private GroupNodeTimelineView timelineView;
        private GroupNodeExpandedView expandedView;
        private GroupNodeDataGridView dataGridView;

        public GroupNodeExpandedDefaultView(NodeContainerModel model)
        {
            this.InitializeComponent();

            DefaultButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            TimeLineButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            ListButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);

            // create three different views
            timelineView = new GroupNodeTimelineView(new GroupNodeTimelineViewModel(new ElementCollectionController(model)));
            dataGridView = new GroupNodeDataGridView(new GroupNodeDataGridViewModel(new ElementCollectionController(model)));
            expandedView = new GroupNodeExpandedView();
            timelineView.Opacity = 0;
            dataGridView.Opacity = 0;
            expandedView.Opacity = 1; //default
            
            var child = (Grid)ExpandedBorder.Child;
            child.Children.Add(timelineView);
            child.Children.Add(dataGridView);
            child.Children.Add(expandedView);
        }

        private void MenuDetailButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button tb = (Button)sender;

            timelineView.Opacity = 0;
            dataGridView.Opacity = 0;
            expandedView.Opacity = 0;

            if (tb.Name == "DefaultButton")
                expandedView.Opacity = 1;
            else if (tb.Name == "TimeLineButton")
                timelineView.Opacity = 1;
            else if (tb.Name == "ListButton")
                dataGridView.Opacity = 1;
        }
    }
}
