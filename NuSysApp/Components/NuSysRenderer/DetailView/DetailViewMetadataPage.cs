using System;
using System.Collections.Concurrent;
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
    public class DetailViewMetadataPage : RectangleUIElement
    {

        private PlaceHolderTextBox _addKeyBox;

        private PlaceHolderTextBox _addValueBox;

        private ButtonUIElement _addKeyValueButton;

        private PlaceHolderTextBox _searchTextBox;

        private ListViewUIElementContainer<MetadataEntry> _metadata_listview;

        private LibraryElementController _controller;

        public DetailViewMetadataPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _addKeyBox = new PlaceHolderTextBox(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter a Key",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_addKeyBox);

            _addValueBox = new PlaceHolderTextBox(this, Canvas, false, false)
            {
                PlaceHolderText = "Enter Values",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_addValueBox);

            _addKeyValueButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas));
            AddChild(_addKeyValueButton);

            _searchTextBox = new PlaceHolderTextBox(this, Canvas, false, false)
            {
                Background = Colors.Azure,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                PlaceHolderText = "Search"
            };
            AddChild(_searchTextBox);

            // create the list view to display the metadata
            CreateListView();

            _controller.MetadataChanged += _controller_MetadataChanged;
            _addKeyValueButton.Tapped += AddKeyValuePairToMetadata;
        }

        /// <summary>
        /// Adds a key value pair to metadata
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
                : new MetadataEntry(key, new List<string>(), MetadataMutability.MUTABLE);

            // if the key already exists update the metadata entry
            if (_controller.GetMetadata().ContainsKey(key))
            {
                var updateMetadataEntryRequestArgs = new UpdateMetadataEntryRequestArgs()
                {
                    Entry = metaDataEntry,
                    LibraryElementId = _controller.LibraryElementModel.LibraryElementId,
                    NewValues = values
                };

                UpdateMetadataEntryRequest request = new UpdateMetadataEntryRequest(updateMetadataEntryRequestArgs);
                request.ExecuteRequestFunction();

            }
            else // otherwise create a new metadata entry
            {
                var createNewMetadataRequestArgs = new CreateNewMetadataRequestArgs()
                {
                    Entry = metaDataEntry,
                    LibraryElementId = _controller.LibraryElementModel.LibraryElementId
                };

                CreateNewMetadataRequest request = new CreateNewMetadataRequest(createNewMetadataRequestArgs);
                request.ExecuteRequestFunction();
            }


        }

        private void _controller_MetadataChanged(object source)
        {
            _metadata_listview.ClearItems();
            _metadata_listview.AddItems(new List<MetadataEntry>(_controller.LibraryElementModel.Metadata.Values));
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

            _metadata_listview.AddItems(new List<MetadataEntry>(_controller.LibraryElementModel.Metadata.Values));
        }

        public override async Task Load()
        {
            _addKeyValueButton.Image =
                    await
                        CanvasBitmap.LoadAsync(ResourceCreator, new Uri("ms-appx:///Assets/icon_metadata_plus.png"),
                            ResourceCreator.Dpi);
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
            _metadata_listview.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _metadata_listview.Width = Width - 2*horizontal_spacing;
            _metadata_listview.Height = Height - 20 - vertical_spacing;







            base.Update(parentLocalToScreenTransform);
        }
    }
}
