﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace NuSysApp.Util
{
    public sealed partial class RelatedListBox : UserControl
    {
        public ObservableCollection<LibraryElementModel> RelatedElements { get; set; } 

        public RelatedListBox(string tag)
        {
            this.InitializeComponent();
            RelatedElements = new ObservableCollection<LibraryElementModel>();
            this.UpdateRelatedElements(tag);
        }

        private void XRootGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            xMatrixTransform.Matrix = xTransformGroup.Value;

            // Look at the Delta property of the ManipulationDeltaRoutedEventArgs to retrieve
            // the translate X and Y changes
            xCompositeTransform.TranslateX = e.Delta.Translation.X;
            xCompositeTransform.TranslateY = e.Delta.Translation.Y;
        }

        public void UpdateTag(string tag)
        {
            this.UpdateRelatedElements(tag);
        }

        /// <summary>
        /// Updates the observable collection to contain elements related to the passed-in tag
        /// </summary>
        /// <param name="tag"></param>
        private void UpdateRelatedElements(string tag)
        {
            // Remove everything previously in the observable collection
            var count = RelatedElements.Count;
            for (int i = 0; i <count; i++)
            {
                RelatedElements.RemoveAt(0);
            }

            // Find and add all the related content to the observable collection
            var content = SessionController.Instance.ContentController.Values;
            var relatedContent = content.Where(item => item.Keywords.Contains(tag));
            foreach (var model in relatedContent)
            {
                RelatedElements.Add(model);
            }
           
            // Modify the title of the related list box
            xTitle.Text = "Elements about '" + tag + "'";
        }

        /// <summary>
        /// When you tap on an item, zoom in on it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_OnTapped(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var block = grid.Children[1] as TextBlock;
            var model = block.DataContext as LibraryElementModel;
            
            var vms = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(item => ((ElementViewModel)item.DataContext).Controller.LibraryElementModel.Id == model.Id);
            var foo = vms.ToList();
            var element = foo[0].DataContext as ElementViewModel;
            SessionController.Instance.SessionView.Explore(element);

        }

        /// <summary>
        /// Remove this box when the exit button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XExitButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            SessionController.Instance.SessionView.RemoveRelatedListBox();
        }

        /// <summary>
        /// Zooms in on a random node in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XRandomButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            // Zoom in on a random element
            var rnd = new Random();
            var i = rnd.Next(0, RelatedElements.Count());
            var model = RelatedElements[i];
            var vms = SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(item => ((ElementViewModel)item.DataContext).Controller.LibraryElementModel.Id == model.Id);
            var foo = vms.ToList();
            var element = foo[0].DataContext as ElementViewModel;
            SessionController.Instance.SessionView.Explore(element);

            // Make the list view selection reflect these changes
            xListView.DeselectRange(new ItemIndexRange(xListView.SelectedIndex,1));
            xListView.SelectRange(new ItemIndexRange(i,1));
            
        }
    }
}
