using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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

        private LibraryList _libraryList;
        private LibraryGrid _libraryGrid;
        private FloatingMenuView _menu;
        private double _graphButtonX;
        private double _graphButtonY;

        //private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView(LibraryBucketViewModel vm, LibraryElementPropertiesWindow properties, FloatingMenuView menu)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            LibraryPageViewModel pageViewModel = new LibraryPageViewModel(new ObservableCollection<NodeContentModel>(SessionController.Instance.ContentController.Values));
            this.MakeViews(pageViewModel, properties);
            WorkspacePivot.Content = _libraryList;
            _menu = menu;
            this.Loaded += async delegate
            {
                vm.InitializeLibrary();
            };
        }

        public async void ToggleVisiblity()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed: Visibility.Visible;
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
        //            OnNewContents?.Invoke(_elements.Values);
        //        });
        //    });
        //}

        //public void AddNewElement(LibraryElement element)
        //{
        //    _elements.Add(element.ContentID, element);
        //    OnNewElementAvailable?.Invoke(element);
        //}
        public void MakeViews(LibraryPageViewModel pageViewModel, LibraryElementPropertiesWindow properties)
        {
            _libraryGrid = new LibraryGrid(this, pageViewModel, properties);
            _libraryList = new LibraryList(this, pageViewModel, properties);
            //_libraryList.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).ListViewBase_OnDragItemsStarting;
            //_libraryGrid.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).GridViewDragStarting;
        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((LibraryViewable)(WorkspacePivot?.Content)).Sort(((ComboBox)sender)?.SelectedItem.ToString());
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ((LibraryViewable)(WorkspacePivot?.Content)).SetItems(SessionController.Instance.ContentController.Values);
            ((LibraryViewable)(WorkspacePivot?.Content)).Search(sender.Text.ToLower());
        }

        private async void ListButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (WorkspacePivot.Content != _libraryList)
            {
                await _libraryList.Update();
                WorkspacePivot.Content = _libraryList;
            }
        }

        public async void UpdateList()
        {
            _libraryList.Update();
        }

        private async void GridButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //await this.AddNode(new Point(12, 0), new Size(12, 12), ElementType.Document);
            this.UpdateList();
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
        //                SessionController.Instance.NuSysNetworkSession.FetchContent(id);
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
        //                m["creators"] = new List<string>() { SessionController.Instance.ActiveWorkspace.Id };

        //                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewNodeRequest(m));
        //            }
        //        });
        //    });
        //}


        //Trent, this needs to be filled in in order for the importing to the library to work.
        private async void AddFile()
        {
            //TODO: Add code that adds local files to the library. Below is the old code that accomplished this. (Decided to separate AddFile from AddNode)
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            ElementType elementType = ElementType.Text;
            string data = "";
            string title = "";

            var storageFile = await FileManager.PromptUserForFile(Constants.AllFileTypes);
            if (storageFile == null) return;

            var fileType = storageFile.FileType.ToLower();
            title = storageFile.DisplayName;

            bool validFileType = true;

            if (Constants.ImageFileTypes.Contains(fileType))
            {
                elementType = ElementType.Image;
                data = Convert.ToBase64String(await MediaUtil.StorageFileToByteArray(storageFile));
            }
            else if (Constants.WordFileTypes.Contains(fileType))
            { 
                elementType = ElementType.Word;
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
                        await reader.LoadAsync((uint) stream.Size);
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
                var contentId = SessionController.Instance.GenerateId();

                await
                    SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                        new CreateNewLibraryElementRequest(contentId, data, elementType, title));
                //await SessionController.Instance.NuSysNetworkSession.ExecuteSystemRequest(new NewContentSystemRequest(contentId, data == null ? "" : data.ToString()), NetworkClient.PacketType.TCP, null, true);

                // TOOD: refresh library

                vm.ClearSelection();
                //   vm.ClearMultiSelection();
            }
            else
            {
                Debug.WriteLine("tried to import invalid filetype");
            }

        }
        public async Task AddNode(Point pos, Size size, ElementType elementType, string contentId)
        {
            var dict = new Message();
            Dictionary<string, object> metadata;

            metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;
            metadata["node_type"] = elementType + "Node";
            
            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["nodeType"] = elementType.ToString();
            dict["x"] = pos.X;
            dict["y"] = pos.Y;
            dict["contentId"] = contentId;
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;
            dict["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            var request = new NewElementRequest(dict);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

            // TOOD: refresh library
        }

        private void Folder_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.AddFile();
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
            draggedGraphImage.Width = 200;
            draggedGraphImage.Height = 200;

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

        private void Graph_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            // Hid the dragged graph image/button
            var draggedGraphImage = SessionController.Instance.SessionView.GraphImage;
            draggedGraphImage.Width = 0;
            draggedGraphImage.Height = 0;

            // extract composite transform and add the chart/graph based on the dropped location
            var t = (CompositeTransform)draggedGraphImage.RenderTransform;
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(e.Position.X, e.Position.Y, 300, 300));

            // graph is added by passing in the bounding rectangle
            this.AddGraph(r);
            
        }

        // adds the graph/chart based on the location that the graph button was dragged to
        private void AddGraph(Rect r)
        {
          
            // TODO: add the graph/chart

        }
    }
}
