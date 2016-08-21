using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Newtonsoft.Json;
using NusysIntermediate;
using WinRTXamlToolkit.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryView : UserControl
    {

        private LibraryFavorites _libraryFavorites;
        private LibraryList _libraryList;
        private LibraryGrid _libraryGrid;
        private FloatingMenuView _menu;
        private LibraryElementPropertiesWindow _propertiesWindow;
        private LibraryPageViewModel _pageViewModel;
        private LibraryFavoritesViewModel _favoritesViewModel;
        private Point2d _searchExportPos;

        //private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView(LibraryBucketViewModel vm, LibraryElementPropertiesWindow properties, FloatingMenuView menu)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            var data = SessionController.Instance.ContentController.ContentValues.Select(item => SessionController.Instance.ContentController.GetLibraryElementController(item.LibraryElementId));
            _pageViewModel = new LibraryPageViewModel(new ObservableCollection<LibraryElementController>(data));
            _favoritesViewModel = new LibraryFavoritesViewModel(new ObservableCollection<LibraryElementController>(data));
            this.MakeViews(_pageViewModel, properties);
            _propertiesWindow = properties;
            properties.AddedToFavorite += AddToFavorites;
            this._libraryList.DeleteClicked += DeleteClicked;
            ListContainer.Children.Add(_libraryList);
            Searchfield.SetHeight = 34;
            _menu = menu;

            vm.OnElementDeleted += delegate
            {
                UITask.Run(delegate
                {
                    properties.Visibility = Visibility.Collapsed;
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

        //Trent, this needs to be filled in in order for the importing to the library to work.
        private async void AddFile()
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            NusysConstants.ElementType elementType = NusysConstants.ElementType.Text;
            string data = string.Empty;
            string title = string.Empty;
            // a list of strings containing pdf text for each page
            List<string> pdfTextByPage = new List<string>();
            int pdfPageCount = 0;

            var storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);

            // get the fileAddedAclsPopup from the session view
            var fileAddedAclsPopup = SessionController.Instance.SessionView.FileAddedAclsPopup;
            // get a mapping of the acls for all of the storage files using the fileAddedAclsPopup
            var fileIdToAccessMap = await fileAddedAclsPopup.GetAcls(storageFiles);

            // check if the user has canceled the upload
            if (fileIdToAccessMap == null)
            {
                return;
            }

            foreach (var storageFile in storageFiles ?? new List<StorageFile>())
            {
                if (storageFile == null) return;

                var contentId = SessionController.Instance.GenerateId();
                string serverURL = null;

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
                    serverURL = contentId + fileType;
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
                    //add the three thumbnails
                    args.LibraryElementArgs.Large_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Large];
                    args.LibraryElementArgs.Small_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Small];
                    args.LibraryElementArgs.Medium_Thumbnail_Bytes = thumbnails[NusysConstants.ThumbnailSize.Medium];

                    args.LibraryElementArgs.Title = title;
                    args.LibraryElementArgs.LibraryElementType = elementType;

                    // add the acls from the map, default to private instead of throwing an error on release
                    Debug.Assert(fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId), "The mapping from the fileAddedPopup is not being output or set correctly");
                    if (fileIdToAccessMap.ContainsKey(storageFile.FolderRelativeId))
                    {
                        args.LibraryElementArgs.AccessType = fileIdToAccessMap[storageFile.FolderRelativeId];
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
            }
        }

        private Task Goto(int v)
        {
            throw new NotImplementedException();
        }

        private void ThumbnailTest(string b64)
        {
            //ShellFile file =new ShellFile();


        }

        public async Task AddNode(Point pos, Size size, NusysConstants.ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != NusysConstants.ElementType.Collection)
                {
                    var dict = new Message();
                    Dictionary<string, object> metadata;

                    metadata = new Dictionary<string, object>();
                    metadata["node_creation_date"] = DateTime.Now;
                    metadata["node_type"] = elementType + "Node";
                    dict["metadata"] = metadata;

                    var elementRequestArgs = new NewElementRequestArgs();
                    elementRequestArgs.ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
                    elementRequestArgs.LibraryElementId = libraryId;
                    elementRequestArgs.Width = size.Width;
                    elementRequestArgs.Height = size.Height;
                    elementRequestArgs.X = pos.X;
                    elementRequestArgs.Y = pos.Y;

                    var request = new NewElementRequest(elementRequestArgs);

                    var contentDataModelId = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId)?.ContentDataModelId;
                    Debug.Assert(contentDataModelId != null);

                    if (!SessionController.Instance.ContentController.ContainsContentDataModel(contentDataModelId))
                    {
                        await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(contentDataModelId);
                    }

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    await request.AddReturnedElementToSessionAsync();

                    //TODO, add undo button here 820
                }
                else
                {
                    var collection = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId) as CollectionLibraryElementModel;
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, collection.IsFinite,
                            new List<Point>(collection?.ShapePoints?.Select(p => new Point(p.X, p.Y)) ?? new List<Point>()), size.Width, size.Height);
                }
            });



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

            // Since we are adding a collection, we should make the dragging rectangle reflect this
            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(NusysConstants.ElementType.Collection);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);

            // Make the rectangle movable and set its position
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += _searchExportPos.X;
            t.TranslateY += _searchExportPos.Y;
            e.Handled = true;
        }

        /// <summary>
        /// Moves the library dragging rectangle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XSearchExportButton_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Obtain the library dragging rectangle  
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;

            // Update its transform
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            // Update the position instance variable
            _searchExportPos.X += e.Delta.Translation.X;
            _searchExportPos.Y += e.Delta.Translation.Y;

            // Handled!
            e.Handled = true;
        }

        /// <summary>
        /// Creates a collection based on the search results, and places it where the cursor was left off 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void XSearchExportButton_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Hide the library dragging rect
            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            rect.Hide();

            // Add a collection to the dropped location
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var dropPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(_searchExportPos);
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
    }
}
