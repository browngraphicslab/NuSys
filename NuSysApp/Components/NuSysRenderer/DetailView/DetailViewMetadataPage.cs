using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewMetadataPage : RectangleUIElement
    {

        private ScrollableTextboxUIElement _addKeyBox;

        private ScrollableTextboxUIElement _addValueBox;

        private ButtonUIElement _addKeyValueButton;

        private ScrollableTextboxUIElement _searchTextBox;

        private ListViewUIElementContainer<MetadataEntry> _metadata_listview;

        private CheckBoxUIElement _showImmutableCheckbox;

        private LibraryElementController _controller;

        /// <summary>
        /// The suggested tags for the controller
        /// </summary>
        private Dictionary<string, int> _suggestedTags;

        /// <summary>
        /// List of currently displayed suggested tags
        /// </summary>
        private List<DynamicTextboxUIElement> _suggestedTagElements;

        /// <summary>
        /// true if we want to rebuild the suggested tags
        /// </summary>
        private bool _rebuildSuggestedTags;

        /// <summary>
        /// The vertical spacing of the tag from its original position
        /// </summary>
        private float _tagVerticalSpacing;

        /// <summary>
        /// The width of the suggested tags box, don't set this except in build suggested tags
        /// used to figure out when we should rebuild suggested tags
        /// </summary>
        private double _suggestedTagsWidth;

        private float _tagHorizontalSpacing;

        public DetailViewMetadataPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _addKeyBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter a Key",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_addKeyBox);

            _addValueBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter Values",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_addValueBox);

            _addKeyValueButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas));
            AddChild(_addKeyValueButton);

            _searchTextBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Background = Colors.Azure,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                PlaceHolderText = "Search"
            };
            AddChild(_searchTextBox);

            // create the list view to display the metadata
            CreateListView();

            _showImmutableCheckbox = new CheckBoxUIElement(this, ResourceCreator, false)
            {
                LabelText = "Show Immutable"
            };
            AddChild(_showImmutableCheckbox);

            _suggestedTagElements = new List<DynamicTextboxUIElement>();

            _controller.MetadataChanged += _controller_MetadataChanged;
            _addKeyValueButton.Tapped += AddKeyValuePairToMetadata;
            _searchTextBox.TextChanged += OnSearchTextChanged;
            _showImmutableCheckbox.Selected += OnShowImmutableSelectionChanged;
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
            _addKeyValueButton.Tapped -= AddKeyValuePairToMetadata;
            _searchTextBox.TextChanged -= OnSearchTextChanged;
            _showImmutableCheckbox.Selected -= OnShowImmutableSelectionChanged;

            foreach (var element in _suggestedTagElements)
            {
                element.Tapped -= NewTag_Tapped;
                RemoveChild(element);
            }

            base.Dispose();
        }

        /// <summary>
        /// Adds a key value pair to metadata, called when the button is pressed to add a key value pair ot metadata
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddKeyValuePairToMetadata(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // get the key of the metadata to be updated
            var key = _addKeyBox.Text;

            // get the value of the metadata to be updated
            var values = new List<string>(_addValueBox.Text.Split(new[] {", ", ","}, StringSplitOptions.RemoveEmptyEntries));

            // get the metadata entry of the key to be updated if the key exists, otherwise create a new metadata entry
            var metaDataEntry = _controller.GetMetadata().ContainsKey(key)
                ? _controller.GetMetadata()[key]
                : new MetadataEntry(key, values, MetadataMutability.MUTABLE);


            // if the key already exists update the metadata entry
            if (_controller.GetMetadata().ContainsKey(key))
            {
                _controller.UpdateMetadata(metaDataEntry, key, new List<string>(metaDataEntry.Values.Concat(values)) );
            }
            else // otherwise create a new metadata entry
            {
                _controller.AddMetadata(metaDataEntry);
            }

            _addValueBox.Text = string.Empty;
            _addKeyBox.Text = string.Empty;


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

            _metadata_listview.AddColumns(new List<ListColumn<MetadataEntry>> {listColumn, listColumn2});
            _metadata_listview.AddItems(new List<MetadataEntry>(_controller.GetMetadata().Values));
        }

        public override async Task Load()
        {
            _addKeyValueButton.Image =
                    await
                        CanvasBitmap.LoadAsync(ResourceCreator, new Uri("ms-appx:///Assets/icon_metadata_plus.png"),
                            ResourceCreator.Dpi);


            _suggestedTags = await _controller.GetSuggestedTagsAsync(false);
            _rebuildSuggestedTags = true;
            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            // helper variable, the current vertical spacing from the top of the window
            var vertical_spacing = 20;
            var horizontal_spacing = 20;

            // layout all the elments to add a metadata key value pair
            var addMetadataItemsHeight = 50;
            _addKeyValueButton.Width = 50;
            _addKeyValueButton.Height = addMetadataItemsHeight;
            _addKeyValueButton.Transform.LocalPosition = new Vector2(Width - _addKeyValueButton.Width - horizontal_spacing, vertical_spacing);

            var textboxWidth = (Width - 4* horizontal_spacing - _addKeyValueButton.Width)/2;
            _addKeyBox.Height = addMetadataItemsHeight;
            _addKeyBox.Width = textboxWidth;
            _addKeyBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _addValueBox.Height = addMetadataItemsHeight;
            _addValueBox.Width = textboxWidth;
            _addValueBox.Transform.LocalPosition = new Vector2(2 * horizontal_spacing + _addKeyBox.Width, vertical_spacing);

            // layout all the elements for search
            vertical_spacing += 20 + addMetadataItemsHeight;
            _searchTextBox.Height = 30;
            _searchTextBox.Width = Width - 2*horizontal_spacing;
            _searchTextBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            //layout all the elements for the list view
            vertical_spacing += 20 + (int) _searchTextBox.Height;
            var immutable_checkbox_height = 40;
            var suggested_tags_box_height = 40;

            _metadata_listview.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _metadata_listview.Width = Width - 2*horizontal_spacing;
            _metadata_listview.Height = Height - 20 - vertical_spacing - immutable_checkbox_height -20 - suggested_tags_box_height - 20;

            // layout the show immutable checkbox
            vertical_spacing += 20 + (int)_metadata_listview.Height;

            _showImmutableCheckbox.Height = immutable_checkbox_height;
            _showImmutableCheckbox.Width = 150;
            _showImmutableCheckbox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            // layout the suggested tags box
            vertical_spacing += 20 + (int)_showImmutableCheckbox.Height;

            BuildSuggestedTags(suggested_tags_box_height, Width - 2*horizontal_spacing,
                new Vector2(horizontal_spacing, vertical_spacing));

            foreach (var tagElement in _suggestedTagElements)
            {
                tagElement.Transform.LocalPosition = new Vector2(tagElement.Transform.LocalX, vertical_spacing + _tagVerticalSpacing);
            }

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// builds the suggested tags box
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="position"></param>
        private void BuildSuggestedTags(int height, float width, Vector2 position)
        {

            if (_suggestedTags != null &&
                (_rebuildSuggestedTags || (Math.Abs(width - _suggestedTagsWidth) > _tagHorizontalSpacing/2)))
            {
            }
            else
            {
                return;
            }

            _suggestedTagsWidth = Width;

            foreach (var element in _suggestedTagElements)
            {
                element.Tapped -= NewTag_Tapped;
                RemoveChild(element);
            }

            var currXOffset = 5f;
            _tagHorizontalSpacing = 5f;
            _tagVerticalSpacing = height*.25f/2;

            foreach (var suggestedTag in _suggestedTags.OrderByDescending(pair => pair.Value))
            {
                var newTag = new DynamicTextboxUIElement(this, Canvas)
                {
                    Background = Constants.color4,
                    TextColor = Constants.color6,
                    FontSize = 15f,
                    FontFamily = "/Assets/fonts/Frutiger LT 56 Italic.ttf#Frutiger LT 55 Roman"
                };
                newTag.SetLoaded();
                newTag.Height = height-2*_tagVerticalSpacing;
                newTag.Text = suggestedTag.Key;
                if (currXOffset + _tagHorizontalSpacing + newTag.Width > width)
                {
                    break;
                }

                newTag.Transform.LocalPosition = new Vector2(position.X + currXOffset, position.Y + _tagVerticalSpacing);
                AddChild(newTag);
                _suggestedTagElements.Add(newTag);
                currXOffset+= _tagHorizontalSpacing + newTag.Width;
                newTag.Tapped += NewTag_Tapped;
            }

            _rebuildSuggestedTags = false;
        }

        private void NewTag_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var dynamicTextbox = item as DynamicTextboxUIElement;
            Debug.Assert(dynamicTextbox != null);

            _suggestedTags.Remove(dynamicTextbox.Text);

            _controller.AddKeyword(new Keyword(dynamicTextbox.Text, Keyword.KeywordSource.TagExtraction));
            _rebuildSuggestedTags = true;

        }
    }
}
