using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using NuSysApp.Controller;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PresentationLinkView : AnimatableUserControl
    {
        public PresentationLinkView(LinkViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;
            var color = Application.Current.Resources["color4"] as SolidColorBrush;
            vm.Color = color;

            //  vm.Controller.LibraryElementModel.OnTitleChanged+= ControllerOnTitleChanged;
            vm.Controller.Disposed += OnDisposed;

            var linkController = (LinkElementController)vm.Controller;
            linkController.AnnotationChanged += LinkControllerOnAnnotationChanged;
            //  linkController.PositionChanged += LinkControllerOnPositionChanged;

            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };
        }

        private void LinkControllerOnPositionChanged(object source, double d, double d1, double dx, double dy)
        {
            throw new NotImplementedException();
        }

        private void LinkControllerOnAnnotationChanged(string text)
        {

        }

        private void AnnotationOnTextChanged(object source, string title)
        {
            var vm = (LinkViewModel)DataContext;
            var controller = (LinkElementController)vm.Controller;
            controller.SetAnnotation(title);
        }

        private void ControllerOnTitleChanged(object source, string title)
        {
            //     Annotation.Text = title;
            //     AnnotationContainer.Visibility = title == "" ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OnDisposed(object source)
        {
            var vm = (ElementViewModel)DataContext;
            vm.PropertyChanged -= OnPropertyChanged;
            vm.Controller.Disposed -= OnDisposed;
            vm.Controller.LibraryElementController.TitleChanged -= ControllerOnTitleChanged;
            //var linkController = (LinkElementController)vm.Controller;
            // linkController.AnnotationChanged -= LinkControllerOnAnnotationChanged;
            DataContext = null;
        }

        private void UpdateText()
        {
            //  var vm = DataContext as LinkViewModel;
            //  vm.Controller.LibraryElementModel.SetTitle(Annotation.Text);           
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {

            this.UpdateControlPoints();

            // get the link view model as the data context
            var vm = DataContext as LinkViewModel;
            var linkModel = (LinkModel)vm?.Model;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {

                // if the link is selected
                if (vm?.IsSelected ?? false)
                {                  
                    if (linkModel.RectangleModel != null)
                    {
                        // if the presentation link goes to a PdfNodeModel make sure we got to the right page
                        var pdfNodeModel = SessionController.Instance.IdToControllers[linkModel.OutAtomId].Model as PdfNodeModel;
                        if (pdfNodeModel != null)
                        {
                            // get the pdfNodeModel id
                            var modelId = pdfNodeModel.Id;

                            // get the pdfNodeView using the pdf Model id
                            var list =
                                SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                                    item => (item.DataContext as ElementViewModel)?.Model.Id == modelId);
                            var view = list?.First() as PdfNodeView;

                            // follow the link
                            if (view != null)
                            {
                                await view.onGoTo(linkModel.RectangleModel.PdfPageNumber);
                            }
                            
                        }

                        linkModel.RectangleModel.Model.Select();

                    }
                }
                else
                {
                    linkModel?.RectangleModel?.Model.Deselect();
                }
            }
        }


        private void OnAtomPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }

        /// <summary>
        /// Updates the location of the bezier controlpoints. 
        /// Do not call this method outside of this class.
        /// </summary>
        private void UpdateControlPoints()
        {
            this.UpdateEndPoints();
            this.UpdateArrow();


            var vm = (LinkViewModel)this.DataContext;

            var controller = (LinkElementController)vm.Controller;

            if (controller.InElement == null)
                return;
            if (controller.OutElement == null)
                return;

            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
        }

        private void UpdateArrow()
        {
            var center = new Point((pathfigure.StartPoint.X + curve.Point3.X) / 2.0, (pathfigure.StartPoint.Y + curve.Point3.Y) / 2.0);
            var xDiff = curve.Point3.X - pathfigure.StartPoint.X;
            var yDiff = curve.Point3.Y - pathfigure.StartPoint.Y;
            var angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            var tranformGroup = new TransformGroup();
            tranformGroup.Children.Add(new RotateTransform { Angle = angle, CenterX = 20, CenterY = 20 });
            tranformGroup.Children.Add(new TranslateTransform { X = center.X - 20, Y = center.Y - 20 });

            arrow.RenderTransform = tranformGroup;
        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            if (controller.InElement == null)
                return;
            if (controller.OutElement == null)
                return;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;
        }

        private async void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            await controller.RequestDelete();
        }

        private void Annotation_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}