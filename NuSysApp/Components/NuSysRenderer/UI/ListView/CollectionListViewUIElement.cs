using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using MyToolkit.Mathematics;
using MyToolkit.Utilities;
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using NuSysApp;
using WinRTXamlToolkit.Imaging;
using Wintellect.PowerCollections;
using WinRTXamlToolkit.Tools;

namespace NuSysApp.Components.NuSysRenderer.UI.ListView
{

    public class CollectionListViewUIElement : RectangleUIElement
    {

        private CollectionRenderItem _collectionRenderItem;
        private CanvasAnimatedControl _resourceCreator;
        private ListViewUIElementContainer<LibraryElementModel> Lib;
        /// <summary>
        /// list of RectangleUIElements which are used to display icons while dragging
        /// </summary>
        private List<RectangleUIElement> _libraryDragElements;
        /// <summary>
        /// True if the drag icons are visible false otherwise
        /// </summary>
        private bool _isDragVisible;

        /// <summary>
        /// how much each of the dragged icons and dropped library elements will be offset from eachother in postive x and positive y pixel coordinates
        /// </summary>
        private float _itemDropOffset = 10;

        private bool _dragCanceled;

        /// <summary>
        /// A dictionary of fileids to access types, static because the adding files methods have to be static
        /// </summary>
        private static Dictionary<string, NusysConstants.AccessType> _fileIdToAccessMap = new Dictionary<string, NusysConstants.AccessType>();


        /// <summary>
        /// ienumerable of selected controllers
        /// </summary>
        private IEnumerable<LibraryElementController> _previouslySelectedControllers;

        public CollectionListViewUIElement(CollectionRenderItem parent, CanvasAnimatedControl resourceCreator) : base(parent, resourceCreator)
        {
            _collectionRenderItem = parent;
            // initialize the ui of the library listview
            InitializeLibraryList();
            // add the libary list view as a child
            AddChild(Lib);
            //TopBarColor = Constants.LIGHT_BLUE;
            //TopBarHeight = 50;
            Background = Colors.White;
            BorderColor = Constants.MED_BLUE;
            BorderWidth = 1;
            //IsSnappable = true;

            _libraryDragElements = new List<RectangleUIElement>();

            // add dragging events
            Lib.RowDragged += LibraryListView_RowDragged;
            Lib.RowDragCompleted += LibraryListView_RowDragCompleted;
            Lib.RowTapped += OnLibraryItemSelected;
            Lib.RowDoubleTapped += LibraryListView_RowDoubleTapped;

            _dragCanceled = false;

            _previouslySelectedControllers = new List<LibraryElementController>();
        }

        /// <summary>
        /// Fired whenever a row is selected, causes the session controller to fetch the content data model for that row
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void OnLibraryItemSelected(LibraryElementModel item, string columnName, CanvasPointer pointer, bool isSelected)
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
                        SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(item.ContentDataModelId);
                    }
                });
            }

            // then we get the list of controllers which are currently selected in the library
            var currentlySelectedControllers = Lib.GetSelectedItems().Select(lem => SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId));

            // if there are any controllers, then we turn off highlighting //todo why!
            if (currentlySelectedControllers.Any())
            {
                BrushManager.SetBrushVisibility(false);
            }

            // then we determine which controllers have been deselected by querying all previously selected controllers except
            // the currently selected controllers. we remove the highlight from these deselected controllers
            var deselectedControllers = _previouslySelectedControllers.Except(currentlySelectedControllers);
            foreach (var controller in deselectedControllers)
            {
                controller?.RemoveHighlight();
            }

            // then we add a highlight to the currently selected controllers,
            //todo AS OF NOW, ADDING HIGHLIGHT MULTIPLE TIMES IS SAFE BUT THIS COULD BE DANGEROUS
            foreach (var controller in currentlySelectedControllers)
            {
                controller?.AddHighlight();
            }

            // now we set the previously selected controllers to the list of controllers which are currently selected
            _previouslySelectedControllers = new List<LibraryElementController>(currentlySelectedControllers);

            //todo what is this doing here. 
            if (!currentlySelectedControllers.Any())
            {
                BrushManager.SetBrushVisibility(true);
            }

        }


        private void LibraryListView_RowDoubleTapped(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(item.LibraryElementId);
            Debug.Assert(controller != null);
            if (controller == null)
            {
                return;
            }
            SessionController.Instance.NuSessionView.ShowDetailView(controller);

        }


        /// <summary>
        /// Fired when the drag event is completed, removes any drag icons from the display, and adds each of the dragged elements to the current collection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private async void LibraryListView_RowDragCompleted(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            if (_dragCanceled)
            {
                _dragCanceled = false;
                return;
            }
            // remove each of the drag elements
            foreach (var rect in _libraryDragElements)
            {
                RemoveChild(rect);
            }
            _libraryDragElements.Clear();
            _isDragVisible = false;

            // add each of the items to the collection
            foreach (var lem in Lib.GetSelectedItems().ToArray())
            {
                var libraryElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);
                await
                    StaticServerCalls.AddElementToWorkSpace(pointer.CurrentPoint,
                            libraryElementController.LibraryElementModel.Type, libraryElementController)
                        .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Fired when a row is dragged from
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void LibraryListView_RowDragged(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            if (_dragCanceled)
            {
                return;
            }
            // if we are currently dragging
            if (_isDragVisible)
            {
                // simply move each of the element sto the new drag location
                var position = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix) + pointer.Delta;

                //If we are on the listview, "put the elements back"
                if (Lib.HitTest(pointer.CurrentPoint) != null)
                {
                    // remove each of the drag elements
                    foreach (var rect in _libraryDragElements)
                    {
                        RemoveChild(rect);
                    }
                    _libraryDragElements.Clear();
                    _isDragVisible = false;
                    _dragCanceled = true;
                }
                else
                {
                    //Otherwise move each of the library drag elements
                    foreach (var element in _libraryDragElements)
                    {
                        element.Transform.LocalPosition = position + new Vector2(_itemDropOffset * _libraryDragElements.IndexOf(element));
                    }
                }

            }
            else
            {
                // set drag visible to true so future calls of this event do not reach this control flow branch
                _isDragVisible = true;

                // get the current position of the pointer relative to the local matrix
                var position = pointer.StartPoint;
                // convert the list of selected library element models from the libraryListView into a list of controllers
                var selectedControllers =
                    Lib.GetSelectedItems()
                        .Select(
                            model =>
                                SessionController.Instance.ContentController.GetLibraryElementController(
                                    model.LibraryElementId))
                        .ToList();

                foreach (var controller in selectedControllers)
                {
                    var rect = new RectangleUIElement(this, ResourceCreator);
                    Task.Run(async delegate
                    {
                        rect.Image = await LoadCanvasBitmap(controller.SmallIconUri);
                        Debug.Assert(rect.Image is CanvasBitmap);
                        rect.Width = (float)(rect.Image as CanvasBitmap).SizeInPixels.Width / (rect.Image as CanvasBitmap).SizeInPixels.Height * 100;
                        rect.Height = 100;


                    });
                    rect.Transform.LocalPosition = position + new Vector2(_itemDropOffset * selectedControllers.IndexOf(controller));
                    _libraryDragElements.Add(rect);
                    position += new Vector2(_itemDropOffset, _itemDropOffset);
                    AddChild(rect);
                }
            }
        }

        private async Task<ICanvasImage> LoadCanvasBitmap(Uri smallIconURI)
        {
            return await MediaUtil.LoadCanvasBitmapAsync(Canvas, smallIconURI);
        }

        public override void Dispose()
        {
            Lib.RowDragged -= LibraryListView_RowDragged;
            Lib.RowDragCompleted -= LibraryListView_RowDragCompleted;
            Lib.RowTapped -= OnLibraryItemSelected;
            Lib.RowDoubleTapped -= LibraryListView_RowDoubleTapped;

            base.Dispose();
        }

        /// <summary>
        /// Initialize the UI for the library list 
        /// </summary>
        public void InitializeLibraryList()
        {
            Lib = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                MultipleSelections = false

            };

            var imgColumn = new LibraryListImageColumn<LibraryElementModel>(Canvas);
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

            List<LibraryElementModel> items = new List<LibraryElementModel>();

            foreach (var child in _collectionRenderItem.ViewModel.GetOutputLibraryIds())
            {
                items.Add(SessionController.Instance.ContentController.GetLibraryElementModel(child));
            }

            Lib.AddItems(items);

            BorderWidth = 5;
            BorderColor = Colors.Black;

            //Width = 500;
            //Height = 10000;
            //MinWidth = 500;
            //MinHeight = 400;


        }

        private void UpdateContents()
        {
            var items = new List<LibraryElementModel>();
            bool itemAdded = false;
            foreach (var child in _collectionRenderItem.ViewModel.GetOutputLibraryIds())
            {
                var item = SessionController.Instance.ContentController.GetLibraryElementModel(child);
                if (!Lib.GetItems().Contains(item))
                {
                    items.Add(item);
                    itemAdded = true;
                }
            }

            Lib.AddItems(items);
            //if(itemAdded) updateEventHandlers();
        }

        private void updateEventHandlers()
        {
            Lib.RowDragged -= LibraryListView_RowDragged;
            Lib.RowDragCompleted -= LibraryListView_RowDragCompleted;
            Lib.RowTapped -= OnLibraryItemSelected;
            Lib.RowDoubleTapped -= LibraryListView_RowDoubleTapped;
            Lib.RowDragged += LibraryListView_RowDragged;
            Lib.RowDragCompleted += LibraryListView_RowDragCompleted;
            Lib.RowTapped += OnLibraryItemSelected;
            Lib.RowDoubleTapped += LibraryListView_RowDoubleTapped;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            Lib.Width = Width - 2 * BorderWidth;
            Lib.Height = Height - BorderWidth;
            Lib.Transform.LocalPosition = new Vector2(BorderWidth, 0);
            
            UpdateContents();

            base.Update(parentLocalToScreenTransform);
        }
        

        /// <summary>
        /// Converts Audio into a Framework element representing its waveform. The quality level defaults to low but
        /// can be set higher if desired.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        private static FrameworkElement GetWaveFormFrameWorkElement(Byte[] bytes, LibraryView.WaveFormQuality quality = LibraryView.WaveFormQuality.Low)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();

            WaveStream waveStream = new MediaFoundationReaderUniversal(stream);
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
            waveStream.Position = 0;
            int bytesRead = 1;
            int samplesPerPixel = 1024;

            if (waveStream.TotalTime.TotalMinutes > 15)
            {
                samplesPerPixel = (int)Math.Pow(2, 15);
            }
            else if (waveStream.TotalTime.TotalMinutes > 8)
            {
                samplesPerPixel = (int)Math.Pow(2, 14);
            }
            else if (waveStream.TotalTime.TotalMinutes > 5)
            {
                samplesPerPixel = (int)Math.Pow(2, 13);
            }
            else if (waveStream.TotalTime.TotalMinutes > 3)
            {
                samplesPerPixel = (int)Math.Pow(2, 12);
            }
            else if (waveStream.TotalTime.TotalMinutes > 0.5)
            {
                samplesPerPixel = (int)Math.Pow(2, 11);
            }

            // change the quality of the waveform by affecting the number of samples used
            switch (quality)
            {
                case LibraryView.WaveFormQuality.Low:
                    break;
                case LibraryView.WaveFormQuality.Medium:
                    samplesPerPixel /= 2;
                    break;
                case LibraryView.WaveFormQuality.High:
                    samplesPerPixel /= 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quality), quality, "The quality level is not supported here");
            }


            byte[] waveData = new byte[samplesPerPixel * bytesPerSample];
            var visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel * bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100 * (highPercent);
                line.Y2 = 100 * (lowPercent);
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 1;
                x++;
                visualGrid.Children.Add(line);
            }
            visualGrid.Height = 100;
            visualGrid.Width = x;
            Line middleLine = new Line();
            middleLine.X1 = 0;
            middleLine.X2 = x;
            middleLine.Y1 = visualGrid.Height / 2;
            middleLine.Y2 = visualGrid.Height / 2;

            middleLine.Stroke = new SolidColorBrush(Colors.Crimson);
            middleLine.StrokeThickness = 1;
            visualGrid.Children.Add(middleLine);

            return visualGrid;
        }

        /// <summary>
        /// Returns the thumbnails from a Framework element
        /// </summary>
        /// <param name="frameWorkElement"></param>
        /// <returns></returns>
        private static async Task<Dictionary<NusysConstants.ThumbnailSize, string>> GetThumbnailsFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Windows.UI.Xaml.Controls.Canvas.SetTop(frameWorkElement, -frameWorkElement.Height * 2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int)frameWorkElement.Width, (int)frameWorkElement.Height);

            // remove the visual grid from the canvas
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(frameWorkElement);

            // create a buffer from the rendered bitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // create a WriteableBitmap with desired width and height
            var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);

            // write the pixels to the bitmap
            using (Stream bitmapStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(pixels, 0, pixels.Length);
            }

            // save the writeable bitmap to a file
            var tempFile = await writeableBitmap.SaveAsync(ApplicationData.Current.LocalCacheFolder);

            // get the thumbnails from the image file
            var thumbnails = await MediaUtil.GetThumbnailDictionary(tempFile);

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            return thumbnails;
        }

        /// <summary>
        /// Pass in a framework element and render a 1 : 1 pixel representation image of it.
        /// Returns the image as a string for use in JSON
        /// </summary>
        /// <param name="frameWorkElement"></param>
        /// <returns>A string representation of the passed in framework element as an image</returns>
        private static async Task<string> GetImageAsStringFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Windows.UI.Xaml.Controls.Canvas.SetTop(frameWorkElement, -frameWorkElement.Height * 2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int)frameWorkElement.Width, (int)frameWorkElement.Height);

            // remove the visual grid from the canvas
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(frameWorkElement);

            // create a buffer from the rendered bitmap
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();

            // create a WriteableBitmap with desired width and height
            var writeableBitmap = new WriteableBitmap(renderTargetBitmap.PixelWidth, renderTargetBitmap.PixelHeight);

            // write the pixels to the bitmap
            using (Stream bitmapStream = writeableBitmap.PixelBuffer.AsStream())
            {
                await bitmapStream.WriteAsync(pixels, 0, pixels.Length);
            }

            // save the writeable bitmap to a file
            var tempFile = await writeableBitmap.SaveAsync(ApplicationData.Current.LocalCacheFolder);

            // use the system to convert the file to a string
            var imageAsString = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(tempFile));

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            // return the string representation of the image
            return imageAsString;
        }
    }
}


