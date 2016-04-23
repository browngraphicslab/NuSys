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
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLinkView : AnimatableUserControl
    {
        public BezierLinkView(LinkViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.PropertyChanged += OnPropertyChanged;

           Annotation.IsActivated = false;
           
          //  vm.Controller.LibraryElementModel.OnTitleChanged+= ControllerOnTitleChanged;
            vm.Controller.Disposed += OnDisposed;

            Annotation.SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                Rect.Width = args.NewSize.Width;
                Rect.Height = args.NewSize.Height;
            };

            Annotation.Text = vm.Annotation;
            Annotation.TextChanged += AnnotationOnTextChanged;

 

            var linkController = (LinkElementController) vm.Controller;
            linkController.AnnotationChanged += LinkControllerOnAnnotationChanged;
          //  linkController.PositionChanged += LinkControllerOnPositionChanged;

            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };

            object value;
            if (vm.LinkModel.InFGDictionary != null)
            {
                switch (SessionController.Instance.IdToControllers[vm.LinkModel.InAtomId].Model.ElementType)
                {
                    case ElementType.Image:
                        Debug.WriteLine("This links from a image with values " + vm.LinkModel.InFGDictionary.TryGetValue("x", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("y", out value));

                        break;
                    case ElementType.Text:
                        Debug.WriteLine("This links from a text with values " + vm.LinkModel.InFGDictionary.TryGetValue("x", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("y", out value));
                        break;
                    case ElementType.Audio:
                        Debug.WriteLine("This links from a text with values " + vm.LinkModel.InFGDictionary.TryGetValue("start", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("end", out value));
                        break;
                }
            }

            if (vm.LinkModel.OutFGDictionary != null)
            {
                switch (SessionController.Instance.IdToControllers[vm.LinkModel.OutAtomId].Model.ElementType)
                {
                    case ElementType.Image:
                        Debug.Write("to a image with values " + vm.LinkModel.InFGDictionary.TryGetValue("x", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("y", out value));

                        break;
                    case ElementType.Text:
                        Debug.Write("to a text with values " + vm.LinkModel.InFGDictionary.TryGetValue("x", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("y", out value));

                        break;
                    case ElementType.Audio:
                        Debug.Write("to a media with values " + vm.LinkModel.InFGDictionary.TryGetValue("start", out value) + ", " + vm.LinkModel.InFGDictionary.TryGetValue("end", out value));
                        break;
                }
            }
        }

        private void LinkControllerOnAnnotationChanged(string text)
        {
            var vm = (LinkViewModel)DataContext;
            Annotation.Text = text;
            if (text != "" || vm.IsSelected)
            {
                Annotation.Visibility = Visibility.Visible;
            }
            else
            {
                Annotation.Visibility = Visibility.Collapsed;
            }
        }

        private void AnnotationOnTextChanged(object source, string title)
        {
            var vm = (LinkViewModel) DataContext;
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
            vm.Controller.LibraryElementModel.OnTitleChanged -= ControllerOnTitleChanged;
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


            var vm = (LinkViewModel)DataContext;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                if (vm.IsSelected)
                {
                    this.Annotation.Activate();
                    AnnotationContainer.Visibility = Visibility.Visible;
                    Delete.Visibility = Visibility.Visible;
                    if (((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain != null)
                    {
                        ((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Select();
                        this.JumpToLinkedTime();
                    }
                    if (((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod != null)
                    {
                        LinkModel model = ((LinkModel)(DataContext as LinkViewModel).Model);
                        if (SessionController.Instance.IdToControllers[model.OutAtomId].Model.ElementType == ElementType.PDF)
                        {
                            PdfNodeModel pdfModel = (PdfNodeModel)SessionController.Instance.IdToControllers[model.OutAtomId].Model;
                            var modelId = pdfModel.Id;

                            var list =
                                SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                                    item => ((ElementViewModel)item.DataContext).Model.Id == modelId);
                            var view = list?.First();
                            if (view == null)
                            {
                                return;
                            }
                           
                            await ((PdfNodeView)view).onGoTo(((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod.PdfPageNumber);
                        }

                        ((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod.Model.Select();

                    }
                }
                else
                {
                    if (Annotation.Text == "")
                    {
                        AnnotationContainer.Visibility = Visibility.Collapsed;
                    }
                    Delete.Visibility = Visibility.Collapsed;
                    this.Annotation.DeActivate();
                    if (((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain != null)
                    {
                        ((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Deselect();
                    }

                    if (((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod != null)
                    {
                        ((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod.Model.Deselect();
                    }
                }
            }
        }

        private void JumpToLinkedTime()
        {

            if (((LinkModel) (DataContext as LinkViewModel).Model).InFineGrain.Start.TotalMilliseconds <
                ((LinkModel) (DataContext as LinkViewModel).Model).InFineGrain.End.TotalMilliseconds)
            {
                if (
                    SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model
                        .ElementType == ElementType.Video)
                {
                    (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model as
                    VideoNodeModel).Jump(((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Start);

                }
                else if (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model
                        .ElementType == ElementType.Audio)
                {
                    (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model as
                    AudioNodeModel).Jump(((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Start);
                }


            }
            else
            {
                if (
                    SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model
                        .ElementType == ElementType.Video)
                {
                    (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model as
                    VideoNodeModel).Jump(((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.End);

                }
                else if (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model
                        .ElementType == ElementType.Audio)
                {
                    (SessionController.Instance.IdToControllers[(DataContext as LinkViewModel).LinkModel.OutAtomId].Model as
                    AudioNodeModel).Jump(((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.End);

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

            var vm = (LinkViewModel)this.DataContext;

            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            curveInner.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curveInner.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);

            Canvas.SetLeft(btnDelete, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(btnDelete, anchor1.Y - distanceY / 2);

            Canvas.SetLeft(AnnotationContainer, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(AnnotationContainer, anchor1.Y - distanceY / 2 - Rect.ActualHeight * 1.5);

        }

        private void UpdateEndPoints()
        {
            var vm = (LinkViewModel)this.DataContext;
            var controller = (LinkElementController)vm.Controller;
            var anchor1 = new Point(controller.InElement.Model.X + controller.InElement.Model.Width / 2, controller.InElement.Model.Y + controller.InElement.Model.Height / 2);
            var anchor2 = new Point(controller.OutElement.Model.X + controller.OutElement.Model.Width / 2, controller.OutElement.Model.Y + controller.OutElement.Model.Height / 2);

            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;

            pathfigureInner.StartPoint = anchor1;
            curveInner.Point3 = anchor2;
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