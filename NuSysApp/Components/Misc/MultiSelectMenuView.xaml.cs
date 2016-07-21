using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MultiSelectMenuView : UserControl
    {
        public InkStroke Stroke { get; set; }

        public static Color SelectedColor { get; set; }

        public MultiSelectMenuView()
        {
            this.InitializeComponent();
            DataContext = new object();
            DeleteButton.Click += DeleteButtonOnClick;
            GroupButton.Click += GroupButtonOnClick;
            AdornmentButton.Tapped += AdormentButtonClick;

            SelectedColor = Colors.Black;
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

        private async void GroupButtonOnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var selections = SessionController.Instance.ActiveFreeFormViewer.Selections;
            if (selections.Count == 0) {
                Visibility = Visibility.Collapsed;
                return;
            }
            var bb = Geometry.NodesToBoudingRect(selections.Where(v =>  (v is ElementViewModel)).Select(item=> item as ElementViewModel).ToList());       

            var metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;

            // TODO: add the graph/chart
            var contentId = SessionController.Instance.GenerateId();
            var newCollectionId = SessionController.Instance.GenerateId();

            var t = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;

            var elementMsg = new Message();
            elementMsg["metadata"] = metadata;
            elementMsg["width"] = bb.Width;
            elementMsg["height"] = bb.Height;
            elementMsg["locationX"] = t.TranslateX;
            elementMsg["locationY"] = t.TranslateY;
            elementMsg["centerX"] = t.CenterX;
            elementMsg["centerY"] = t.CenterY;
            elementMsg["zoom"] = t.ScaleX;
            elementMsg["x"] = bb.X;
            elementMsg["y"] = bb.Y;
            elementMsg["contentId"] = contentId;
            elementMsg["type"] = ElementType.Collection;
            elementMsg["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            elementMsg["id"] = newCollectionId;
            elementMsg["color"] = Colors.Red;
            elementMsg["points"] = Stroke.GetInkPoints();

            // Removes the ink from the canvas
            var req = InkStorage.CreateRemoveInkRequest(new InkWrapper(Stroke, "ink"));
            SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.RemoveStroke(Stroke);

            var deleteMsg = new Message();
            deleteMsg["contentId"] = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId;
            var model = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel as CollectionLibraryElementModel;
            model.InkLines.Remove(req.Item2);
            deleteMsg["inklines"] = new HashSet<string>(model.InkLines);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(deleteMsg));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Collection, "Search Results"));

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new SubscribeToCollectionRequest(contentId));

            

            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(elementMsg)); 

            var controller = await StaticServerCalls.PutCollectionInstanceOnMainCollection(bb.X, bb.Y, contentId, bb.Width, bb.Height, newCollectionId, CollectionElementModel.CollectionViewType.FreeForm);

           
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
