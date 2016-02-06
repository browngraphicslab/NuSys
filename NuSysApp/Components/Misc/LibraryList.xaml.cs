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

namespace NuSysApp
{
    public sealed partial class LibraryList : UserControl
    {
        private ObservableCollection<StackPanel> _items;
        public LibraryList(List<LibraryElement> items, LibraryView library)
        {
            this.InitializeComponent();
            ListBox.ItemsSource = items;
            library.OnNewContents += Refresh;
        }

        private void Refresh(ICollection<LibraryElement> elements)
        {
            /*
            _items = new ObservableCollection<StackPanel>();
            
            foreach (var element in elements)
            {
                StackPanel itemPanel = new StackPanel();
                itemPanel.Orientation = Orientation.Horizontal;
                TextBlock title = new TextBlock();
                title.Text = element.Title + "  |  ";
                TextBlock nodeType = new TextBlock();
                nodeType.Text = element.NodeType.ToString();
                itemPanel.Children.Add(title);
                itemPanel.Children.Add(title);
            }
            
            ListBox.ItemsSource = _items;
            */
            ListBox.ItemsSource = new ObservableCollection<LibraryElement>(elements);
        }


    }
}
