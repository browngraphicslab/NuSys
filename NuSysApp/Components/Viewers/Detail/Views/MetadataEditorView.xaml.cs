
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Threading.Tasks;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// Defines back end logic for search, sort, and the list view in the metadata editor
    /// </summary>
    public sealed partial class MetadataEditorView : UserControl
    {

        private ObservableCollection<MetadataEntry> MetadataCollection { get; set; }
        private List<MetadataEntry> _orgList;
        // important for edge case, where your pointer goes over the button first instead of the whole grid
        private bool _edgeCaseButtonExited;
        private bool _sortedDescending;

        public DetailViewerView DetailViewerView { set; get; }


        /// <summary>
        /// The observable collection will contain displayed entries, 
        /// and _orgList will contain the original entries
        /// </summary>
        public MetadataEditorView()
        {
            this.InitializeComponent();
            MetadataCollection = new ObservableCollection<MetadataEntry>();
            _orgList = new List<MetadataEntry>();
            _sortedDescending = false;
        }

        /// <summary>
        /// Updates the MetadataCollection (the data displayed in the ListView) based
        /// on the specific LibraryElement being investigated. The library element's 
        /// metadata is obtained from the DetailViewer.
        /// </summary>
        public void Update()
        {
            // Clear all previous lists/collections
            MetadataCollection.Clear();
            _orgList.Clear();

            // Extract dictionary from libraryelementmodel.
            var vm = (DetailViewerViewModel)DetailViewerView.DataContext;
            var dict = vm.CurrentElementController.LibraryElementModel.Metadata ?? new Dictionary<string, Tuple<string, bool>>();

            // Convert dictionary entries to MetadataEntries, and add to MetadataCollection
            foreach (var key in dict.Keys)
            {

                // Create new entry from the dictionary information
                var entry = new MetadataEntry(key, dict[key].Item1, dict[key].Item2);
                _orgList.Add(entry);
                xField.Text = "";
                xValue.Text = "";

                // If showing immutable data, add all entries
                if (xToggleSwitch.IsOn)
                {
                    MetadataCollection.Add(entry);
                }

                // Otherwise we only show mutable data
                else if (entry.Mutability)
                {
                    MetadataCollection.Add(entry);
                }

            }
        }


        /// <summary>
        /// Controls the presence of immutable entries in the list view, based on the toggle switch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            this.Update();
        }

        /// <summary>
        /// Adds a new entry and resets text when the "insert" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {

            var key = xField.Text;
            var val = xValue.Text;

            // Obtains metadata dictionary, and uses it to handle bad input
            var vm = (DetailViewerViewModel)DetailViewerView.DataContext;
            var metadata = vm.CurrentElementController.LibraryElementModel.Metadata;
            if (metadata.ContainsKey(key) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(val))
                return;


            var entry = new MetadataEntry(xField.Text, xValue.Text, true);

            // Adds metadata entry to the library element and updates the listview
            vm.CurrentElementController.LibraryElementController.AddMetadata(entry);
            this.Update();
            xField.Text = "";
            xValue.Text = "";
        }


        /// <summary>
        /// Deletes the metadata associated with the delete button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XDeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            // Finds the MetadataEntry, then uses that to delete the metadata from the lib element
            var button = sender as Button;
            var grid = button.GetVisualParent() as Grid;
            var entry = grid.DataContext as MetadataEntry;
            var vm = (DetailViewerViewModel)DetailViewerView.DataContext;
            vm.CurrentElementController.LibraryElementController.RemoveMetadata(entry.Key);

            // Finally, updates the ListView to reflect the changes
            this.Update();
        }


        /// <summary>
        /// Controls the visibility of the delete button by calling a helper method when appropriate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XListViewItemGrid_OnPointerEnterExit(object sender, PointerRoutedEventArgs e)
        {
            // Obtain Grid and Entry 
            var grid = sender as Grid;
            var entry = grid.DataContext as MetadataEntry;

            // If the entry is mutable, toggle the visibility of the delete button
            if (entry != null && entry.Mutability)
            {
                this.ToggleDeleteButtonVisibility(grid);
            }
        }

        /// <summary>
        /// Toggles the delete button visibility in the passed in grid by switching the visibility property
        /// </summary>
        /// <param name="grid"></param>
        private void ToggleDeleteButtonVisibility(Grid grid)
        {
            // Loop thru the grid's children to find the button
            foreach (var element in grid.Children)
            {
                if (element.ToString().Equals("Windows.UI.Xaml.Controls.Button"))
                {
                    // Once the button is found, toggle visibility
                    if (element.Visibility == Visibility.Collapsed && !_edgeCaseButtonExited)
                    {
                        element.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        element.Visibility = Visibility.Collapsed;
                        _edgeCaseButtonExited = false;
                    }
                }
            }
        }

        /// <summary>
        /// Ensures the button will dissapear when you move out of it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteButton_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.Visibility = Visibility.Collapsed;
            _edgeCaseButtonExited = true;
        }


        /// <summary>
        /// Sorts the items in the listview when the button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSortButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            List<MetadataEntry> lst = new List<MetadataEntry>(MetadataCollection.OrderBy(a => a.Key));

            // CHANGE THIS WHEN YOU GET AN ICON
            if (!_sortedDescending)
            {
                button.Content = "^";
                _sortedDescending = true;
            }
            else
            {
                lst.Reverse();
                _sortedDescending = false;
                button.Content = "v";
            }

            // Clear old collection, re-add items in sorted order
            MetadataCollection.Clear();
            foreach (var entry in lst)
            {
                MetadataCollection.Add(entry);
            }
        }



        /// <summary>
        /// Modifies the metadata observable collection based on the search text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // Extract the text, and revert list to normal if the box is empty
            var box = sender as TextBox;
            var text = box.Text;
            if (text == "")
            {
                this.Update();
                return;
            }

            // Search results are stored in sets so there are no repeats
            var keyResult = new HashSet<MetadataEntry>();
            var valResult = new HashSet<MetadataEntry>();

            // If toggle switch on, search thru both mutable and immutable entries
            if (xToggleSwitch.IsOn)
            {
                keyResult = new HashSet<MetadataEntry>(_orgList.Where(w => this.StringContains(w.Key, text, StringComparison.OrdinalIgnoreCase)));
                valResult = new HashSet<MetadataEntry>(_orgList.Where(w => this.StringContains(w.Value, text, StringComparison.OrdinalIgnoreCase)));
            }

            // Otherwise, the switch is off and only search through mutable entries
            else
            {
                keyResult = new HashSet<MetadataEntry>(_orgList.Where(w => (this.StringContains(w.Key, text, StringComparison.OrdinalIgnoreCase)) && w.Mutability));
                valResult = new HashSet<MetadataEntry>(_orgList.Where(w => (this.StringContains(w.Value, text, StringComparison.OrdinalIgnoreCase)) && w.Mutability));
            }

            // Join the result sets, and add them back to the observable collection
            keyResult.UnionWith(valResult);
            MetadataCollection.Clear();
            foreach (var element in keyResult)
            {
                MetadataCollection.Add(element);
            }
        }

        /// <summary>
        /// Enables using contains that is insensitive to case
        /// </summary>
        /// <param name="source"></param>
        /// <param name="toCheck"></param>
        /// <param name="comp"></param>
        /// <returns></returns>
        private bool StringContains(string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

        /// <summary>
        /// Saves modified keys 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KeyTextBox_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {

            var textbox = sender as TextBox;
            var entry = textbox.DataContext as MetadataEntry;
            if (!textbox.Text.Equals(""))
            {
                entry.Key = textbox.Text;
            }
        }

        /// <summary>
        /// Saves modified values 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ValueTextBox_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            var textbox = sender as TextBox;
            var entry = textbox.DataContext as MetadataEntry;
            if (!textbox.Text.Equals(""))
            {
                entry.Value = textbox.Text;
            }
        }
    }
}