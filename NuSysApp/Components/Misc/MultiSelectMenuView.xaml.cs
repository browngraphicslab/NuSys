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
using Microsoft.Graphics.Canvas.Geometry;
using NetTopologySuite.Geometries;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MultiSelectMenuView : UserControl
    {
        public InkStroke Stroke { get; set; }

        public static Color SelectedColor { get; set; }

        public bool Finite { get; set; }
        public List<Windows.Foundation.Point> Points { get; set; }

        public MultiSelectMenuView()
        {
            this.InitializeComponent();
            DataContext = new object();
            DeleteButton.Click += DeleteButtonOnClick;
            GroupButton.Click += GroupButtonOnClick;
            AdornmentButton.Tapped += AdormentButtonClick;

            SelectedColor = Colors.Black;
            Finite = false;
            Points = new List<Windows.Foundation.Point>();
        }

        public void Show()
        {
            ColorPicker.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Visible;
        }

        private void AdormentButtonClick(object sender, TappedRoutedEventArgs e)
        {
            ColorPicker.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }

        private void GroupButtonOnClick(object sender, RoutedEventArgs e)
        {
            GroupSettings.Visibility = Visibility.Visible;
            Buttons.Visibility = Visibility.Collapsed;
        }

        private void GroupSettingsXOnClick(object sender, RoutedEventArgs e)
        {
            GroupSettings.Visibility = Visibility.Collapsed;
        }

        private async void CreateGroupButtonOnClick(object sender, RoutedEventArgs routedEventArgs)

        /// <summary>
        /// Creates a collection based on the nodes enclosed in the ink stroke. TODO: refactor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="routedEventArgs"></param>
        {
            var selections = SessionController.Instance.ActiveFreeFormViewer.Selections;
            if (selections.Count == 0) {
                Visibility = Visibility.Collapsed;
                return;
            }
            //var bb = Geometry.NodesToBoudingRect(selections.Where(v =>  (v is ElementViewModel)).Select(item=> item as ElementViewModel).ToList());       
            var bb = Stroke.BoundingRect;

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            // TODO: add the graph/chart
            var contentId = SessionController.Instance.GenerateId();
            var newCollectionId = SessionController.Instance.GenerateId();

            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;

            // Removes the ink from the canvas
            var req = InkStorage.CreateRemoveInkRequest(new InkWrapper(Stroke, "ink"));
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(Stroke);

            var deleteMsg = new Message();
            deleteMsg["contentId"] = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            var model = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Remove(req.Item2);
            deleteMsg["inklines"] = new HashSet<string>(model.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(deleteMsg));

            // make a pointcollection that will be the "shape" property of the collection (use pointcollection or list?)
            var inkpoints = Stroke.GetInkPoints().ToArray();
            Points = new List<Windows.Foundation.Point>();
            foreach (var i in inkpoints)
            {
                Points.Add(i.Position);
            }

            if (FiniteCheck.IsChecked.Value != null)
            {
                Finite = FiniteCheck.IsChecked.Value;
            }

            if (ShapeCheck.IsChecked.Value != true)
            {
                Points.Clear();
            }

            var m = new Message();
            m["id"] = contentId;
            m["data"] = "";
            m["type"] = ElementType.Collection.ToString();
            m["title"] = "new collection, " + Finite.ToString() + " " + Points.Count;
            m["finite"] = Finite;
            m["shape_points"] = Points;
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(m));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SubscribeToCollectionRequest(contentId));

            //here, settings should also be passed in as parameters
            var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(bb.X, bb.Y, contentId, Finite, Points, bb.Width, bb.Height, newCollectionId, CollectionElementModel.CollectionViewType.FreeForm);
            
            foreach (var vm in selections.ToArray())
            {
                if (vm is ElementViewModel)
                {
                    var elementViewModel = vm as ElementViewModel;

                    var libraryElementModel = elementViewModel.Controller.LibraryElementModel;
                    var dict = new Message();
                    dict["title"] = libraryElementModel?.Title;
                    dict["width"] = elementViewModel.Width;
                    dict["height"] = elementViewModel.Height;
                    dict["type"] = libraryElementModel.Type.ToString();
                    dict["x"] = elementViewModel.Transform.TranslateX - bb.X + Constants.MaxCanvasSize / 2.0;
                    dict["y"] = elementViewModel.Transform.TranslateY - bb.Y + Constants.MaxCanvasSize / 2.0;
                    dict["contentId"] = libraryElementModel.LibraryElementId;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = controller.LibraryElementModel.LibraryElementId;

                    if (elementViewModel is PdfNodeViewModel)
                    {
                        dict["page"] = (elementViewModel as PdfNodeViewModel).CurrentPageNumber;
                    }

                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                    elementViewModel.Controller.RequestDelete();
                }
                // do something with links here
                
            }

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
            m["type"] = ElementType.Area.ToString();
            m["points"] = Stroke.GetInkPoints();
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.AddAdorment(Stroke, SelectedColor);

          
            var request = InkStorage.CreateRemoveInkRequest(new InkWrapper(Stroke, "ink"));
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(Stroke);
            //SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request.Item1);

            var deleteMsg = new Message();
            deleteMsg["contentId"] = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            var model = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Remove(request.Item2);
            deleteMsg["inklines"] = new HashSet<string>(model.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(deleteMsg));

            Visibility = Visibility.Collapsed;
        }
    }
}
