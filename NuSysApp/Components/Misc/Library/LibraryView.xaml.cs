using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        //private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView(LibraryBucketViewModel vm, LibraryElementPropertiesWindow properties)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            LibraryPageViewModel pageViewModel = new LibraryPageViewModel(new ObservableCollection<LibraryElement>(((LibraryBucketViewModel)this.DataContext)._elements.Values));
            this.MakeViews(pageViewModel, properties);
            WorkspacePivot.Content = _libraryList;

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
            _libraryList.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).ListViewBase_OnDragItemsStarting;
            _libraryGrid.OnLibraryElementDrag += ((LibraryBucketViewModel)this.DataContext).GridViewDragStarting;
        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((LibraryViewable)(WorkspacePivot?.Content)).Sort(((ComboBox)sender)?.SelectedItem.ToString());
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ((LibraryViewable)(WorkspacePivot?.Content)).SetItems(((LibraryBucketViewModel)this.DataContext)._elements.Values);
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

        private async void GridButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (WorkspacePivot.Content != _libraryGrid)
            {
                await _libraryGrid.Update();
                WorkspacePivot.Content = _libraryGrid;
            }
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

    }
}
