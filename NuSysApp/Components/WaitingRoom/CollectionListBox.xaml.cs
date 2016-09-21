using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using NusysIntermediate;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    /// This will be used (likely temporarily) to populate the waiting room's collection listview.
    /// this box allows for us to easily set the title, date, users, etc. of the collection in grid columns.
    /// also accounts for whether the collection is made by rosemary so we can hide it and not mess it up while testing 
    public sealed partial class CollectionListBox : UserControl
    {
        private WaitingRoomView _waitingRoom;
        public LibraryElementModel LibraryElementModel;
        public string ID { set; get; }
        public string Title { set { TitleBox.Text = value; } get { return TitleBox.Text; } }
        public string Date { set; get; }
        public string Access { set; get; }

        public bool MadeByRosemary = false;

        public CollectionListBox(LibraryElementModel libraryElementModel, WaitingRoomView w)
        {
            this.InitializeComponent();

            _waitingRoom = w;
            LibraryElementModel = libraryElementModel;
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(LibraryElementModel.LibraryElementId);
            controller.TitleChanged += ControllerOnTitleChanged;

            ID = libraryElementModel.LibraryElementId;
            Title = libraryElementModel.Title;
            Date = libraryElementModel.Timestamp;
            Access = "public"; //only temporary - when merging with acls make this real

            TitleBox.Text = Title;
            DateBox.Text = Date;

            if (libraryElementModel.Creator == "rms" || libraryElementModel.Creator == "rosemary")
            {
                MadeByRosemary = true;
            }
            AccessBox.Text = libraryElementModel.AccessType.ToString();
            Access = libraryElementModel.AccessType.ToString();
        }

        private void ControllerOnTitleChanged(object sender, string s)
        {
            Title = s;
        }

        private void Collection_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (_waitingRoom != null)
            {
                _waitingRoom.SetSelectedCollection(LibraryElementModel);
                _waitingRoom.Join_Workspace_Click(sender, e);
            }
        }
    }
}
