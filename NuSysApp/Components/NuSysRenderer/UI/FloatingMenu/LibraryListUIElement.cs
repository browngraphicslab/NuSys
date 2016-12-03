using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;

namespace NuSysApp
{
    public class LibraryListUIElement : ResizeableWindowUIElement
    {
        public ListViewUIElementContainer<LibraryElementModel> libraryListView;

        private List<RectangleUIElement> _libraryDragElements;

        private bool _isDragVisible;

        private float _itemDropOffset = 10;

        private ButtonUIElement _addFileButton;

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
            /// Medium Quality, somewhat blurry on three region levels
            /// </summary>
            High
        }

        public LibraryListUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {

            InitializeLibraryList();
            AddChild(libraryListView);

            _addFileButton = new ButtonUIElement(this, ResourceCreator, new RectangleUIElement(this, Canvas))
            {
                BorderWidth = 3,
                SelectedBorder = Colors.LightGray,
                Background = TopBarColor,
                Bordercolor = TopBarColor
            };
            AddButton(_addFileButton, TopBarPosition.Right);
            _addFileButton.Tapped += AddFileButtonTapped;

            _libraryDragElements = new List<RectangleUIElement>();

            libraryListView.RowDragged += LibraryListView_RowDragged;
            libraryListView.RowDragCompleted += LibraryListView_RowDragCompleted;
            libraryListView.RowTapped += OnLibraryItemSelected;

            // events so that the library list view adds and removes elements dynamically
            SessionController.Instance.ContentController.OnNewLibraryElement += UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete += UpdateLibraryListToRemoveElement;
        }

        /// <summary>
        /// Fired whenever a row is selected, causes the session controller to fetch the content data model for that row
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void OnLibraryItemSelected(LibraryElementModel item, string columnName, CanvasPointer pointer)
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

        public override async Task Load()
        {
            _addFileButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/add from file dark.png"));
            base.Load(); 
        }

        private void AddFileButtonTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            UITask.Run(() =>
            {
                AddFile();
            });

        }

        private void LibraryListView_RowDragCompleted(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {

            foreach (var rect in _libraryDragElements.ToArray())
            {
                rect.Dispose();
                RemoveChild(rect);
                _libraryDragElements.Remove(rect);
            }
            _isDragVisible = false;

            // convert the current point of the drag event to a point on the collection
            var collectionPoint = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(
                                                            pointer.CurrentPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);

            foreach (var lem in libraryListView.GetSelectedItems())
            {
                //Before we add the node, we need to check if the access settings for the library element and the workspace are incompatible
                // If they are different we simply return 
                var currWorkSpaceAccessType =
                    SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;
                var currWorkSpaceLibraryElementId =
                    SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;

                // if the item is private and the workspace is public or the item is the current workspace then continue
                if ((lem.AccessType == NusysConstants.AccessType.Private &&
                    currWorkSpaceAccessType == NusysConstants.AccessType.Public) || lem.LibraryElementId == currWorkSpaceLibraryElementId)
                {
                    continue;
                }

                // otherwise add the item to the workspace at the current point
                var libraryElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);
                libraryElementController.AddElementAtPosition(collectionPoint.X, collectionPoint.Y);

                // increment the collectionPoint by itemDropOffset
                collectionPoint += new Vector2(_itemDropOffset, _itemDropOffset);
              
            }
        }

        private async void LibraryListView_RowDragged(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            // if we are currently dragging
            if (_isDragVisible)
            {
                var position = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix) + pointer.Delta;
                foreach (var element in _libraryDragElements)
                {
                    element.Transform.LocalPosition = position + new Vector2(_itemDropOffset * _libraryDragElements.IndexOf(element));
                }

            }
            else
            {
                // get the current position of the pointer relative to the local matrix
                var position = pointer.StartPoint;
                // convert the list of selected library element models from the libraryListView into a list of controllers
                var selectedControllers =
                    libraryListView.GetSelectedItems()
                        .Select(
                            model =>
                                SessionController.Instance.ContentController.GetLibraryElementController(
                                    model.LibraryElementId))
                        .ToList();
                _isDragVisible = true;
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
            libraryListView.RowDragged -= LibraryListView_RowDragged;
            libraryListView.RowDragCompleted -= LibraryListView_RowDragCompleted;
            libraryListView.RowTapped -= OnLibraryItemSelected;

            SessionController.Instance.ContentController.OnNewLibraryElement -= UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete -= UpdateLibraryListToRemoveElement;
            _addFileButton.Tapped -= AddFileButtonTapped;
            base.Dispose();
        }

        public void InitializeLibraryList()
        {
            libraryListView = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                MultipleSelections = true
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

            libraryListView.AddColumns(new List<ListColumn<LibraryElementModel>> { listColumn, listColumn2, listColumn3 });


            libraryListView.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());

            BorderWidth = 5;
            Bordercolor = Colors.Black;
            TopBarColor = Colors.Azure;
            Height = 400;
            Width = 400;
            MinWidth = 400;
            MinHeight = 400;


        }

        private void UpdateLibraryListToRemoveElement(LibraryElementModel element)
        {
            libraryListView.RemoveItems(new List<LibraryElementModel> {element});
        }

        private void UpdateLibraryListWithNewElement(LibraryElementModel libraryElement)
        {
            libraryListView.AddItems(new List<LibraryElementModel> {libraryElement});
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            libraryListView.Width = Width - 2 * BorderWidth;
            libraryListView.Height = Height - TopBarHeight - BorderWidth;
            libraryListView.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);

            _addFileButton.ImageBounds = new Rect(_addFileButton.BorderWidth, _addFileButton.BorderWidth, _addFileButton.Width - 2*BorderWidth, _addFileButton.Height-2*BorderWidth);
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
                    aspectRatio = ((double)thumb.OriginalWidth) / ((double)thumb.OriginalHeight);

                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.WordFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Word;

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
                    var MuPdfDoc = await MediaUtil.DataToPDF(Convert.ToBase64String(fileBytes));

                    pdfPageCount = MuPdfDoc.PageCount;

                    // convert each page of the pdf into an image file, and store it in the pdfPages list
                    for (int pageNumber = 0; pageNumber < MuPdfDoc.PageCount; pageNumber++)
                    {
                        // set the pdf text by page for the current page number
                        pdfTextByPage.Add(MuPdfDoc.GetAllTexts(pageNumber));

                        // get variables for drawing the page
                        var pageSize = MuPdfDoc.GetPageSize(pageNumber);
                        var width = pageSize.X;
                        var height = pageSize.Y;

                        // create an image to use for converting
                        var image = new WriteableBitmap(width, height);

                        // create a buffer to draw the page on
                        IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                        buf.Length = image.PixelBuffer.Length;

                        // draw the page onto the buffer
                        MuPdfDoc.DrawPage(pageNumber, buf, 0, 0, width, height, false);
                        var ss = buf.AsStream();

                        // copy the buffer to the image
                        await ss.CopyToAsync(image.PixelBuffer.AsStream());
                        image.Invalidate();

                        // save the image as a file (temporarily)
                        var x = await image.SaveAsync(NuSysStorages.SaveFolder);

                        // use the system to convert the file to a byte array
                        pdfPages.Add(Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(x)));
                        if (pageNumber == 0)
                        {
                            // if we are on the first apge, get thumbnails of the file from the system
                            thumbnails = await MediaUtil.GetThumbnailDictionary(x);
                        }

                        // delete the image file that we saved
                        await x.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    }

                    data = JsonConvert.SerializeObject(pdfPages);
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
                    aspectRatio = ((double)thumb.OriginalWidth) / ((double)thumb.OriginalHeight);

                    data = Convert.ToBase64String(fileBytes);
                    thumbnails = await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.AudioFileTypes.Contains(fileType))
                {
                    elementType = NusysConstants.ElementType.Audio;
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
                        args = new CreateNewPdfContentRequestArgs()
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