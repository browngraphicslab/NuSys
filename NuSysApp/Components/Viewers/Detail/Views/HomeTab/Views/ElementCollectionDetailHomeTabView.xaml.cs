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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class CollectionDetailView : UserControl
    {
        public CollectionDetailView(ElementCollectionViewModel vm)
        {
            this.InitializeComponent();

            DataContext = vm;
            // SetDimension(SessionController.Instance.SessionView.ActualWidth / 2 - 30);

            var model = (TextElementModel)vm.Model;


            List<Uri> AllowedUris = new List<Uri>();
            AllowedUris.Add(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));


            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                await SessionController.Instance.InitializeRecog();
                SetHeight(SessionController.Instance.SessionView.ActualHeight / 2);
            };

            MyWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));
        }

        public void SetHeight(double parentHeight)
        {
            MyWebView.Height = parentHeight;
        }


    }
}
