using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using MyToolkit.Utilities;
using NusysIntermediate;
using Wintellect.PowerCollections;
using WinRTXamlToolkit.Tools;

namespace NuSysApp.Components.NuSysRenderer.UI.ListView
{
    class CollectionListViewUIElement
    {

        private CollectionRenderItem _collectionRenderItem;
        private NuSysApp.NuSysRenderer _renderEngine;


        public CollectionListViewUIElement(NuSysApp.NuSysRenderer renderEngine)
        {
            _renderEngine = renderEngine;
        }

        public ListViewUIElementContainer<LibraryElementModel> ConstructListViewUIElementContainer(CollectionRenderItem collectionRenderItem)
        {
            // Check parent later
            ListViewUIElementContainer<LibraryElementModel> Lib = new ListViewUIElementContainer<LibraryElementModel>(collectionRenderItem,
                _renderEngine.CanvasAnimatedControl);

            List<LibraryElementModel> libraryElementModelList = new List<LibraryElementModel>();

            _collectionRenderItem = collectionRenderItem;

            foreach (var child in _collectionRenderItem.ViewModel.GetOutputLibraryIds())
            {
                libraryElementModelList.Add(SessionController.Instance.ContentController.GetLibraryElementModel(child));
            }

            var imgColumn = new LibraryListImageColumn<LibraryElementModel>(_renderEngine.CanvasAnimatedControl);
            imgColumn.Title = "";
            imgColumn.RelativeWidth = 1;
            imgColumn.ColumnFunction = model => model.GetController().SmallIconUri;

            var listColumn1 = new ListTextColumn<LibraryElementModel>();
            listColumn1.Title = "Title";
            listColumn1.RelativeWidth = 2;
            listColumn1.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Type";
            listColumn2.RelativeWidth = 1.25f;
            listColumn2.ColumnFunction = model => model.Type.ToString();

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Creator";
            listColumn3.RelativeWidth = 1;
            listColumn3.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn4 = new ListTextColumn<LibraryElementModel>();
            listColumn4.Title = "Last Edited Timestamp";
            listColumn4.RelativeWidth = 1.8f;
            listColumn4.ColumnFunction = model => model.GetController()?.GetLastEditedTimestampInMinutes(); //Trims the seconds portion of the timestamp

            var listColumn5 = new ListTextColumn<LibraryElementModel>();
            listColumn5.Title = "Tags";
            listColumn5.RelativeWidth = 1f;
            listColumn5.ColumnFunction = model => model.Keywords != null ? string.Join(", ", model.Keywords.Select(i => i.Text)) : "";

            var listColumn6 = new ListTextColumn<LibraryElementModel>();
            listColumn6.Title = "Parent";
            listColumn6.RelativeWidth = 1f;
            listColumn6.ColumnFunction = model => SessionController.Instance.ContentController.GetLibraryElementController(model.ParentId) != null ? SessionController.Instance.ContentController.GetLibraryElementController(model.ParentId).Title : "";

            var listColumn7 = new ListTextColumn<LibraryElementModel>();
            listColumn7.Title = "Creation Date";
            listColumn7.RelativeWidth = 1f;
            listColumn7.ColumnFunction = model => model.GetController().GetCreationTimestampInMinutes();

            var listColumn8 = new ListTextColumn<LibraryElementModel>();
            listColumn8.Title = "Access";
            listColumn8.RelativeWidth = 1f;
            listColumn8.ColumnFunction = model => model.AccessType.ToString();

            Lib.AddColumns(new List<ListColumn<LibraryElementModel>> { imgColumn, listColumn1, listColumn2, listColumn3, listColumn4 });
            Lib.AddColumnOptions(new List<ListColumn<LibraryElementModel>> { listColumn5, listColumn8, listColumn7, listColumn6 });

            Lib.AddItems(libraryElementModelList);

            Lib.BorderWidth = 5;

            var width = _collectionRenderItem.ViewModel.Width;
            var height = _collectionRenderItem.ViewModel.Height;

            Debug.WriteLine("height: " + height);
            Debug.WriteLine("width: " + width);

            Lib.Width = (float)width;
            Lib.Height = (float)height;

            Lib.RowTapped += OnLibraryItemSelected;

            return Lib;
        }

        private void OnLibraryItemSelected(LibraryElementModel item, string columnName, CanvasPointer pointer,
            bool isSelected)
        {

            // first we just try to get the content data model for the element that was selected since that it is important for loading images
            if (!SessionController.Instance.ContentController.ContainsContentDataModel(item.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    if (item.Type == NusysConstants.ElementType.Collection)
                    {
                        var request = new GetEntireWorkspaceRequest(item.LibraryElementId);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                        Debug.Assert(request.WasSuccessful() == true);
                        await request.AddReturnedDataToSessionAsync();
                        await request.MakeCollectionFromReturnedElementsAsync();
                    }
                    else
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(
                            item.ContentDataModelId);
                    }
                });
            }
        }
    }
}
