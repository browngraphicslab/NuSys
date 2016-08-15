using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public string ID { set; get; }
        public string Title { set; get; }
        public string Date { set; get; }
        public string Access { set; get; }
        public bool MadeByRosemary = false;

        public CollectionListBox(LibraryElementModel m)
        {
            this.InitializeComponent();

            ID = m.LibraryElementId;
            Title = m.Title;
            Date = m.Timestamp;
            Access = "public"; //only temporary - when merging with acls make this real

            TitleBox.Text = Title;
            DateBox.Text = Date;

            if (m.Creator == "rms" || m.Creator == "rosemary")
            {
                MadeByRosemary = true;
            }
        }
    }
}
