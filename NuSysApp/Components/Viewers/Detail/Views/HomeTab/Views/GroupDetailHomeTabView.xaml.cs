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
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupDetailHomeTabView : UserControl
    {

        private ObservableCollection<FrameworkElement> _views;
        private int _count = 0;

        private double _x;
        private double _y;
        private string _libraryElementId;

        public GroupDetailHomeTabView(GroupDetailHomeTabViewModel vm)
        {
            this.InitializeComponent();
            DataContext = vm;
            _libraryElementId = vm.LibraryElementController.LibraryElementModel?.LibraryElementId;

            var model = vm.Model;
            //If same collection, disable enter collection button
            var id = ((GroupDetailHomeTabViewModel)DataContext).LibraryElementController.LibraryElementModel.LibraryElementId;

            // Show the return to origin button if you are currently in the collection
            if (id == SessionController.Instance.ActiveFreeFormViewer.LibraryElementId)
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


            vm.LibraryElementController.Disposed += ControllerOnDisposed;
        }


        private void UpdateModelText(String s)
        {
             ((GroupDetailHomeTabViewModel)DataContext).LibraryElementController.ContentDataController.SetData(s);
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
            if (((GroupDetailHomeTabViewModel)DataContext).LibraryElementController.Data != "")
            {
                UpdateText(((GroupDetailHomeTabViewModel)DataContext).LibraryElementController.Data);
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
            vm.LibraryElementController.Disposed -= ControllerOnDisposed;
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
            var id = ((GroupDetailHomeTabViewModel)DataContext).LibraryElementController.LibraryElementModel.LibraryElementId;
            if (id != SessionController.Instance.ActiveFreeFormViewer.LibraryElementId)
            {
                UITask.Run(async delegate
                {
                    var content = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                    if (content != null && content.Type == NusysConstants.ElementType.Collection)
                    {
                        Visibility = Visibility.Collapsed;
                        await SessionController.Instance.EnterCollection(id);
                    }
                }); 
            }

            SessionController.Instance.SessionView.DetailViewerView.CloseDv();

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

        #region addToCollection
        private void AddToCollection_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _x = e.GetCurrentPoint(view).Position.X - 25;
            _y = e.GetCurrentPoint(view).Position.Y - 25;
        }

        private void AddToCollection_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((SessionController.Instance.ActiveFreeFormViewer.LibraryElementId == element?.LibraryElementId) ||
                (element?.Type == NusysConstants.ElementType.Link))
            {
                return;
            }

            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(element);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;


            t.TranslateX += _x;
            t.TranslateY += _y;

            if (!SessionController.Instance.ContentController.ContainsContentDataModel(element.ContentDataModelId))
            {
                Task.Run(async delegate
                {
                    SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(element.ContentDataModelId);
                });
            }

        }



        private void AddToCollection_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
            {
                return;
            }

            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);

            var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where(i => i is LibraryView);
            if (itemsBelow.Any())
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Hide();
            }
            else
            {
                SessionController.Instance.SessionView.LibraryDraggingRectangle.Show();

            }
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;
            var t = (CompositeTransform)rect.RenderTransform;

            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;

            _x += e.Delta.Translation.X;
            _y += e.Delta.Translation.Y;

        }

        private async void AddToCollection_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            LibraryElementModel element = SessionController.Instance.ContentController.GetLibraryElementModel(_libraryElementId);
            if ((SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId == element.LibraryElementId) || (element.Type == NusysConstants.ElementType.Link))
            {
                return;
            }

            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;


            if (rect.Visibility == Visibility.Collapsed)
                return;

            rect.Hide();
            var r =
                SessionController.Instance.SessionView.MainCanvas.TransformToVisual(
                    SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));

            if (_x > SessionController.Instance.SessionView.MainCanvas.ActualWidth - SessionController.Instance.SessionView.DetailViewerView.ActualWidth) return;

            await AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type, element.LibraryElementId);
        }

        public async Task AddNode(Point pos, Size size, NusysConstants.ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                var args = new NewElementRequestArgs();
                args.Width = size.Width;
                args.Height = size.Height;
                args.LibraryElementId = libraryId;
                args.ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
                args.X = pos.X;
                args.Y = pos.Y;

                if (elementType != NusysConstants.ElementType.Collection)
                {
                    var request = new NewElementRequest(args);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                }
                else
                {
                    await StaticServerCalls.PutCollectionInstanceOnMainCollection(args);
                }
            });
        }

        #endregion addToCollection

    }
}

