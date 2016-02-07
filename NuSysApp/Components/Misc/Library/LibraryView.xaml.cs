using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
        public delegate void NewContentsEventHandler(ICollection<LibraryElement> elements);
        public event NewContentsEventHandler OnNewContents;

        public delegate void NewElementAvailableEventHandler(LibraryElement element);
        public event NewElementAvailableEventHandler OnNewElementAvailable;

        private LibraryList _libraryList;
        private LibraryGrid _libraryGrid;

        private Dictionary<string, LibraryElement> _elements = new Dictionary<string, LibraryElement>();
        public LibraryView()
        {
            this.InitializeComponent();
            this.MakeViews();
        }

        public async void ToggleVisiblity()
        {
            Visibility = Visibility == Visibility.Visible ? Visibility.Collapsed: Visibility.Visible;
        }

        public async Task Reload()
        {
            Task.Run(async delegate
            {
                var dictionaries = await SessionController.Instance.NuSysNetworkSession.GetAllLibraryElements();
                foreach (var kvp in dictionaries)
                {
                    var id = kvp.Value["id"];
                    var element = new LibraryElement(kvp.Value);
                    if (!_elements.ContainsKey(id))
                    {
                        _elements.Add(id, element);
                    }
                }
                UITask.Run(delegate {
                    OnNewContents?.Invoke(_elements.Values);
                });
            });
        }

        public void AddNewElement(LibraryElement element)
        {
            _elements.Add(element.ContentID, element);
            OnNewElementAvailable?.Invoke(element);
        }
        public void MakeViews()
        {
            _libraryGrid = new LibraryGrid(new ObservableCollection<LibraryElement>(_elements.Values));
            _libraryList = new LibraryList(new List<LibraryElement>(_elements.Values),this);
            WorkspacePivot.Content = _libraryList;

            //var filesGrid = new LibraryGrid(new ObservableCollection<LibraryElement>(_elements.Values));
            //var filesList = new LibraryList(new ObservableCollection<LibraryElement>(_elements.Values));
            //FilesPivot.Content = filesList;
        }

        private void ComboBox1_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((LibraryViewable) (WorkspacePivot?.Content)).Sort(((ComboBox) sender)?.SelectedItem.ToString());
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            ((LibraryViewable)(WorkspacePivot?.Content)).SetItems(_elements.Values);
            ((LibraryViewable)(WorkspacePivot?.Content)).Search(sender.Text.ToLower());
        }
    }
}
