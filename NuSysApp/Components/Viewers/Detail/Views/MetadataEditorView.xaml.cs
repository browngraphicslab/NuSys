using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Viewers.Detail.Views
{
    public sealed partial class MetadataEditorView : UserControl
    {

        private ObservableCollection<Entry> Metadata; 

        public MetadataEditorView()
        {
            this.InitializeComponent();
            Metadata=new ObservableCollection<Entry>();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var entry = new Entry();
            entry.Key = xField.Text;
            entry.Value = xValue.Text;
            Metadata.Add(entry);
            xField.Text = "";
            xValue.Text = "";
            
        }
    }
}
