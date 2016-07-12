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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupDetailHomeTabView : UserControl
    {

        private ObservableCollection<FrameworkElement> _views;
        private FreeFormNodeViewFactory _factory;
        private int _count = 0;

        public GroupDetailHomeTabView(GroupDetailHomeTabViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;

            var model = vm.Model;
            //If same collection, disable enter collection button
            var id = ((GroupDetailHomeTabViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;

            // Show the return to origin button if you are currently in the collection
            if (id == SessionController.Instance.ActiveFreeFormViewer.ContentId)
            {

                ReturnToOriginButton.Visibility = Visibility.Visible;
            }
            // Otherwise, let the user enter the collection
            else
            {
                EnterCollectionButton.Visibility = Visibility.Visible;
            }

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
             ((GroupDetailHomeTabViewModel)DataContext).Controller.SetContentData(s);
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
            if (((GroupDetailHomeTabViewModel)DataContext).Controller.LibraryElementModel.Data != "")
            {
                UpdateText(((GroupDetailHomeTabViewModel)DataContext).Controller.LibraryElementModel.Data);
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


        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (GroupDetailHomeTabViewModel)DataContext;
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
            var allNodes = SessionController.Instance.IdToSendables.ContentValues;
            var modelList = new ObservableCollection<ElementModel>();
            foreach (var sendable in allNodes)
            {
                var node = sendable.Model;
                var groups = (List<string>) node.GetMetaData("groups");
                if (groups.Contains(vm.ContentId))
                {
                    modelList.Add(node);
                }
            }

            foreach (var model in modelList)
            {
                var nodeModel = SessionController.Instance.IdToSendables[model.ContentId];
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

        private void EnterCollectionButton_Click(object sender, RoutedEventArgs e)
        {

            
            var id = ((GroupDetailHomeTabViewModel)DataContext).Controller.LibraryElementModel.LibraryElementId;
            if (id != SessionController.Instance.ActiveFreeFormViewer.ContentId)
            {
                UITask.Run(async delegate
                {
                    var content = SessionController.Instance.ContentController.GetContent(id);
                    if (content != null && content.Type == ElementType.Collection)
                    {
                        List<Message> messages = new List<Message>();
                        await Task.Run(async delegate
                        {
                            messages = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(id);
                        });
                        Visibility = Visibility.Collapsed;
                        SessionController.Instance.FireEnterNewCollectionEvent();
                        await
                            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                                new UnsubscribeFromCollectionRequest(
                                    SessionController.Instance.ActiveFreeFormViewer.ContentId));
                        await SessionController.Instance.SessionView.LoadWorkspaceFromServer(messages, id);
                    }
                });


            }

            SessionController.Instance.SessionView.DetailViewerView.Visibility = Visibility.Collapsed;

        }

        /// <summary>
        /// Calls a method to return the camera to the origin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReturnToOriginButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.ReturnCameraToOrigin();
        }

        /// <summary>
        /// Returns the camera to the origin using a storyboard
        /// </summary>
        private void ReturnCameraToOrigin()
        {
            var duration = new Duration(TimeSpan.FromSeconds(1));
            var storyboard = new Storyboard();
            storyboard.Duration = duration;

            // Saves the final product as a composite transform and updates other transforms based on this
            var futureTransform = new CompositeTransform
            {
                TranslateX = Constants.InitialTranslate,
                TranslateY = Constants.InitialTranslate,
                ScaleX = Constants.InitialScale,
                ScaleY = Constants.InitialScale,
                CenterX = Constants.InitialCenter,
                CenterY = Constants.InitialCenter
            };
            SessionController.Instance.SessionView.FreeFormViewer.PanZoom.UpdateTempTransform(futureTransform);
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Transform = futureTransform;
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Redraw();

            // Create a DoubleAnimation for each property to animate
            var scaleAnimationX = MakeAnimationElement(futureTransform.ScaleX, "ScaleX", duration);
            var scaleAnimationY = MakeAnimationElement(futureTransform.ScaleY, "ScaleY", duration);
            var centerAnimationX = MakeAnimationElement(futureTransform.CenterX, "CenterX", duration);
            var centerAnimationY = MakeAnimationElement(futureTransform.CenterY, "CenterY", duration);
            var translateAnimationX = MakeAnimationElement(futureTransform.TranslateX, "TranslateX", duration);
            var translateAnimationY = MakeAnimationElement(futureTransform.TranslateY, "TranslateY", duration);
            var animationList = new List<DoubleAnimation>(new DoubleAnimation[] { scaleAnimationX, scaleAnimationY, centerAnimationX, centerAnimationY, translateAnimationX, translateAnimationY });

            // Add each animation to the storyboard
            foreach (var anim in animationList)
            {
                storyboard.Children.Add(anim);
            }

            // Begin the animation.
            storyboard.Begin();
        }

        /// <summary>
        /// Produces an animation element to animate a certain property transition using a storyboard
        /// </summary>
        /// <param name="to"></param>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="transform"></param>
        /// <param name="dependent"></param>
        /// <returns></returns>
        private DoubleAnimation MakeAnimationElement(double to, String name, Duration duration,
            CompositeTransform transform = null, bool dependent = false)
        {

            if (transform == null)
            {
                transform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            }

            var toReturn = new DoubleAnimation();
            toReturn.EnableDependentAnimation = true;
            toReturn.Duration = duration;
            Storyboard.SetTarget(toReturn, transform);
            Storyboard.SetTargetProperty(toReturn, name);
            toReturn.To = to;
            toReturn.EasingFunction = new QuadraticEase();
            return toReturn;
        }
    }
 }

