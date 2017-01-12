using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class ReadOnlyMetadataWindow : ReadOnlyModeWindow
    {
        private ScrollableTextboxUIElement _searchTextBox;

        private ListViewUIElementContainer<MetadataEntry> _metadata_listview;

        private CheckBoxUIElement _showImmutableCheckbox;

        private LibraryElementController _controller;
        public ReadOnlyMetadataWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            _searchTextBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Background = Colors.Azure,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                PlaceHolderText = "Search"
            };
            AddChild(_searchTextBox);

            _showImmutableCheckbox = new CheckBoxUIElement(this, ResourceCreator, false)
            {
                LabelText = "Show Immutable"
            };
            AddChild(_showImmutableCheckbox);

            _searchTextBox.TextChanged += OnSearchTextChanged;
            _showImmutableCheckbox.Selected += OnShowImmutableSelectionChanged;
        }

        public void UpdateList(LibraryElementController controller)
        {
            if (_controller != null)
            {
                _controller.MetadataChanged -= _controller_MetadataChanged;
            }
            _controller = controller;
            CreateListView();
            _controller.MetadataChanged += _controller_MetadataChanged;
        }

        private void OnShowImmutableSelectionChanged(CheckBoxUIElement sender, bool show_immutable)
        {
            filterlist();
        }

        private void filterlist()
        {
            _metadata_listview.ClearItems();
            var filtered_metadata = filter_by_mutability(new List<MetadataEntry>(_controller.GetMetadata().Values),
                _showImmutableCheckbox.IsSelected);
            filtered_metadata = filter_by_search_text(filtered_metadata, _searchTextBox.Text);
            _metadata_listview.AddItems(filtered_metadata);
        }

        private List<MetadataEntry> filter_by_mutability(List<MetadataEntry> metadataToBeFiltered, bool show_immutable)
        {
            if (show_immutable)
            {
                return new List<MetadataEntry>(metadataToBeFiltered.Where(entry => entry.Mutability == MetadataMutability.IMMUTABLE));
            }
            return metadataToBeFiltered;
        }

        private List<MetadataEntry> filter_by_search_text(List<MetadataEntry> metadataToBeFiltered, string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                return metadataToBeFiltered;
            }
            return new List<MetadataEntry>(metadataToBeFiltered.Where(entry => entry.Key.Contains(searchText) || entry.Values.Contains(searchText)));
        }

        private void OnSearchTextChanged(InteractiveBaseRenderItem item, string text)
        {
            filterlist();
        }

        public override void Dispose()
        {

            _controller.MetadataChanged -= _controller_MetadataChanged;
            _searchTextBox.TextChanged -= OnSearchTextChanged;
            _showImmutableCheckbox.Selected -= OnShowImmutableSelectionChanged;

            base.Dispose();
        }


        private void _controller_MetadataChanged(object source)
        {
            _metadata_listview.ClearItems();
            _metadata_listview.AddItems(new List<MetadataEntry>(_controller.GetMetadata().Values));
        }

        private void CreateListView()
        {
            ICanvasResourceCreatorWithDpi resourceCreator;
            _metadata_listview = new ListViewUIElementContainer<MetadataEntry>(this, ResourceCreator)
            {
                Background = Colors.White,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_metadata_listview);

            var listColumn = new ListTextColumn<MetadataEntry>();
            listColumn.Title = "Key";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = metadataEntry => metadataEntry.Key;

            var listColumn2 = new ListTextColumn<MetadataEntry>();
            listColumn2.Title = "Value";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction =
                metadataEntry => string.Join(", ", metadataEntry.Values.Select(value => string.Join(", ", value)));

            _metadata_listview.AddColumns(new List<ListColumn<MetadataEntry>> { listColumn, listColumn2 });

            _metadata_listview.AddItems(new List<MetadataEntry>(_controller.GetMetadata().Values));
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_metadata_listview == null)
            {
                return;
            }
            // helper variable, the current vertical spacing from the top of the window
            var vertical_spacing = 20;
            var horizontal_spacing = 20;

            // layout all the elements for search
            _searchTextBox.Height = 30;
            _searchTextBox.Width = Width - 2 * horizontal_spacing;
            _searchTextBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            //layout all the elements for the list view
            vertical_spacing += 20 + (int)_searchTextBox.Height;
            var immutable_checkbox_height = 40;

            _metadata_listview.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _metadata_listview.Width = Width - 2 * horizontal_spacing;
            _metadata_listview.Height = Height - 20 - vertical_spacing - immutable_checkbox_height - 20;

            // layout the show immutable checkbox
            vertical_spacing += 20 + (int)_metadata_listview.Height;

            _showImmutableCheckbox.Height = immutable_checkbox_height;
            _showImmutableCheckbox.Width = 150;
            _showImmutableCheckbox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
