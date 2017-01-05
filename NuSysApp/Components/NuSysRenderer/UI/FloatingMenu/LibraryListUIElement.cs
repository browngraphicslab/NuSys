﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI.BaseUIElements;
using WinRTXamlToolkit.Imaging;
using Buffer = Windows.Storage.Streams.Buffer;

namespace NuSysApp
{
    public class LibraryListUIElement : ResizeableWindowUIElement
    {

        /// <summary>
        /// The actual list view used to display the library
        /// </summary>
        public ListViewUIElementContainer<LibraryElementModel> LibraryListView;

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

        /// <summary>
        /// The add file button on the top right corner of the library list
        /// </summary>
        private ButtonUIElement _addFileButton;

        /// <summary>
        /// A dictionary of fileids to access types, static because the adding files methods have to be static
        /// </summary>
        private static Dictionary<string, NusysConstants.AccessType> _fileIdToAccessMap = new Dictionary<string, NusysConstants.AccessType>();

        /// <summary>
        /// Define the quality of the waveform
        /// </summary>
        public enum WaveFormQuality
        {
            /// <summary>
            /// Lowest Quality, somewhat blurry on one region level
            /// </summary>
            Low,
            /// <summary>
            /// Medium Quality, somewhat blurry on two region levels
            /// </summary>
            Medium,
            /// <summary>
            /// High Quality, somewhat blurry on three region levels
            /// </summary>
            High
        }

        /// <summary>
        /// Search bar for the LibraryListUIElement
        /// </summary>
        private ScrollableTextboxUIElement _searchBar;

        /// <summary>
        /// The height of the searchbar
        /// </summary>
        private float _searchBarHeight = 25;

        /// <summary>
        /// Filter button for activating the filter menu
        /// </summary>
        private ButtonUIElement _filterButton;

        /// <summary>
        /// the width of the filter button
        /// </summary>

        private float _filterButtonWidth = 50;

        /// <summary>
        /// the menu used for filtering library elements
        /// </summary>
        public FilterMenu FilterMenu { get; }

        public LibraryListUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {
            // initialize the ui of the library listview
            InitializeLibraryList();
            // add the libary list view as a child
            AddChild(LibraryListView);

            // set up the ui of the add file button
            _addFileButton = new RectangleButtonUIElement(this, ResourceCreator, UIDefaults.PrimaryStyle);
            // set the image bounds for the addfile button
            _addFileButton.ImageBounds = new Rect(_addFileButton.BorderWidth, _addFileButton.BorderWidth, _addFileButton.Width - 2 * BorderWidth, _addFileButton.Height - 2 * BorderWidth);
            // add the addfile button to the window
            AddButton(_addFileButton, TopBarPosition.Right);

            // initialize the search bar
            _searchBar = new ScrollableTextboxUIElement(this, Canvas,false,true)
            {
                Height = _searchBarHeight,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                FontSize = 14,
                BorderWidth = 3,
                Bordercolor = Colors.Gray
            };
            _searchBar.TextChanged += SearchBarTextChanged;
            AddChild(_searchBar);

            // initialize the filter button
            _filterButton = new RectangleButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "Filter")
            {
                Width = _filterButtonWidth,
                Height = _searchBarHeight,
            };
            AddChild(_filterButton);

            FilterMenu = new FilterMenu(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(FilterMenu);
            

            // initialize the list of library drag elements
            _libraryDragElements = new List<RectangleUIElement>();

            // add the add file button tapped event
            _addFileButton.Tapped += AddFileButtonTapped;

            // add dragging events
            LibraryListView.RowDragged += LibraryListView_RowDragged;
            LibraryListView.RowDragCompleted += LibraryListView_RowDragCompleted;
            LibraryListView.RowTapped += OnLibraryItemSelected;

            _filterButton.Tapped += OnFilterButtonTapped;

            // events so that the library list view adds and removes elements dynamically
            SessionController.Instance.ContentController.OnNewLibraryElement += UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete += UpdateLibraryListToRemoveElement;
        }

        /// <summary>
        /// Event handler for when the text of the library search bar changes
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void SearchBarTextChanged(InteractiveBaseRenderItem item, string text)
        {
            //throw new NotImplementedException();
        }

        private void OnFilterButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            FilterMenu.IsVisible = !FilterMenu.IsVisible;
            FilterMenu.Height = 400;
            FilterMenu.Width = 200;
        }

        /// <summary>
        /// Fired whenever a row is selected, causes the session controller to fetch the content data model for that row
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void OnLibraryItemSelected(LibraryElementModel item, string columnName, CanvasPointer pointer, bool isSelected)
        {
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
        }

        /// <summary>
        /// Loads any async resources we need
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _addFileButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/add from file dark.png"));
            base.Load(); 
        }

        /// <summary>
        /// Called when the addfile button is tapped, triggers the adding file sequence of events
        /// </summary>
        /// <param name="interactiveBaseRenderItem"></param>
        /// <param name="pointer"></param>
        private void AddFileButtonTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                AddFile();
            });

        }

        /// <summary>
        /// Fired when the drag event is completed, removes any drag icons from the display, and adds each of the dragged elements to the current collection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void LibraryListView_RowDragCompleted(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            // remove each of the drag elements
            foreach (var rect in _libraryDragElements.ToArray())
            {
                rect.Dispose();
                RemoveChild(rect);
                _libraryDragElements.Remove(rect);
            }
            _isDragVisible = false;

            // add each of the items to the collection
            foreach (var lem in LibraryListView.GetSelectedItems())
            {
                var libraryElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);

                StaticServerCalls.AddElementToCurrentCollection(pointer.CurrentPoint, libraryElementController.LibraryElementModel.Type, libraryElementController);
            }
        }

        /// <summary>
        /// Fired when a row is dragged from
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private async void LibraryListView_RowDragged(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            // if we are currently dragging
            if (_isDragVisible)
            {
                // simply move each of the element sto the new drag location
                var position = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix) + pointer.Delta;
                foreach (var element in _libraryDragElements)
                {
                    element.Transform.LocalPosition = position + new Vector2(_itemDropOffset * _libraryDragElements.IndexOf(element));
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
                    LibraryListView.GetSelectedItems()
                        .Select(
                            model =>
                                SessionController.Instance.ContentController.GetLibraryElementController(
                                    model.LibraryElementId))
                        .ToList();

                // add each of the controllers smalliconurls as drag icons
                foreach (var controller in selectedControllers)
                {
                    var rect = new RectangleUIElement(this, ResourceCreator);
                    rect.Image = await CanvasBitmap.LoadAsync(Canvas, controller.SmallIconUri);
                    rect.Transform.LocalPosition = position + new Vector2(_itemDropOffset * selectedControllers.IndexOf(controller));
                    _libraryDragElements.Add(rect);
                    position += new Vector2(_itemDropOffset, _itemDropOffset);
                    AddChild(rect);

                }
            }
        }

        public override void Dispose()
        {
            LibraryListView.RowDragged -= LibraryListView_RowDragged;
            LibraryListView.RowDragCompleted -= LibraryListView_RowDragCompleted;
            LibraryListView.RowTapped -= OnLibraryItemSelected;

            _filterButton.Tapped -= OnFilterButtonTapped;


            SessionController.Instance.ContentController.OnNewLibraryElement -= UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete -= UpdateLibraryListToRemoveElement;
            _addFileButton.Tapped -= AddFileButtonTapped;

            _searchBar.TextChanged -= SearchBarTextChanged;
            base.Dispose();
        }

        /// <summary>
        /// Initialize the UI for the library list 
        /// </summary>
        public void InitializeLibraryList()
        {
            LibraryListView = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                MultipleSelections = false
            };

            var listColumn = new ListTextColumn<LibraryElementModel>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Creator";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Last Edited Timestamp";
            listColumn3.RelativeWidth = 3;
            listColumn3.ColumnFunction = model => model.LastEditedTimestamp;

            LibraryListView.AddColumns(new List<ListColumn<LibraryElementModel>> { listColumn, listColumn2, listColumn3 });


            LibraryListView.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());

            BorderWidth = 5;
            Bordercolor = Colors.Black;
            TopBarColor = Colors.Azure;
            Height = 400;
            Width = 400;
            MinWidth = 400;
            MinHeight = 400;


        }

        /// <summary>
        /// Removes an element from the libary list when it is deleted
        /// </summary>
        /// <param name="element"></param>
        private void UpdateLibraryListToRemoveElement(LibraryElementModel element)
        {
            LibraryListView.RemoveItems(new List<LibraryElementModel> {element});
        }

        /// <summary>
        /// Adds an element to the library list when it is added to the library
        /// </summary>
        /// <param name="libraryElement"></param>
        private void UpdateLibraryListWithNewElement(LibraryElementModel libraryElement)
        {
            LibraryListView.AddItems(new List<LibraryElementModel> {libraryElement});
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // make the library fill the resizeable window leaving room for the search bar and filter button
            LibraryListView.Width = Width - 2 * BorderWidth;
            LibraryListView.Height = Height - TopBarHeight - BorderWidth - _searchBarHeight;
            LibraryListView.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);
            _searchBar.Width = Width - 2*BorderWidth - _filterButtonWidth;
            _searchBar.Transform.LocalPosition = new Vector2(BorderWidth, Height - BorderWidth - _searchBarHeight);
            _filterButton.Transform.LocalPosition = new Vector2(BorderWidth + _searchBar.Width, Height - BorderWidth - _searchBarHeight);
            FilterMenu.Transform.LocalPosition = new Vector2(Width, 0);

            base.Update(parentLocalToScreenTransform);
        }

        /// <summary>
        /// Returns a list of library element model ids for the files which were just added to the library
        /// </summary>
        /// <param name="storageFiles"></param>
        /// <returns></returns>
        public static async Task<List<LibraryElementController>> AddFile(IReadOnlyList<StorageFile> storageFiles = null)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            NusysConstants.ElementType elementType = NusysConstants.ElementType.Text;
            string data = string.Empty;
            string title = string.Empty;
            // a list of strings containing pdf text for each page
            List<string> pdfTextByPage = new List<string>();
            int pdfPageCount = 0;
            double aspectRatio = 0;

            var addedLibraryElemControllers = new List<LibraryElementController>();

            if (storageFiles == null)
            {
                storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);
            }

            // get the fileAddedAclsPopup from the session view
            var fileAddedAclsPopup = SessionController.Instance.SessionView.FileAddedAclsPopup;

            // get a mapping of the acls for all of the storage files using the fileAddedAclsPopup
            var tempfileIdToAccessMaps = await fileAddedAclsPopup.GetAcls(storageFiles);

            if (tempfileIdToAccessMaps == null) //if the user canceled the document import
            {
                return addedLibraryElemControllers;
            }

            foreach (var fileAccess in tempfileIdToAccessMaps)
            {
                _fileIdToAccessMap.Add(fileAccess.Key, fileAccess.Value);
            }

            // check if the user has canceled the upload
            if (_fileIdToAccessMap == null)
            {
                return addedLibraryElemControllers;
            }

            foreach (var storageFile in storageFiles ?? new List<StorageFile>())
            {
                if (storageFile == null)
                    return addedLibraryElemControllers;

                var contentId = SessionController.Instance.GenerateId();

                var fileType = storageFile.FileType.ToLower();
                title = storageFile.DisplayName;

                bool validFileType = true;
                // Create a thumbnail dictionary mapping thumbnail sizes to the byte arrays.
                // Note that only video and images are to get thumbnails this way, currently.
                var thumbnails = new Dictionary<NusysConstants.ThumbnailSize, string>();
                thumbnails[NusysConstants.ThumbnailSize.Small] = string.Empty;
                thumbnails[NusysConstants.ThumbnailSize.Medium] = string.Empty;
                thumbnails[NusysConstants.ThumbnailSize.Large] = string.Empty;

                if (Constants.ImageFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Image;
                    data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));

                    var thumb = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
                    aspectRatio = thumb.OriginalWidth / (double)thumb.OriginalHeight;

                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.WordFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Word;

                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    data = Convert.ToBase64String(fileBytes);
                }
                else if (Constants.PowerpointFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Powerpoint;
                }
                else if (Constants.PdfFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.PDF;

                    // create a list of strings which contain the image data for each page of the pdf
                    List<string> pdfPages = new List<string>();

                    // read the contents of storageFile into a MUPDF Document
                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    data = Convert.ToBase64String(fileBytes);
                    var MuPdfDoc = await MediaUtil.DataToPDF(data);

                    pdfPageCount = MuPdfDoc.PageCount;

                    // get variables for drawing the page
                    var pageSize = MuPdfDoc.GetPageSize(0);
                    var width = pageSize.X;
                    var height = pageSize.Y;

                    // create an image to use for converting
                    var image = new WriteableBitmap(width, height);

                    // create a buffer to draw the page on
                    IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                    buf.Length = image.PixelBuffer.Length;

                    // draw the page onto the buffer
                    MuPdfDoc.DrawPage(0, buf, 0, 0, width, height, false);
                    var ss = buf.AsStream();

                    // copy the buffer to the image
                    await ss.CopyToAsync(image.PixelBuffer.AsStream());
                    image.Invalidate();

                    // save the image as a file (temporarily)
                    var x = await image.SaveAsync(NuSysStorages.SaveFolder);

                    thumbnails = await MediaUtil.GetThumbnailDictionary(x);

                    // delete the image file that we saved
                    await x.DeleteAsync(StorageDeleteOption.PermanentDelete);

                }
                else if (Constants.VideoFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Video;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes = null;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }

                    var thumb = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
                    aspectRatio = thumb.OriginalWidth / (double)thumb.OriginalHeight;

                    data = Convert.ToBase64String(fileBytes);
                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.AudioFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Audio;
                    IRandomAccessStream s = await storageFile.OpenReadAsync();

                    byte[] fileBytes;
                    using (IRandomAccessStreamWithContentType stream = await storageFile.OpenReadAsync())
                    {
                        fileBytes = new byte[stream.Size];
                        using (DataReader reader = new DataReader(stream))
                        {
                            await reader.LoadAsync((uint)stream.Size);
                            reader.ReadBytes(fileBytes);
                        }
                    }
                    var frameWorkWaveForm = GetWaveFormFrameWorkElement(fileBytes);
                    thumbnails = await GetThumbnailsFromFrameworkElement(frameWorkWaveForm);

                    // override the largest thumbnail for higher resolution
                    thumbnails[NusysConstants.ThumbnailSize.Large] =
                        await GetImageAsStringFromFrameworkElement(frameWorkWaveForm);

                    data = Convert.ToBase64String(fileBytes);
                }
                else
                {
                    validFileType = false;
                }
                if (validFileType)
                {
                    CreateNewContentRequestArgs args;
                    //if there is pdf text, add it to the request
                    if (pdfTextByPage.Any())
                    {
                        args = new CreateNewPdfContentRequestArgs
                        {
                            PdfText = JsonConvert.SerializeObject(pdfTextByPage),
                            PageCount = pdfPageCount
                        };
                        pdfTextByPage.Clear();
                    }
                    else
                    {
                        args = new CreateNewContentRequestArgs();
                    }
                    args.ContentId = contentId;
                    args.DataBytes = data;

                    //add the extension if there is one
                    if (fileType != null)
                    {
                        args.FileExtension = fileType;
                    }

                    CreateNewLibraryElementRequestArgs libraryElementArgs;
                    switch (elementType)
                    {
                        case NusysConstants.ElementType.Image:
                            var imageArgs = new CreateNewImageLibraryElementRequestArgs();
                            imageArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = imageArgs;
                            break;
                        case NusysConstants.ElementType.Word:
                            var wordArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                            wordArgs.PdfPageStart = 0;
                            wordArgs.PdfPageEnd = pdfPageCount;
                            libraryElementArgs = wordArgs;
                            break;
                        case NusysConstants.ElementType.PDF:
                            var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                            pdfArgs.PdfPageStart = 0;
                            pdfArgs.PdfPageEnd = pdfPageCount;
                            pdfArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = pdfArgs;
                            break;
                        case NusysConstants.ElementType.Video:
                            var videoArgs = new CreateNewVideoLibraryElementRequestArgs();
                            videoArgs.AspectRatio = aspectRatio;
                            libraryElementArgs = videoArgs;
                            break;
                        case NusysConstants.ElementType.Audio:
                            libraryElementArgs = new CreateNewAudioLibraryElementRequestArgs();
                            break;
                        default:
                            libraryElementArgs = new CreateNewLibraryElementRequestArgs();
                            break;
                    }

                    args.LibraryElementArgs = libraryElementArgs;

                    //add the three thumbnails
                    args.LibraryElementArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                    args.LibraryElementArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                    args.LibraryElementArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];

                    args.LibraryElementArgs.Title = title;
                    args.LibraryElementArgs.LibraryElementType = elementType;

                    // add the acls from the map, default to private instead of throwing an error on release
                    Debug.Assert(_fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId), "The mapping from the fileAddedPopup is not being output or set correctly");
                    if (_fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId))
                    {
                        args.LibraryElementArgs.AccessType = _fileIdToAccessMap[storageFile.FolderRelativeId];
                        args.LibraryElementArgs.LibraryElementId = SessionController.Instance.GenerateId();
                    }
                    else
                    {
                        args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Private;
                    }

                    var request = new CreateNewContentRequest(args);

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

                    // if we succesfully complete the request
                    if (request.AddReturnedLibraryElementToLibrary())
                    {
                        // add the library element controller of the new content to the list we are going to return
                        addedLibraryElemControllers.Add(SessionController.Instance.ContentController.GetLibraryElementController(args.LibraryElementArgs.LibraryElementId));
                    }
                    

                    vm.ClearSelection();
                }
                else
                {
                    Debug.WriteLine("tried to import invalid filetype");
                }

                _fileIdToAccessMap.Remove(storageFile.FolderRelativeId);
            }

            return addedLibraryElemControllers;
        }

        /// <summary>
        /// Converts Audio into a Framework element representing its waveform. The quality level defaults to low but
        /// can be set higher if desired.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        private static FrameworkElement GetWaveFormFrameWorkElement(Byte[] bytes, WaveFormQuality quality = WaveFormQuality.Low)
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
                case WaveFormQuality.Low:
                    break;
                case WaveFormQuality.Medium:
                    samplesPerPixel /= 2;
                    break;
                case WaveFormQuality.High:
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
            var tempFile = await writeableBitmap.SaveAsync(NuSysStorages.SaveFolder);

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
            var tempFile = await writeableBitmap.SaveAsync(NuSysStorages.SaveFolder);

            // use the system to convert the file to a string
            var imageAsString = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(tempFile));

            // delete the writeable bitmap file that we saved
            await tempFile.DeleteAsync(StorageDeleteOption.PermanentDelete);

            // return the string representation of the image
            return imageAsString;
        }


    }
}