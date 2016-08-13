using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NAudio.MediaFoundation;
using NusysIntermediate;
using SharpDX.Direct2D1;
using SharpDX.WIC;
using WinRTXamlToolkit.Imaging;
using Image = Windows.UI.Xaml.Controls.Image;
using SolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LibraryView : UserControl
    {
        //public delegate void NewContentsEventHandler(ICollection<LibraryElement> elements);
        //public event NewContentsEventHandler OnNewContents;
        //private LibraryGrid workspaceGrid;
        //private LibraryList workspaceList;

        //public delegate void NewElementAvailableEventHandler(LibraryElement element);
        //public event NewElementAvailableEventHandler OnNewElementAvailable;
        private LibraryFavorites _libraryFavorites;
        private LibraryList _libraryList;
        private LibraryGrid _libraryGrid;
        private FloatingMenuView _menu;
        private double _graphButtonX;
        private double _graphButtonY;
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
            ListContainer.Children.Add(_libraryList);
            Searchfield.SetHeight = 34;
            _menu = menu;
            this.updateTabs();

            vm.OnElementDeleted += delegate
            {
                UITask.Run(delegate
                {
                    properties.Visibility = Visibility.Collapsed;
                });
            };
            _searchExportPos = new Point2d(0,0);
        }

        public async void ToggleVisiblity()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            if (Visibility == Visibility.Collapsed)
            {
                _propertiesWindow.Visibility = Visibility.Collapsed;
            }
        }
        //public async Task InitializeLibrary()
        //{
        //    Task.Run(async delegate
        //    {
        //        var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
        //        foreach (var kvp in dictionaries)
        //        {
        //            var id = kvp.Value["id"];
        //            var element = new LibraryElement(kvp.Value);
        //            if (!_elements.ContainsKey(id))
        //            {
        //                _elements.Add(id, element);
        //            }
        //        }
        //        UITask.Run(delegate {
        //            OnNewContents?.Invoke(_elements.ContentValues);
        //        });
        //    })
        //}

        //public void AddNewElement(LibraryElement element)
        //{
        //    _elements.Add(element.ContentID, element);
        //    OnNewElementAvailable?.Invoke(element);
        //}
        public void MakeViews(LibraryPageViewModel pageViewModel, LibraryElementPropertiesWindow properties)
        {
            //_libraryGrid = new LibraryGrid(this, pageViewModel, properties);
            _libraryList = new LibraryList(this, pageViewModel, properties);
            _libraryFavorites = new LibraryFavorites(this, _favoritesViewModel, properties);
            //_libraryList.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).ListViewBase_OnDragItemsStarting;
            //_libraryGrid.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).GridViewDragStarting;
        }


        private void TextBox_OnTextChanging(Object sender, String args)
        {
            if (ListContainer.Children[0] == _libraryList)
            {
                _libraryList.Search(((TextInputBlock)sender).Text.ToLower());
            }

            _propertiesWindow.Visibility = Visibility.Collapsed;
        }


        //public async void UpdateList()
        //{
        //    _libraryList.Update();
        //}

        private async void GridButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //await this.AddNode(new Point(12, 0), new Size(12, 12), ElementType.Document);
            //this.UpdateList();
            //if (WorkspacePivot.Content != _libraryGrid)
            //{
            //    await _libraryGrid.Update();
            //    WorkspacePivot.Content = _libraryGrid;
            //}
        }


        //private void GridViewDragStarting(object sender, DragStartingEventArgs e)
        //{
        //    //e.Data.Properties.
        //}
        //private void ListViewBase_OnDragItemsStarting(object sender, DragItemsStartingEventArgs e)
        //{
        //    List<LibraryElement> elements = new List<LibraryElement>();
        //    foreach (var element in e.Items)
        //    {
        //        var id = ((LibraryElement)element).ContentID;
        //        elements.Add((LibraryElement)element);
        //        if (SessionController.Instance.ContentController.Get(id) == null)
        //        {
        //            Task.Run(async delegate
        //            {
        //                SessionController.Instance.NuSysNetworkSession.FetchLibraryElementDataAsync(id);
        //            });
        //        }
        //    }
        //    e.Data.OperationCompleted += DataOnOperationCompleted;
        //    e.Data.Properties.Add("LibraryElements", elements);
        //    var title = ((LibraryElement)e.Items[0]).Title ?? "";
        //    var type = ((LibraryElement)e.Items[0]).NodeType.ToString();
        //    e.Data.SetText(type + "  :  " + title);
        //    e.Cancel = false;
        //}
        //private void DataOnOperationCompleted(DataPackage sender, OperationCompletedEventArgs args)
        //{
        //    UITask.Run(delegate
        //    {
        //        var ids = (List<LibraryElement>)sender.Properties["LibraryElements"];

        //        var width = SessionController.Instance.SessionView.ActualWidth;
        //        var height = SessionController.Instance.SessionView.ActualHeight;
        //        var centerpoint =
        //            SessionController.Instance.ActiveWorkspace.CompositeTransform.Inverse.TransformPoint(
        //                new Point(width / 2, height / 2));
        //        Task.Run(delegate
        //        {
        //            foreach (var element in ids)
        //            {
        //                Message m = new Message();
        //                m["contentId"] = element.ContentID;
        //                m["x"] = centerpoint.X - 200;
        //                m["y"] = centerpoint.Y - 200;
        //                m["width"] = 400;
        //                m["height"] = 400;
        //                m["nodeType"] = element.NodeType.ToString();
        //                m["autoCreate"] = true;
        //                m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.ContentId };

        //                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
        //            }
        //        });
        //    });
        //}



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
            string data = "";
            string title = "";
            string pdf_text = "";

            var storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);
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

                    // read the contents of the pdf from storageFile into data
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

                    // Get the MUPDF Document from data
                    var myDoc = await MediaUtil.DataToPDF(data); 
                    
                    // get the text from the MUPDF document into pdf_text
                    int numPages = myDoc.PageCount;
                    int currPage = 0;
                    while (currPage < numPages)
                    {
                        pdf_text = pdf_text + myDoc.GetAllTexts(currPage);
                        currPage++;
                    }

                    // draw the first page of the pdf on a writeable bitmap called image
                    var pageSize = myDoc.GetPageSize(0);
                    var width = pageSize.X;
                    var height = pageSize.Y;
                    var image = new WriteableBitmap(width, height);
                    IBuffer buf = new Windows.Storage.Streams.Buffer(image.PixelBuffer.Capacity);
                    buf.Length = image.PixelBuffer.Length;                  
                    myDoc.DrawPage(0, buf, 0, 0, width, height, false);
                    var ss = buf.AsStream();
                    await ss.CopyToAsync(image.PixelBuffer.AsStream());
                    image.Invalidate();

                    // temporarily save the image and use the system to generate thumbnails, then delete the image
                    var x = await image.SaveAsync(NuSysStorages.SaveFolder);
                    thumbnails = await MediaUtil.GetThumbnailDictionary(x);
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

                    data = Convert.ToBase64String(fileBytes);
                    thumbnails=await MediaUtil.GetThumbnailDictionary(storageFile);
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
                    if (!string.IsNullOrEmpty(pdf_text))
                    {
                        args = new CreateNewPdfContentRequestArgs()
                        {
                            PdfText = pdf_text
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

                    if(!SessionController.Instance.ContentController.ContainsContentDataModel(contentDataModelId))
                    {
                        await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(contentDataModelId);
                    }

                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    request.AddReturnedElementToSession();
                }
                else
                {
                    var collection = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId) as CollectionLibraryElementModel;
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, collection.IsFinite,
                            new List<Point>(collection?.ShapePoints?.Select(p => new Point(p.X, p.Y)) ?? new List<Point>()), size.Width, size.Height);
                }
            });

            // TODO: Remove Tood
            // TOOD: refresh library
        }

        private void Folder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.AddFile();
        }

        public FrameworkElement HeaderRow
        {
            get { return xHeaderRow; }
        }


        //private void Favorites_OnTapped(object sender, TappedRoutedEventArgs e)
        //{

        //    _propertiesWindow.Visibility = Visibility.Collapsed;

        //    if (((Button)sender == btnFav))
        //    {
        //        ListContainer.Children.Clear();
        //        ListContainer.Children.Add(_libraryFavorites);
        //    }
        //    else if (((Button)sender == btnAll))
        //    {
        //        ListContainer.Children.Clear();
        //        ListContainer.Children.Add(_libraryList);
        //    }

        //    this.updateTabs();

        //}

        private void updateTabs()
        {
            //if (ListContainer.Children[0] == _libraryFavorites)
            //{
            //    btnFav.Background = new SolidColorBrush(Colors.White);
            //    btnAll.Background = (SolidColorBrush)Application.Current.Resources["color9"];
            //}
            //else if (ListContainer.Children[0] == _libraryList)
            //{
            //    btnAll.Background = new SolidColorBrush(Colors.White);
            //    btnFav.Background = (SolidColorBrush)Application.Current.Resources["color9"];
            //}
        }

        private void Graph_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // find x and y coordintes of pointer, to be used in graph image manipulation
            var view = SessionController.Instance.SessionView;
            _graphButtonX = e.GetCurrentPoint(view).Position.X;
            _graphButtonY = e.GetCurrentPoint(view).Position.Y;
        }

        private void Graph_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            // show draggable graph image
            var draggedGraphImage = SessionController.Instance.SessionView.GraphImage;
            Canvas.SetZIndex(draggedGraphImage, 3);
            draggedGraphImage.Width = 100;
            draggedGraphImage.Height = 100;

            // set up transforms
            draggedGraphImage.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)draggedGraphImage.RenderTransform;
            t.TranslateX += _graphButtonX - (draggedGraphImage.Width / 2);
            t.TranslateY += _graphButtonY - (draggedGraphImage.Height / 2);
        }



        private void Graph_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // moves draggable graph image based on pointer manipulation
            var view = SessionController.Instance.SessionView;
            var t = (CompositeTransform)view.GraphImage.RenderTransform;
            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;
        }

        private async void Graph_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Hid the dragged graph image/button
            var draggedGraphImage = SessionController.Instance.SessionView.GraphImage;
            draggedGraphImage.Width = 0;
            draggedGraphImage.Height = 0;

            // extract composite transform and add the chart/graph based on the dropped location
            var t = (CompositeTransform)draggedGraphImage.RenderTransform;
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            //     var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(e.Position.X, e.Position.Y, 300, 300));
            var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(t.TranslateX, t.TranslateY));


            // graph is added by passing in the bounding rectangle
            await ExportSearchResultsToCollection(r);

        }

        /// <summary>
        /// Exports the search results to a collection, and places the new collection at the passed in point
        /// @tdgreen, plz comment this, thx. -zkirsche 
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private async Task ExportSearchResultsToCollection(Point r)
        {

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            // TODO: add the graph/chart
            var contentId = SessionController.Instance.GenerateId();
            var newCollectionId = SessionController.Instance.GenerateId();

            var elementMsg = new Message();
            elementMsg["metadata"] = metadata;
            elementMsg["width"] = 300;
            elementMsg["height"] = 300;
            elementMsg["x"] = r.X;
            elementMsg["y"] = r.Y;
            elementMsg["contentId"] = contentId;
            elementMsg["type"] = NusysConstants.ElementType.Collection;
            elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
            elementMsg["id"] = newCollectionId;
            if (ListContainer.Children[0] == _libraryList)
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, "", NusysConstants.ElementType.Collection, "Search Results for '"+Searchfield.Text+"'"));
            else if (ListContainer.Children[0] == _libraryFavorites)
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, "", NusysConstants.ElementType.Collection, "Favorites"));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new SubscribeToCollectionRequest(contentId));

            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(elementMsg)); 

            var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(r.X, r.Y, contentId, false, new List<Windows.Foundation.Point>(), 300, 300, newCollectionId);
            if (ListContainer.Children[0] == _libraryList)
            {
                foreach (var libraryItemTemplate in _pageViewModel.ItemList.ToList().GetRange(0, Math.Min(_pageViewModel.ItemList.Count, 30)))
                {
                    var dict = new Message();
                    dict["title"] = libraryItemTemplate?.Title;
                    dict["width"] = "300";
                    dict["height"] = "300";
                    dict["type"] = libraryItemTemplate?.Type;
                    dict["x"] = "50000";
                    dict["y"] = "50000";
                    dict["contentId"] = libraryItemTemplate?.LibraryElementId;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = controller.LibraryElementModel.LibraryElementId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                }
            }
            else if (ListContainer.Children[0] == _libraryFavorites)
            {
                foreach (var itemTemplate in _favoritesViewModel.ItemList.ToList().GetRange(0, Math.Min(_favoritesViewModel.ItemList.Count, 30)))
                {
                    var dict = new Message();
                    dict["title"] = itemTemplate?.Title;
                    dict["width"] = "300";
                    dict["height"] = "300";
                    dict["type"] = itemTemplate?.Type;
                    dict["x"] = "50000";
                    dict["y"] = "50000";
                    dict["contentId"] = itemTemplate?.LibraryElementId;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = controller.LibraryElementModel.LibraryElementId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
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
            view.LibraryDraggingRectangle.SwitchType(NusysConstants.ElementType.Collection);
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