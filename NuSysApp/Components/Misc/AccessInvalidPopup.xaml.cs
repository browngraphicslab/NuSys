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
    public sealed partial class AccessInvalidPopup : UserControl
    {

        private LibraryElementModel _element;

        public AccessInvalidPopup()
        {
            this.InitializeComponent();
        }

        private void OK_OnClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void MakePublic_OnClick(object sender, RoutedEventArgs e)
        {
            _element.AccessType = NusysConstants.AccessType.Public;
        }

        public LibraryElementModel Element
        {
            set { if (value != null) _element = value; }
            get { return _element; }
        }
    }
}
