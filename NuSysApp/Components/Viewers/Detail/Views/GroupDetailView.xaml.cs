using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class GroupDetailView : UserControl
    {

        private ObservableCollection<FrameworkElement> _views;
        private FreeFormNodeViewFactory _factory;
        
        private int _count = 0;

        public GroupDetailView(ElementCollectionViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            var model = (CollectionElementModel)vm.Model;


            List<Uri> AllowedUris = new List<Uri>();
            AllowedUris.Add(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));


            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                await SessionController.Instance.InitializeRecog();
                SetHeight(SessionController.Instance.SessionView.ActualHeight / 2);
            };

            MyWebView.NavigationCompleted += MyWebViewOnNavigationCompleted;
            MyWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));

            //   _views = new ObservableCollection<FrameworkElement>();

            // _factory = new FreeFormNodeViewFactory();

            // this.AddChildren();

            //Loaded += delegate (object sender, RoutedEventArgs args)
            //{
            //    var sw = SessionController.Instance.SessionView.ActualWidth / 1.2;
            //    var sh = SessionController.Instance.SessionView.ActualHeight / 1.2;

            //    var ratio = xGrid.ActualWidth > xGrid.ActualHeight ? xGrid.ActualWidth / sw : xGrid.ActualHeight / sh;
            //    xGrid.Width = xGrid.ActualWidth / ratio;
            //    xGrid.Height = xGrid.ActualHeight / ratio;
            //};

            MyWebView.ScriptNotify += wvBrowser_ScriptNotify;


            vm.Controller.Disposed += ControllerOnDisposed;
        }


        private void UpdateModelText(String s)
        {
             ((ElementCollectionViewModel)DataContext).Controller.LibraryElementModel.SetContentData((ElementCollectionViewModel)DataContext, s);
        }

        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // The string received from the JavaScript code can be found in e.Value
            string data = e.Value;
            //Debug.WriteLine(data);

            if (data != "")
            {
                UpdateModelText(data);

            }
        }

        private void MyWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (((ElementCollectionViewModel)DataContext).Text != "")
            {
                UpdateText(((ElementCollectionViewModel)DataContext).Text);
            }
        }

        private async void UpdateText(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                String[] myString = { str };
                IEnumerable<String> s = myString;
                MyWebView.InvokeScriptAsync("InsertText", s);
            }
        }

        public void SetHeight(double parentHeight)
        {
            MyWebView.Height = parentHeight;
        }


        private void ControllerOnDisposed(object source)
        {
            var vm = (ElementCollectionViewModel)DataContext;
            MyWebView.NavigationCompleted -= MyWebViewOnNavigationCompleted;
            MyWebView.ScriptNotify -= wvBrowser_ScriptNotify;
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        public async Task AddChildren()
        {
            // TODO: Refactor
            /*
            var vm = (ElementCollectionViewModel) DataContext;
            var allNodes = SessionController.Instance.IdToSendables.Values;
            var modelList = new ObservableCollection<ElementModel>();
            foreach (var sendable in allNodes)
            {
                var node = sendable.Model;
                var groups = (List<string>) node.GetMetaData("groups");
                if (groups.Contains(vm.Id))
                {
                    modelList.Add(node);
                }
            }

            foreach (var model in modelList)
            {
                var nodeModel = SessionController.Instance.IdToSendables[model.Id];
                var view = await _factory.CreateFromSendable(nodeModel.Model, null);
                var viewVm = (ElementViewModel)view.DataContext;
                view.RenderTransform = new CompositeTransform();
                _views.Add(view);
            }

            var numCols = 4;
            var numRows = 4;

            List<FrameworkElement> children = new List<FrameworkElement>();

            foreach (FrameworkElement view in _views)
            {
                Border wrapping = new Border();
                wrapping.Padding = new Thickness(10);
                wrapping.Child = view;
                children.Add(wrapping);

            }

            int count = 0;
            
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    var wrapping = children[count];
                    Grid.SetRow(wrapping, i);
                    Grid.SetColumn(wrapping, j);
                    xGrid.Children.Add(wrapping);
                    count++;
                }
            }
             */
        }
    }
}
