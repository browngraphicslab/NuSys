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
using Windows.UI.Xaml.Automation.Peers;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class DetailViewMetadataPage : RectangleUIElement
    {

        private ScrollableTextboxUIElement _addKeyBox;

        private ScrollableTextboxUIElement _addValueBox;

        private ButtonUIElement _addKeyValueButton;

        private ScrollableTextboxUIElement _searchTextBox;

        private ListViewUIElementContainer<MetadataEntry> _metadata_listview;

        private CheckBoxUIElement _hideImmutableCheckbox;

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

        /// <summary>
        /// String on the button to add a new key value 
        /// </summary>
        private const string AddNewKeyValueText = "Add Key Value Pair";

        /// <summary>
        /// String on the button to edit a key value pair
        /// </summary>
        private const string EditKeyValueText = "Edit Key Value Pair";

        /// <summary>
        /// button used to delete a key
        /// </summary>
        private RectangleButtonUIElement _deleteKeyButton;

        public DetailViewMetadataPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _addKeyBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter a Key",
                Background = Colors.White,
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE
            };
            AddChild(_addKeyBox);

            _addValueBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter Values",
                Background = Colors.White,
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE
            };
            AddChild(_addValueBox);

            _addKeyValueButton = new RectangleButtonUIElement(this, Canvas, UIDefaults.SecondaryStyle, AddNewKeyValueText);
            AddChild(_addKeyValueButton);

            _deleteKeyButton = new RectangleButtonUIElement(this, Canvas, UIDefaults.SecondaryStyle, "Delete Key")
            {
                IsVisible = false
            };
            AddChild(_deleteKeyButton);

            _searchTextBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Background = Colors.White,
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE,
                PlaceHolderText = "Search"
            };
            AddChild(_searchTextBox);

            // create the list view to display the metadata
            CreateListView();

            _hideImmutableCheckbox = new CheckBoxUIElement(this, ResourceCreator, false)
            {
                LabelText = "Hide Immutable"
            };
            AddChild(_hideImmutableCheckbox);

            _suggestedTagElements = new List<DynamicTextboxUIElement>();

            _controller.MetadataChanged += _controller_MetadataChanged;
            _controller.TitleChanged += _controller_TitleChanged;
            _controller.AccessTypeChanged += ControllerOnAccessTypeChanged;
            _addKeyValueButton.Tapped += AddKeyValuePairToMetadata;
            _searchTextBox.TextChanged += OnSearchTextChanged;
            _hideImmutableCheckbox.Selected += OnShowImmutableSelectionChanged;
            _controller.KeywordsChanged += _controller_KeywordsChanged;
            _addKeyBox.TextChanged += _addKeyBox_TextChanged;
            _metadata_listview.RowDoubleTapped += _metadata_listview_RowDoubleTapped;
            _metadata_listview.RowDragged += MetadataListviewOnRowDragged;
            _deleteKeyButton.Tapped += _deleteKeyButton_Tapped;
        }

        private MetadataEntry _lastDragEntry;
        private void MetadataListviewOnRowDragged(MetadataEntry item, string columnName, CanvasPointer pointer)
        {
            RenderItemInteractionManager.SetDrag(DropFunc,  _controller.LargeIconUri);
            _lastDragEntry = item;
        }

        private bool DropFunc(CanvasPointer canvasPointer, BaseRenderItem baseRenderItem)
        {
            if (baseRenderItem is CollectionRenderItem)
            {
                Task.Run(async delegate
                {
                    var id = SessionController.Instance.GenerateId();
                    await StaticServerCalls.AddElementToCollection(canvasPointer.CurrentPoint,
                        NusysConstants.ElementType.Variable, null, baseRenderItem as CollectionRenderItem,id);

                    var controller = SessionController.Instance.ElementModelIdToElementController[id] as VariableElementController;
                    if (_lastDragEntry != null)
                    {
                        controller.SetMetadataKey(_lastDragEntry.Key);
                    }
                });
            }
            else if (baseRenderItem?.Parent is VariableElementRenderItem)
            {
                var controller = (baseRenderItem.Parent as VariableElementRenderItem).ViewModel.Controller as VariableElementController;
                controller.SetMetadataKey(_lastDragEntry.Key);
            }
            return false;
        }

        /// <summary>
        /// event handler called whenever the access type changed on the controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="accessType"></param>
        private void ControllerOnAccessTypeChanged(object sender, NusysConstants.AccessType accessType)
        {
            filterlist();
        }

        private void _deleteKeyButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _controller.RemoveMetadata(_addKeyBox.Text);
            _addKeyBox.ClearText();
            _addValueBox.ClearText();
        }

        /// <summary>
        /// Event called when a row in the metadata list is double tapped, fills in the proper fields of the textboxes with the info
        /// for that piece of metadata
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void _metadata_listview_RowDoubleTapped(MetadataEntry item, string columnName, CanvasPointer pointer)
        {
            if (item.Mutability == MetadataMutability.MUTABLE  || item.Key == "Keywords") 
            {
                _addKeyBox.ClearText();
                _addKeyBox.Text = item.Key;
            }
            else if (item.Key == "Search_Url" || item.Key == "Original_Url")
            {
                try
                {
                   // Windows.System.Launcher.LaunchUriAsync(new Uri(item.Values.First()));
                    SessionController.Instance.SessionView.FreeFormViewer.ShowWebPreview(item.Values.First(), pointer.CurrentPoint.X, pointer.CurrentPoint.Y);
                }
                catch (Exception e)
                {
                    //Todo alert the user?
                }
            }
        }

        /// <summary>
        /// Event claled whenever the key box text changes
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void _addKeyBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            // get all the metadata
            var metadata = _controller.GetMetadata().Values;

            // try to find an entry that has the same key as the text in the key box
            var foundEntry = metadata.FirstOrDefault(entry => entry.Key == text);

            // if we found an entry and the entry is mutable
            if (foundEntry != null && (foundEntry.Mutability == MetadataMutability.MUTABLE || foundEntry.Key == "Keywords"))
            {
                _addValueBox.ClearText();
                // set the value box to have the values for that key
                _addValueBox.Text = string.Join(", ", foundEntry.Values.Select(value => string.Join(", ", value)));
                _addKeyValueButton.ButtonText = EditKeyValueText;

                _deleteKeyButton.IsVisible = foundEntry.Key != "Keywords";
            }
            else
            {
                _addKeyValueButton.ButtonText = AddNewKeyValueText;
                _deleteKeyButton.IsVisible = false;
            }

        }

        private void _controller_TitleChanged(object sender, string e)
        {
            filterlist();
        }

        private void _controller_KeywordsChanged(object sender, HashSet<Keyword> keywords)
        {
            filterlist();
        }

        private void OnShowImmutableSelectionChanged(CheckBoxUIElement sender, bool hide_immutable)
        {
            filterlist();
        }

        /// <summary>
        /// call this method to filter the list
        /// </summary>
        private void filterlist()
        {
            _metadata_listview.ClearItems();
            var filtered_metadata = filter_by_mutability(new List<MetadataEntry>(GetAllMetadata()),
                _hideImmutableCheckbox.IsSelected);
            filtered_metadata = filter_by_search_text(filtered_metadata, _searchTextBox.Text);
            _metadata_listview.AddItems(filtered_metadata);
        }

        /// <summary>
        /// helper method which takes in a list of metadata entry to filter, and returns only thus that are immutable if
        /// show_immutable is true, all otherwise
        /// </summary>
        /// <param name="metadataToBeFiltered"></param>
        /// <param name="hide_immutable"></param>
        /// <returns></returns>
        private List<MetadataEntry> filter_by_mutability(List<MetadataEntry> metadataToBeFiltered, bool hide_immutable)
        {
            if (hide_immutable)
            {
                return new List<MetadataEntry>(metadataToBeFiltered.Where(entry => entry.Mutability == MetadataMutability.MUTABLE));
            }
            return metadataToBeFiltered;
        }

        /// <summary>
        /// helper method which filters by search text and returns only metadata whose key or values contains the search text
        /// </summary>
        /// <param name="metadataToBeFiltered"></param>
        /// <param name="searchText"></param>
        /// <returns></returns>
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
            _controller.TitleChanged -= _controller_TitleChanged;
            _controller.AccessTypeChanged -= ControllerOnAccessTypeChanged;
            _addKeyValueButton.Tapped -= AddKeyValuePairToMetadata;
            _searchTextBox.TextChanged -= OnSearchTextChanged;
            _hideImmutableCheckbox.Selected -= OnShowImmutableSelectionChanged;
            _controller.KeywordsChanged -= _controller_KeywordsChanged;
            _addKeyBox.TextChanged -= _addKeyBox_TextChanged;
            _metadata_listview.RowDoubleTapped -= _metadata_listview_RowDoubleTapped;
            _metadata_listview.RowDragged -= MetadataListviewOnRowDragged;
            _deleteKeyButton.Tapped -= _deleteKeyButton_Tapped;


            foreach (var element in _suggestedTagElements.ToArray())
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
                // we have to use a different process for updating keywords
                if (key == "Keywords")
                {
                    // get the current keywords
                    var current_keywords = _controller.GetMetadata("Keywords");

                    // get any keywords that were added
                    var new_keywords = values.Except(_controller.GetMetadata("Keywords"));
                    foreach (var keyword in new_keywords)
                    {
                        _controller.AddKeyword(new Keyword(keyword));
                    }

                    // get any keywords that were removed
                    var removed_keywords = current_keywords.Except(values);
                    foreach (var keyword in removed_keywords)
                    {
                        _controller.RemoveKeyword(new Keyword(keyword));
                    }
                }
                else
                {
                    _controller.UpdateMetadata(metaDataEntry, key, new List<string>(values));
                }

            }
            else // otherwise create a new metadata entry
            {
                _controller.AddMetadata(metaDataEntry);
            }

            _addValueBox.ClearText();
            _addKeyBox.ClearText();


        }

        private void _controller_MetadataChanged(object source)
        {
            filterlist();
        }

        private void CreateListView()
        {
            ICanvasResourceCreatorWithDpi resourceCreator;
            _metadata_listview = new ListViewUIElementContainer<MetadataEntry>(this, ResourceCreator)
            {
                Background = Colors.White,
                BorderWidth = 1,
                BorderColor = Constants.DARK_BLUE
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
            _metadata_listview.AddItems(new List<MetadataEntry>(GetAllMetadata()));
        }

        private IEnumerable<MetadataEntry> GetAllMetadata()
        {
            return _controller.GetMetadata().Values.Concat(
                new List<MetadataEntry>()
                {
                    new MetadataEntry("Icon", new List<string>(), MetadataMutability.IMMUTABLE)
                });
        }

        public override async Task Load()
        {
            _suggestedTags = await _controller.GetSuggestedTagsAsync(false);
            Debug.Assert(_suggestedTagElements.Count() < 250, "If this happens please get Trent.  Its not necessarily bad i just want to make sure its not returning crazy tags");
            var keywords = _controller.GetMetadata("Keywords");
            Debug.Assert(keywords != null);
            var tagsToRemove = _suggestedTags.Where(item => keywords.Contains(item.Key));
            foreach (var tag in tagsToRemove.ToArray())
            {
                _suggestedTags.Remove(tag.Key);
            }
            _rebuildSuggestedTags = true;
            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            // helper variable, the current vertical spacing from the top of the window
            var vertical_spacing = 20;
            var horizontal_spacing = 20;
            var addMetadataItemsHeight = 50;

            // layout all the elments to add a metadata key value pair
            var textboxWidth = (Width - 4* horizontal_spacing)/2;
            _addKeyBox.Height = addMetadataItemsHeight;
            _addKeyBox.Width = textboxWidth;
            _addKeyBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _addValueBox.Height = addMetadataItemsHeight;
            _addValueBox.Width = textboxWidth;
            _addValueBox.Transform.LocalPosition = new Vector2(2 * horizontal_spacing + _addKeyBox.Width, vertical_spacing);

            if(_deleteKeyButton.IsVisible)
            {
                _addKeyValueButton.Transform.LocalPosition = new Vector2(Width/2 - _addKeyValueButton.Width - 5, _addKeyBox.Transform.LocalY + _addKeyBox.Height + vertical_spacing);
                _deleteKeyButton.Transform.LocalPosition = new Vector2(Width / 2 + 5, _addKeyBox.Transform.LocalY + _addKeyBox.Height + vertical_spacing);
            }
            else
            {
                _addKeyValueButton.Transform.LocalPosition = new Vector2(Width / 2 - _addKeyValueButton.Width / 2, _addKeyBox.Transform.LocalY + _addKeyBox.Height + vertical_spacing);
            }

            // layout all the elements for search
            vertical_spacing += 40 + addMetadataItemsHeight + (int)_addKeyValueButton.Height;
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

            _hideImmutableCheckbox.Height = immutable_checkbox_height;
            _hideImmutableCheckbox.Width = 300;
            _hideImmutableCheckbox.LabelTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _hideImmutableCheckbox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            // layout the suggested tags box
            vertical_spacing += 20 + (int)_hideImmutableCheckbox.Height;

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
                    FontFamily = UIDefaults.TextFont
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
