using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

            GroupNodeTimelineViewModel viewModel = new GroupNodeTimelineViewModel(model);

            DefaultButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            TimeLineButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            ListButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);

            // create three different views
            timelineView = new GroupNodeTimelineView(viewModel);
            dataGridView = new GroupNodeDataGridView(new GroupNodeDataGridViewModel(model));
            expandedView = new GroupNodeExpandedView();
            timelineView.Opacity = 0;
            dataGridView.Opacity = 0;
            expandedView.Opacity = 1; 
            timelineView.IsHitTestVisible = false;
            dataGridView.IsHitTestVisible = false;
            expandedView.IsHitTestVisible = false;

            

            var child = (Grid)ExpandedBorder.Child;
            child.Children.Insert(0,timelineView);
            child.Children.Add(dataGridView);
            child.Children.Add(expandedView);
        }

        

        private void MenuDetailButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button tb = (Button)sender;
            
            timelineView.Opacity = 0;
            dataGridView.Opacity = 0;
            expandedView.Opacity = 0;
            timelineView.IsHitTestVisible = false;
            dataGridView.IsHitTestVisible = false;
            expandedView.IsHitTestVisible = false;
            
            if (tb.Name == "DefaultButton")
            {
                expandedView.Opacity = 1;
                expandedView.IsHitTestVisible = true;
            }   
            else if (tb.Name == "TimeLineButton")
            {
                timelineView.Opacity = 1;
                timelineView.IsHitTestVisible = true;
            }
            else if (tb.Name == "ListButton")
            {
                dataGridView.Opacity = 1;
                dataGridView.IsHitTestVisible = true;
            }
                
        }
    }
}
