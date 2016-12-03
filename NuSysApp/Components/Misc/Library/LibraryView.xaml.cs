using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NAudio.Wave;
using Newtonsoft.Json;
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class LibraryView : UserControl
    {
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


        private LibraryFavorites _libraryFavorites;
        private LibraryList _libraryList;
        private LibraryGrid _libraryGrid;
        private FloatingMenuView _menu;
        private LibraryElementPropertiesWindow _propertiesWindow;
        private LibraryPageViewModel _pageViewModel;
        private LibraryFavoritesViewModel _favoritesViewModel;
        private Point2d _searchExportPos;

        private Dictionary<string, NusysConstants.AccessType> _fileIdToAccessMap = new Dictionary<string, NusysConstants.AccessType>();



        //private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView(LibraryBucketViewModel vm, LibraryElementPropertiesWindow properties, FloatingMenuView menu)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            _propertiesWindow = properties;
            _menu = menu;

        }

        public void Init()
        {
            var data = SessionController.Instance.ContentController.ContentValues.Select(item => SessionController.Instance.ContentController.GetLibraryElementController(item.LibraryElementId));
            _pageViewModel = new LibraryPageViewModel(new ObservableCollection<LibraryElementController>(data));
            _favoritesViewModel = new LibraryFavoritesViewModel(new ObservableCollection<LibraryElementController>(data));
            this.MakeViews(_pageViewModel, _propertiesWindow);
            _propertiesWindow.AddedToFavorite += AddToFavorites;
            this._libraryList.DeleteClicked += DeleteClicked;
            ListContainer.Children.Add(_libraryList);
            Searchfield.SetHeight = 34;
           

            (DataContext as LibraryBucketViewModel).OnElementDeleted += delegate
            {
                UITask.Run(delegate
                {
                    _propertiesWindow.Visibility = Visibility.Collapsed;
                });
            };
            _searchExportPos = new Point2d(0, 0);
        }


        /// <summary>
        /// Will add the action reference to the undo button when delete is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="action"></param>
        private void DeleteClicked(object sender, IUndoable action)
        {
            xUndoButton.Activate(action);
        }

        public async void ToggleVisiblity()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (Visibility == Visibility.Collapsed)
            {
                _propertiesWindow.Visibility = Visibility.Collapsed;
            }
        }

        public void MakeViews(LibraryPageViewModel pageViewModel, LibraryElementPropertiesWindow properties)
        {
            _libraryList = new LibraryList(this, pageViewModel, properties);
            _libraryFavorites = new LibraryFavorites(this, _favoritesViewModel, properties);
        }


        private void TextBox_OnTextChanging(Object sender, String args)
        {
            if (ListContainer.Children[0] == _libraryList)
            {
                _libraryList.Search(((TextInputBlock)sender).Text.ToLower());
            }

            _propertiesWindow.Visibility = Visibility.Collapsed;
        }

        private void AddToFavorites(object sender, LibraryElementModel element)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(element.LibraryElementId);
            if (!element.Favorited)
            {
                controller?.SetFavorited(true);
            }

            else
            {
                controller?.SetFavorited(false);
                if (ListContainer.Children[0] == _libraryFavorites)
                    _propertiesWindow.Visibility = Visibility.Collapsed;
            }
        }



        private async void AddFile()
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            NusysConstants.ElementType elementType = NusysConstants.ElementType.Text;
            string data = string.Empty;
            string title = string.Empty;
            // a list of strings containing pdf text for each page
            List<string> pdfTextByPage = new List<string>();
            int pdfPageCount = 0;
            double aspectRatio = 0;

            var storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);

            // get the fileAddedAclsPopup from the session view
            var fileAddedAclsPopup = SessionController.Instance.SessionView.FileAddedAclsPopup;

            // get a mapping of the acls for all of the storage files using the fileAddedAclsPopup
            var tempfileIdToAccessMaps = await fileAddedAclsPopup.GetAcls(storageFiles);

            if (tempfileIdToAccessMaps == null) //if the user canceled the document import
            {
                return;
            }

            foreach (var fileAccess in tempfileIdToAccessMaps)
            {
                _fileIdToAccessMap.Add(fileAccess.Key, fileAccess.Value);
            }

            // check if the user has canceled the upload
            if (_fileIdToAccessMap == null)
            {
                return;
            }

            foreach (var storageFile in storageFiles ?? new List<StorageFile>())
            {
                if (storageFile == null) return;

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
                    aspectRatio = ((double)thumb.OriginalWidth)/((double)thumb.OriginalHeight);

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
                    }
                    else
                    {
                        args.LibraryElementArgs.AccessType = NusysConstants.AccessType.Private;
                    }

                    var request = new CreateNewContentRequest(args);

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

                    request.AddReturnedLibraryElementToLibrary();

                    //update listview so item is added to top of list
                    var listvm = (LibraryPageViewModel)_libraryList.DataContext;
                    
                    vm.ClearSelection();
                }
                else
                {
                    Debug.WriteLine("tried to import invalid filetype");
                }

                _fileIdToAccessMap.Remove(storageFile.FolderRelativeId);
            }
        }     

        private void Folder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.AddFile();
        }

        public FrameworkElement HeaderRow
        {
            get { return xHeaderRow; }
        }

        /// <summary>
        /// Exports the search results to a collection, and places the new collection at the passed in point
        /// @tdgreen, plz comment this, thx. -zkirsche 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task ExportSearchResultsToCollection(Point r)
        {
            var contentId = SessionController.Instance.GenerateId();
            var newCollectionId = SessionController.Instance.GenerateId();

            // We determine the access type of the tool generated collection based on the collection we're in and pass that in to the request
            NusysConstants.AccessType newCollectionAccessType;
            var currWorkSpaceAccessType = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;
            if (currWorkSpaceAccessType == NusysConstants.AccessType.Public)
            {
                newCollectionAccessType = NusysConstants.AccessType.Public;
            }
            else
            {
                newCollectionAccessType = NusysConstants.AccessType.Private;
            }

            // This creates a request to create the new collection of search results 
            var newContentRequestArgs = new CreateNewContentRequestArgs()
            {
                ContentId = contentId,
                LibraryElementArgs = new CreateNewLibraryElementRequestArgs()
                {
                    ContentId = contentId,
                    AccessType = newCollectionAccessType,
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    LibraryElementId = newCollectionId,
                    Title = "Search Results for '" + Searchfield.Text + "'"
                }
            };

            var newContentRequest = new CreateNewContentRequest(newContentRequestArgs);
            // This if checks whether this collection is being generated from search results or favorites. 
            // TODO Add functionality back in for favorites.
            if (ListContainer.Children[0] == _libraryList)
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(newContentRequest);
                newContentRequest.AddReturnedLibraryElementToLibrary();
            }
                
            //else if (ListContainer.Children[0] == _libraryFavorites)
            //    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, "", NusysConstants.ElementType.Collection, "Favorites"));

            // After creating the collection, we need to instantiate an instance of the collection and place it on the workspace. 
            var newElementRequestArgs = new NewElementRequestArgs()
            {
                Height = 300,
                Width = 300,
                X = r.X,
                Y = r.Y,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId,
                LibraryElementId = newCollectionId
            };
            var elementRequest = new NewElementRequest(newElementRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);
            await elementRequest.AddReturnedElementToSessionAsync();
            
            // We then populate this new collection with instances of the all the search results
            if (ListContainer.Children[0] == _libraryList) // if there are search results
            {
                foreach (var libraryItemTemplate in _pageViewModel.ItemList.ToList().GetRange(0, Math.Min(_pageViewModel.ItemList.Count, 30)))
                {
                    if (libraryItemTemplate.LibraryElementId == newCollectionId)
                    {
                        // Since the collection that was just created has a similar title, it gets added to the search results.
                        // We don't do anything on that iteration of this for each loop.
                    }
                    else
                    {
                        var elementRequestArgs = new NewElementRequestArgs()
                        {
                            Height = 300,
                            Width = 300,
                            X = 50000,
                            Y = 50000,
                            AccessType = newCollectionAccessType,
                            LibraryElementId = libraryItemTemplate.LibraryElementId,
                            ParentCollectionId = newCollectionId
                        };
                        var embeddedElementRequest = new NewElementRequest(elementRequestArgs);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(embeddedElementRequest);
                        embeddedElementRequest.AddReturnedElementToSession();
                    }
                    
                }
            }
        }

        /// <summary>
        /// Adds a library dragging rectangle to represent where the exported collection will be
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
          
            //// Since we are adding a collection, we should make the dragging rectangle reflect this
            //var view = SessionController.Instance.SessionView;
            //view.LibraryDraggingRectangle.SetIcon(NusysConstants.ElementType.Collection);
            //view.LibraryDraggingRectangle.Show();
            //var rect = view.LibraryDraggingRectangle;
            //Canvas.SetZIndex(rect, 3);

            //// Make the rectangle movable and set its position
            //rect.RenderTransform = new CompositeTransform();
            //var t = (CompositeTransform)rect.RenderTransform;
            //t.TranslateX += _searchExportPos.X;
            //t.TranslateY += _searchExportPos.Y;
            //e.Handled = true;
        }

        /// <summary>
        /// Moves the library dragging rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //// Obtain the library dragging rectangle  
            //var view = SessionController.Instance.SessionView;
            //var rect = view.LibraryDraggingRectangle;

            // Update its transform
            //var t = (CompositeTransform)rect.RenderTransform;
            //t.TranslateX += e.Delta.Translation.X;
            //t.TranslateY += e.Delta.Translation.Y;

            //// Update the position instance variable
            //_searchExportPos.X += e.Delta.Translation.X;
            //_searchExportPos.Y += e.Delta.Translation.Y;

            //// Handled!
            //e.Handled = true;
        }

        /// <summary>
        /// Creates a collection based on the search results, and places it where the cursor was left off 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void XSearchExportButton_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Hide the library dragging rect
            //var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            //rect.Hide();

            // Add a collection to the dropped location
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(new Vector2((float)_searchExportPos.X, (float)_searchExportPos.Y), SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);

            var dropPoint = new Point(r.X, r.Y);
            await ExportSearchResultsToCollection(dropPoint);
            e.Handled = true;
        }

        /// <summary>
        /// When the search export button is clicked, first find the position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _searchExportPos.X = e.GetCurrentPoint(view).Position.X - 25;
            _searchExportPos.Y = e.GetCurrentPoint(view).Position.Y - 25;
            e.Handled = true;
        }

        /// <summary>
        /// Converts Audio into a Framework element representing its waveform. The quality level defaults to low but
        /// can be set higher if desired.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="quality"></param>
        /// <returns></returns>
        private FrameworkElement GetWaveFormFrameWorkElement(Byte[] bytes, WaveFormQuality quality = WaveFormQuality.Low)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();

            WaveStream waveStream = new MediaFoundationReaderUniversal(stream); 
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample/8)*waveStream.WaveFormat.Channels;
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
                samplesPerPixel = (int) Math.Pow(2, 11);
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
            

            byte[] waveData = new byte[samplesPerPixel*bytesPerSample];
            var visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel*bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float) low) - short.MinValue)/ushort.MaxValue);
                float highPercent = ((((float) high) - short.MinValue)/ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100*(highPercent);
                line.Y2 = 100*(lowPercent);
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
            middleLine.Y1 = visualGrid.Height/2;
            middleLine.Y2 = visualGrid.Height/2;

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
        private async Task<Dictionary<NusysConstants.ThumbnailSize, string>> GetThumbnailsFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Canvas.SetTop(frameWorkElement, -frameWorkElement.Height*2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int) frameWorkElement.Width, (int) frameWorkElement.Height);

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
        private async Task<string> GetImageAsStringFromFrameworkElement(FrameworkElement frameWorkElement)
        {
            // add the ui element to the canvas out of sight
            Canvas.SetTop(frameWorkElement, -frameWorkElement.Height*2);
            SessionController.Instance.SessionView.MainCanvas.Children.Add(frameWorkElement);

            // render it
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(frameWorkElement, (int) frameWorkElement.Width, (int) frameWorkElement.Height);

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