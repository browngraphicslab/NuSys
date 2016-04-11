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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MyDataGrid
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
          

            
            var header1 = new HarshHeader {ColIndex = 0, Title = "Header0"};
            var header2 = new HarshHeader {ColIndex = 1, Title = "Header1"};
            var header3 = new HarshHeader {ColIndex = 2, Title = "Header2"};
            var header4 = new HarshHeader {ColIndex = 3, Title = "Header3"};

            var vm = new DataGridViewModel();
            vm.Header = new ObservableCollection<HarshHeader>{header1, header2, header3, header4};
            
            DataContext = vm;
            this.InitializeComponent();

        }
    }
}
