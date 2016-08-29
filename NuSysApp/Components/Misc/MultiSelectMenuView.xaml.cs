using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using Microsoft.Graphics.Canvas.Geometry;
using NetTopologySuite.Geometries;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MultiSelectMenuView : UserControl
    {
        public delegate void CreateCollectionHandler(bool finite, bool shaped);

        public event CreateCollectionHandler CreateCollection;

        public InkStroke Stroke {
            get;
            set; }

        public static Color SelectedColor { get; set; }

        public bool Finite { get; set; }
        public List<Windows.Foundation.Point> Points { get; set; }

        public MultiSelectMenuView()
        {
            this.InitializeComponent();
            DataContext = new object();
            DeleteButton.Click += DeleteButtonOnClick;
            GroupButton.Click += GroupButtonOnClick;

            SelectedColor = Colors.Black;
            Finite = false;
            Points = new List<Windows.Foundation.Point>();
        }

        public void Show(double x = 100, double y= 100)
        {
            Canvas.SetLeft(this, x);
            Canvas.SetTop(this, y);
            ColorPicker.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
            GroupSettings.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Collapsed;
        }
        

        private void GroupButtonOnClick(object sender, RoutedEventArgs e)
        {
            GroupSettings.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Collapsed;
        }

        private void GroupSettingsXOnClick(object sender, RoutedEventArgs e)
        {
            GroupSettings.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
        }

        private async void CreateGroupButtonOnClick(object sender, RoutedEventArgs routedEventArgs)

        /// <summary>
        /// Creates a collection based on the nodes enclosed in the ink stroke. TODO: refactor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        {
            var selections = SessionController.Instance.SessionView.FreeFormViewer.Selections;
            if (selections.Count == 0) {
                Visibility = Visibility.Collapsed;
                return;
            }

            CreateCollection?.Invoke(FiniteCheck.IsOn, ShapeCheck.IsOn);
            Visibility = Visibility.Collapsed;
            return;
            var transform = NuSysRenderer.Instance.GetTransformUntil(selections.First());
            
            var createNewContentRequestArgs = new CreateNewContentRequestArgs
            {
                LibraryElementArgs = new CreateNewLibraryElementRequestArgs
                {
                    AccessType =
                       SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    Title = "Unnamed Collection",
                    LibraryElementId = SessionController.Instance.GenerateId()
                },
                ContentId = SessionController.Instance.GenerateId()
            };

            // execute the content request
            var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
            contentRequest.AddReturnedLibraryElementToLibrary();

            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                Height = Constants.DefaultNodeSize,
                Width = Constants.DefaultNodeSize,
                X = 50000,
                Y = 50000
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();


            return;
            //var bb = Geometry.NodesToBoudingRect(selections.Where(v =>  (v is ElementViewModel)).Select(item=> item as ElementViewModel).ToList());       
            var bb = Stroke.BoundingRect;

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            // TODO: add the graph/chart
            var newLibraryElementId = SessionController.Instance.GenerateId();

            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            // Removes the ink from the canvas
            var req = InkStorage.CreateRemoveInkRequest(new InkWrapper(Stroke, "ink"));
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(Stroke);

            var deleteMsg = new Message();
            deleteMsg["contentId"] = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            var model = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel as CollectionLibraryElementModel;

            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new UpdateLibraryElementModelRequest(deleteMsg));

            // make a pointcollection that will be the "shape" property of the collection (use pointcollection or list?)
            var inkpoints = Stroke.GetInkPoints().ToArray();
            Points = new List<Windows.Foundation.Point>();
            foreach (var i in inkpoints)
            {
                Points.Add(i.Position);
            }

            if (FiniteCheck.IsOn)
            {
                Finite = true;
            } else
            {
                Finite = false;
            }

            if (!ShapeCheck.IsOn)
            {
                Points.Clear();
            }
            /*
            var m = new CreateNewLibraryElementRequestArgs();
            m.LibraryElementId = newLibraryElementId;
            m.LibraryElementType = NusysConstants.ElementType.Collection;
            m.Title = "new collection, " + Finite.ToString() + ", " + Points.Count;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewContentRequest(new CreateNewContentRequestArgs() {LibraryElementArgs = m}));

            //here, settings should also be passed in as parameters
            var args = new NewElementRequestArgs();
            args.Height = bb.Height;
            args.X = bb.X;
            args.Width = bb.Width;
            args.Y = bb.Y;
            args.LibraryElementId = newLibraryElementId;
            args.ParentCollectionId
            var request = new NewElementRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedElementToSessionAsync()

            var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(args);
            */
            Visibility = Visibility.Collapsed;
        }

        private void DeleteButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var selections = SessionController.Instance.ActiveFreeFormViewer.Selections.OfType<ElementViewModel>();
            foreach (var elementViewModel in selections)
            {
                elementViewModel.Controller.RequestDelete();
            }

            Visibility = Visibility.Collapsed;
        }

        public Button Delete
        {
            get { return DeleteButton; }
        }

        public Button Group
        {
            get { return GroupButton; }
        }

        private void Rectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var rect = (Rectangle)sender;
            var brush = (SolidColorBrush)rect.Fill;
            SelectedColor = brush.Color;

            var m = new Message();
            m["width"] = 400;
            m["height"] = 400;
            m["color"] = Colors.Red;
            m["type"] = NusysConstants.ElementType.Area.ToString();
            m["points"] = Stroke.GetInkPoints();
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;

            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddAdorment(Stroke, SelectedColor);
          
            var request = InkStorage.CreateRemoveInkRequest(new InkWrapper(Stroke, "ink"));
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(Stroke);
            //SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request.Item1);

            var deleteMsg = new Message();
            deleteMsg["contentId"] = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            var collectionController = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementController as CollectionLibraryElementController;
            collectionController.InkLines.Remove(request.Item2);
            deleteMsg["inklines"] = new HashSet<string>(collectionController.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new UpdateLibraryElementModelRequest(deleteMsg));

            Visibility = Visibility.Collapsed;
        }

        private void OnChangeAccessClick(object sender, RoutedEventArgs e)
        {
            AccessPanel.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Collapsed;
        }

        private void OnSaveAccessClick(object sender, RoutedEventArgs e)
        {
            AccessPanel.Visibility = Visibility.Collapsed;
            Buttons.Visibility = Visibility.Visible;
        }
    }
}
