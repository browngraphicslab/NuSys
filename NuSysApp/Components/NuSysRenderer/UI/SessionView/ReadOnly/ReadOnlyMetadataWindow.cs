using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class ReadOnlyMetadataWindow : ReadOnlyModeWindow
    {
        private ScrollableTextboxUIElement _searchTextBox;

        private ListViewUIElementContainer<MetadataEntry> _metadata_listview;

        private TextboxUIElement _label;

        private LibraryElementController _controller;
        public ReadOnlyMetadataWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _label = new TextboxUIElement(this, ResourceCreator);
            _label.Text = "metadata";
            _label.FontFamily = UIDefaults.TitleFont;
            _label.Width = Width;
            _label.Height = 38;
            _label.FontSize = 20;
            _label.TextColor = Constants.DARK_BLUE;
            _label.Background = Constants.LIGHT_BLUE;
            _label.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _label.IsHitTestVisible = false;

            AddChild(_label);

            _searchTextBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Background = Colors.White,
                BorderWidth = 1,
                BorderColor = Constants.LIGHT_BLUE,
                PlaceHolderText = "Search"
            };
            AddChild(_searchTextBox);


            _searchTextBox.TextChanged += OnSearchTextChanged;

        }

        public override Task Load()
        {
            CreateListView();

            return base.Load();
        }

        public void UpdateList(LibraryElementController controller)
        {
            if (_controller != null)
            {
                _controller.MetadataChanged -= _controller_MetadataChanged;
            }
            _controller = controller;
            _metadata_listview?.ClearItems();
            _metadata_listview?.ClearFilter();
            _metadata_listview?.AddItems(new List<MetadataEntry>(_controller.GetMetadata().Values));
            filterlist();

            _controller.MetadataChanged += _controller_MetadataChanged;
        }

        private void filterlist()
        {
            _metadata_listview?.ClearItems();
            var filtered_metadata = filter_by_search_text(new List<MetadataEntry>(_controller.GetMetadata().Values),
                _searchTextBox.Text);
            _metadata_listview?.AddItems(filtered_metadata);
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
                BorderWidth = 1,
                BorderColor = Constants.LIGHT_BLUE
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
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_metadata_listview == null)
            {
                return;
            }
            // helper variable, the current vertical spacing from the top of the window
            var vertical_margin = 0;
            var horizontal_margin = 0;
            var searchTextBoxHeight = 30;

            _label.Width = Width;
            _label.Transform.LocalPosition = new Vector2(0, vertical_margin);

            //layout all the elements for the list view
            _metadata_listview.Transform.LocalPosition = new Vector2(horizontal_margin, vertical_margin + _label.Height);
            _metadata_listview.Width = Width - 2*horizontal_margin;
            _metadata_listview.Height = Height - (2*vertical_margin + searchTextBoxHeight + _label.Height);

            // layout all the elements for search
            _searchTextBox.Height = 30;
            _searchTextBox.Width = Width - 2 * horizontal_margin;
            _searchTextBox.Transform.LocalPosition = new Vector2(horizontal_margin, vertical_margin + _metadata_listview.Height + _label.Height);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
