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

namespace NuSysApp.Components.Viewers.Detail.Views
{
    public sealed partial class MetadataEditorView : UserControl
    {

        private ObservableCollection<MetadataEntry> Metadata;
        private List<MetadataEntry> _mutableEntries;
        private List<MetadataEntry> _immutableEntries;
        // important for edge case, where your pointer goes over the button first instead of the whole grid
        private bool _edgeCaseButtonExited; 

        public MetadataEditorView()
        {
            this.InitializeComponent();
            Metadata = new ObservableCollection<MetadataEntry>();
            _mutableEntries = new List<MetadataEntry>();
            _immutableEntries = new List<MetadataEntry>();

            _immutableEntries.Add(new MetadataEntry("Name", "Nick", false));
            _immutableEntries.Add(new MetadataEntry("Login", "nturley", false));
            _immutableEntries.Add(new MetadataEntry("Year", "Senior", false));
            _mutableEntries.Add(new MetadataEntry("Project", "NuSys", true));
            _mutableEntries.Add(new MetadataEntry("Role", "Team Lead", true));

            foreach (var entry in _mutableEntries)
            {
                Metadata.Add(entry);
            }

            xToggleSwitch.IsOn = false;
            _edgeCaseButtonExited = false;
        }

        /// <summary>
        /// Adds a new entry and resets text when the "insert" button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // Only add an entry that has a key
            if (!xField.Text.Equals(""))
            {
                var entry = new MetadataEntry(xField.Text, xValue.Text, true);
                Metadata.Add(entry);
                xField.Text = "";
                xValue.Text = "";
            }
        }

        /// <summary>
        /// Controls the presence of immutable entries in the list view, based on the toggle switch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            // If it is switched on, that means we need to add the immutable data
            if (xToggleSwitch.IsOn)
            {
                foreach (var entry in _immutableEntries)
                {
                    Metadata.Add(entry);
                }
            }

            // Otherwise, it is switched off and we should remove the immutable entries
            else
            {
                foreach (var entry in _immutableEntries)
                {
                    Metadata.Remove(entry);
                }
            }
        }

        /// <summary>
        /// Deletes the entry associated with the delete button clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XDeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var grid = button.GetVisualParent() as Grid;
            var entry = grid.DataContext as MetadataEntry;
            _mutableEntries.Remove(entry);
            Metadata.Remove(entry);

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
            if (entry.Mutability)
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
            List<MetadataEntry> lst = new List<MetadataEntry>(Metadata.OrderBy(a => a.Key));
            if (button.Content.Equals("v"))
            {
                
                button.Content = "^";
            }
            else
            {
                lst.Reverse();
                button.Content = "v";
            }

            Metadata.Clear();
            
            foreach (var entry in lst)
            {
                Metadata.Add(entry);
            }
        }

       
        
        /// <summary>
        /// Modifies the metadata observable collection based on the search text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            var text = box.Text;

            var allEntries = new List<MetadataEntry>(_mutableEntries);
            if (xToggleSwitch.IsOn)
            {
                allEntries.AddRange(_immutableEntries);
            }
        
            var filtered = new HashSet<MetadataEntry>();

            var keyResult = new HashSet<MetadataEntry>(allEntries.Where(w => this.StringContains(w.Key,text,StringComparison.OrdinalIgnoreCase)));
            var valResult = new HashSet<MetadataEntry>(allEntries.Where(w => this.StringContains(w.Value, text, StringComparison.OrdinalIgnoreCase))); 
            
            keyResult.UnionWith(valResult);
            Metadata.Clear();
            foreach (var element in keyResult)
            {
                Metadata.Add(element);
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
        /// Saves modified entries 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
           
            var textbox = sender as TextBox;
            var entry = textbox.DataContext as MetadataEntry;
            if (!textbox.Text.Equals(""))
            {
                if (textbox.PlaceholderText.Equals("Enter a Field"))
                {
                    entry.Key = textbox.Text;
                }
                else
                {
                    entry.Value = textbox.Text;
                }
            }
            
            
        }
    }
}
