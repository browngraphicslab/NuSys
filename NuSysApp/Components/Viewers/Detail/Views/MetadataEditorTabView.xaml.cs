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
using Windows.System;
using Windows.UI;
using Windows.UI.Core;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// <summary>
    /// Defines back end logic for search, sort, and the list view in the metadata editor
    /// </summary>
    public sealed partial class MetadataEditorView : AnimatableUserControl
    {

        private ObservableCollection<MetadataEntry> MetadataCollection { get; set; }
        private List<MetadataEntry> _orgList;
        private List<Image> _shownButtons;
        private List<TextBox> _highlightedTextBoxes; 
        private bool _sortedDescending;

        public IMetadatable Metadatable { set; get; }

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
            _shownButtons = new List<Image>();
            _highlightedTextBoxes = new List<TextBox>();
            CoreWindow.GetForCurrentThread().KeyDown += OnKeyDown;
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
            var dict = Metadatable.GetMetadata() ?? new Dictionary<string, MetadataEntry>();

            // Convert dictionary entries to MetadataEntries, and add to MetadataCollection
            foreach (var key in dict.Keys)
            {

                // Create new entry from the dictionary information
                var entry = dict[key];
                _orgList.Add(entry);
                xKey.Text = "";
                xValue.Text = "";

                // If showing immutable data, add all entries
                if ((bool) xCheckBox.IsChecked)
                {
                    MetadataCollection.Add(entry);
                }

                // Otherwise we only show mutable data
                else if (entry.Mutability==MetadataMutability.MUTABLE)
                {
                    MetadataCollection.Add(entry);
                }

            }
        }

        /// <summary>
        /// Adds a new entry and resets text when the "insert" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddEntryButton_OnClick(object sender, RoutedEventArgs e)
        {

            var key = xKey.Text;
            var val = xValue.Text;

            // Obtains metadata dictionary, and uses it to handle bad input

            var metadata = Metadatable.GetMetadata();
            if (metadata.ContainsKey(key) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(val))
                return;

            var entry = new MetadataEntry(xKey.Text, new List<string>() { xValue.Text}, MetadataMutability.MUTABLE);

            // Adds metadata entry to the library element and updates the listview
            Metadatable.AddMetadata(entry);
            this.Update();
            xKey.Text = "";
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
            var button = sender as Image;
            var stackPanel = button.GetVisualParent() as StackPanel;
            var relPanel = stackPanel.GetVisualParent() as RelativePanel;
            var grid = relPanel.GetVisualParent() as Grid;
            var entry = grid.DataContext as MetadataEntry;
            Metadatable.RemoveMetadata(entry.Key);

            // Finally, updates the ListView to reflect the changes. Note that we are not calling
            // the update method in order to preserve order
            MetadataCollection.Remove(entry);
            _orgList.Remove(entry);
            this.HideSelectionButtonsFromPreviousSelection();
        }


        /// <summary>
        /// Controls the visibility of the delete & edit buttons by calling a helper method when appropriate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XListViewItemGrid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Obtain Grid and Entry 
            var grid = sender as Grid;
            var entry = grid.DataContext as MetadataEntry;

            // First hide all of the currently shown buttons and clear the list
           this.HideSelectionButtonsFromPreviousSelection();

            // If the entry is mutable, toggle the visibility of the selection (delete & edit)
            if (entry != null && (entry.Mutability==MetadataMutability.MUTABLE))
            {
                this.ToggleSelectionButtonsVisibility(grid);
            }
        }

        /// <summary>
        /// Toggles the delete button visibility in the passed in grid by switching the visibility property
        /// </summary>
        /// <param name="grid"></param>
        private void ToggleSelectionButtonsVisibility(Grid grid)
        {
            // The button is a couple panes deep, so loop thru all containers to find it
            foreach (var element in grid.Children)
            {
                if (element.ToString().Equals("Windows.UI.Xaml.Controls.RelativePanel"))
                {
                    var relPanel = element as RelativePanel;
                    foreach (var item in relPanel.Children)
                    {
                        if (item.ToString().Equals("Windows.UI.Xaml.Controls.StackPanel"))
                        {
                            var stackPanel = item as StackPanel;
                            foreach (var child in stackPanel.Children)
                            {
                                if (child.ToString().Equals("Windows.UI.Xaml.Controls.Image"))
                                {
                                    child.Visibility = Visibility.Visible;
                                    _shownButtons.Add(child as Image);
                                }
                            }

                        }
                    }
                }
                    
            }
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
                _sortedDescending = true;
            }
            else
            {
                lst.Reverse();
                _sortedDescending = false;
            }

            // Clear old collection, re-add items in sorted order
            MetadataCollection.Clear();
            foreach (var entry in lst)
            {
                MetadataCollection.Add(entry);
            }

            // Make sure to get rid of any artficats from the previous selection
            this.HideSelectionButtonsFromPreviousSelection();
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
            if ((bool)xCheckBox.IsChecked)
            {
                keyResult = new HashSet<MetadataEntry>(_orgList.Where(w => this.StringContains(w.Key, text, StringComparison.OrdinalIgnoreCase)));
                valResult = new HashSet<MetadataEntry>(_orgList.Where(w => this.EntryContainsValue(w, text)));

            }

            // Otherwise, the switch is off and only search through mutable entries
            else
            {
                keyResult = new HashSet<MetadataEntry>(_orgList.Where(w => (this.StringContains(w.Key, text, StringComparison.OrdinalIgnoreCase)) && (w.Mutability==MetadataMutability.MUTABLE)));
                valResult = new HashSet<MetadataEntry>(_orgList.Where(w => (this.EntryContainsValue(w, text)) && (w.Mutability == MetadataMutability.MUTABLE)));
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
        /// Checks if a string value is in the list of an entry's values
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="toCheck"></param>
        /// <returns></returns>
        private bool EntryContainsValue(MetadataEntry entry, string toCheck)
        {
            foreach (var value in entry.Values)
            {
                if (this.StringContains(value, toCheck, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
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
                var keys = textbox.Text.Split(',').Select(sValue => sValue.Trim()).ToArray();
                entry.Values = keys.ToList();
            }
        }

        /// <summary>
        /// Finds the text boxes and makes it obvious that you should edit them.
        /// See .xaml file for grid/pane containment.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XEditButton_OnClick(object sender, RoutedEventArgs e)
        {

            // Get the containers
            var button = sender as Image;
            var sp = button.GetVisualParent() as StackPanel;
            var rp = sp.GetVisualParent() as RelativePanel;
            var grid = rp.GetVisualParent() as Grid;

            // The first text box is in the relative panel
            foreach (var child in rp.Children)
            {
                if (child.ToString().Equals("Windows.UI.Xaml.Controls.TextBox"))
                {
                    var box = child as TextBox;     
                    box.Background = new SolidColorBrush(Colors.CornflowerBlue);
                    box.IsHitTestVisible = true;
                    _highlightedTextBoxes.Add(box);
                }
            }

            // The second text box is in the grid 
            foreach (var child in grid.Children)
            {
                if (child.ToString().Equals("Windows.UI.Xaml.Controls.TextBox"))
                {
                    var box = child as TextBox;
                    box.Background = new SolidColorBrush(Colors.CornflowerBlue);
                    box.IsHitTestVisible = true;
                    _highlightedTextBoxes.Add(box);
                }
            }
        }

        /// <summary>
        /// Updates the observable collection when check box checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCheckBox_OnChecked(object sender, RoutedEventArgs e)
        {
            this.HideSelectionButtonsFromPreviousSelection();
            this.Update();
        }

        /// <summary>
        /// Updates the observable collection when check box unchecked. Update method is not called in order to preserve order
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCheckBox_OnUnchecked(object sender, RoutedEventArgs e)
        {
            this.HideSelectionButtonsFromPreviousSelection();

            // Find immutable entries and remove them
            var imEntries = MetadataCollection.Where(s => s.Mutability == MetadataMutability.IMMUTABLE).ToList();
            foreach (var entry in imEntries)
            {
                MetadataCollection.Remove(entry);
            }
        }

        /// <summary>
        /// Hides the edit and delete buttons from the previous selection. Also removes the highlight
        /// from their text boxes
        /// </summary>
        private void HideSelectionButtonsFromPreviousSelection()
        {
            foreach (var box in _highlightedTextBoxes)
            {
                box.Background = null;
                box.IsHitTestVisible = false;
            }
            foreach (var button in _shownButtons)
            {
                button.Visibility = Visibility.Collapsed;
            }
            _highlightedTextBoxes.Clear();
            _shownButtons.Clear();
        }

        /// <summary>
        /// Enables the user to add metadata using the enter key, if they have filled out the 
        /// appropriate info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnKeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Enter && !xKey.Text.Equals("") && !xValue.Text.Equals(""))
            {
                this.AddEntryButton_OnClick(new Object(), new RoutedEventArgs());
                xKey.Focus(FocusState.Keyboard);
            }
        }

    }
}