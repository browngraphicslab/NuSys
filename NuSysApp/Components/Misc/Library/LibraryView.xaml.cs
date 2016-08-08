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
using SharpDX.Direct2D1;
using SharpDX.WIC;
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
            this._libraryList.DeleteClicked += DeleteClicked;
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
        //                SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(id);
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

            ElementType elementType = ElementType.Text;
            string data = "";
            string title = "";
            string pdf_text = "";

            var storageFiles = await FileManager.PromptUserForFiles(Constants.AllFileTypes);
            foreach (var storageFile in storageFiles)
            {
                if (storageFile == null) return;

                var contentId = SessionController.Instance.GenerateId();
                string serverURL = null;

                var fileType = storageFile.FileType.ToLower();
                title = storageFile.DisplayName;

                bool validFileType = true;
                // Create a thumbnail dictionary mapping thumbnail sizes to the byte arrays.
                // Note that only video and images are to get thumbnails this way, currently.
                var thumbnails = new Dictionary<ThumbnailSize, string>();
                thumbnails[ThumbnailSize.SMALL] = "";
                thumbnails[ThumbnailSize.MEDIUM] = "";
                thumbnails[ThumbnailSize.LARGE] = "";

                if (Constants.ImageFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Image;
                    data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
                    serverURL = contentId + fileType;
                    thumbnails =await MediaUtil.GetThumbnailDictionary(storageFile);
                }
                else if (Constants.WordFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Word;
                    
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
                    elementType = ElementType.Powerpoint;
                }
                else if (Constants.PdfFileTypes.Contains(fileType))
                {
                    elementType = ElementType.PDF;
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

                    // Get text from the pdf
                    var myDoc = await MediaUtil.DataToPDF(data); 
                    
                    int numPages = myDoc.PageCount;
                    int currPage = 0;
                    while (currPage < numPages)
                    {
                        pdf_text = pdf_text + myDoc.GetAllTexts(currPage);
                        currPage++;
                    }

                    /// The following was supposed to create a thumbnail from the first page of the pdf and save
                    /// it as a 64 bit string, however there is an exception thrown when converting the 
                    /// RenderBitmap to a ByteArray...something about the index being larger than the capacity of 
                    /// the buffer...

                    /*
                    // Instantiate a MuPDF doc and save the rendered first page to a WritableBitmap
                    var doc = await MediaUtil.DataToPDF(data);
                    var width = 50;
                    var height = 50;
                    var writableBitmap = new WriteableBitmap(width, height);
                    IBuffer buf = new Windows.Storage.Streams.Buffer(writableBitmap.PixelBuffer.Capacity);
                    buf.Length = writableBitmap.PixelBuffer.Length;
                    doc.DrawPage(1, buf, 0, 0, 0, 0, false);
                    var ss = buf.AsStream();
                    await ss.CopyToAsync(writableBitmap.PixelBuffer.AsStream());
                    writableBitmap.Invalidate();

                    // Create an Image, and set the source to the writable bitmap
                    var myImage = new Image();
                    myImage.Source = writableBitmap;
                    myImage.Height = 50;
                    myImage.Width = 50;

                    // Take screenshot of Image using a render bitmap. In order to do this, the image must
                    // be inside a visual component, like the session view.
                    var r = new RenderTargetBitmap();
                    SessionController.Instance.SessionView.MainCanvas.Children.Add(myImage);
                    await r.RenderAsync(myImage);
                    SessionController.Instance.SessionView.MainCanvas.Children.Remove(myImage);

                    // Obtain a ByteArray from the RenderBitmap, store it as a string in the thumbnail dictionary
                    var tdata = await MediaUtil.RenderTargetBitmapToByteArray(r);
                    thumbnails[ThumbnailSize.SMALL] =
                        Convert.ToBase64String(tdata);
                    */

                }
                else if (Constants.VideoFileTypes.Contains(fileType))
                {
                    elementType = ElementType.Video;
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
                    elementType = ElementType.Audio;
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
                    var m = new Message();
                    m["id"] = contentId;
                    m["data"] = data;
                    m["small_thumbnail"] = thumbnails[ThumbnailSize.SMALL];
                    //await StorageUtil.SaveAsStorageFile(thumbnails[ThumbnailSize.SMALL], @"C:\Users\Zach\Documents\test.jpg");
                    m["medium_thumbnail"] = thumbnails[ThumbnailSize.MEDIUM];
                    m["large_thumbnail"] = thumbnails[ThumbnailSize.LARGE];
                    if (!string.IsNullOrEmpty(pdf_text))
                    {
                        m["pdf_text"] = pdf_text;
                    }
                    m["type"] = elementType.ToString();
                    if (title != null)
                    {
                        m["title"] = title;
                    }
                    if (serverURL != null)
                    {
                        m["server_url"] = serverURL;
                    }
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(m));
                    vm.ClearSelection();
                    //   vm.ClearMultiSelection();
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

        public async Task AddNode(Point pos, Size size, ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != ElementType.Collection)
                {
                    var element = SessionController.Instance.ContentController.GetContent(libraryId);
                    var dict = new Message();
                    Dictionary<string, object> metadata;

                    metadata = new Dictionary<string, object>();
                    metadata["node_creation_date"] = DateTime.Now;
                    metadata["node_type"] = elementType + "Node";

                    dict = new Message();
                    dict["title"] = element?.Title + " element";
                    dict["width"] = size.Width.ToString();
                    dict["height"] = size.Height.ToString();
                    dict["type"] = elementType.ToString();
                    dict["x"] = pos.X;
                    dict["y"] = pos.Y;
                    dict["contentId"] = libraryId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                }
                else
                {
                    var collection = SessionController.Instance.ContentController.GetContent(libraryId) as CollectionLibraryElementModel;
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, collection.IsFinite, 
                            collection.ShapePoints, size.Width, size.Height);
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
            elementMsg["type"] = ElementType.Collection;
            elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            elementMsg["id"] = newCollectionId;
            if (ListContainer.Children[0] == _libraryList)
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Collection, "Search Results for '"+Searchfield.Text+"'"));
            else if (ListContainer.Children[0] == _libraryFavorites)
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Collection, "Favorites"));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SubscribeToCollectionRequest(contentId));

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
                    dict["contentId"] = libraryItemTemplate?.ContentID;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = controller.LibraryElementModel.LibraryElementId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
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
                    dict["contentId"] = itemTemplate?.ContentID;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = controller.LibraryElementModel.LibraryElementId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
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
            view.LibraryDraggingRectangle.SwitchType(ElementType.Collection);
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

    public enum ThumbnailSize
    {
        SMALL,MEDIUM,LARGE
    }

}