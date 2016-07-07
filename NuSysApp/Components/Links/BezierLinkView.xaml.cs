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
using NuSysApp.Util;
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

        //    Annotation.Text = vm.Annotation;
            Annotation.TextChanged += AnnotationOnTextChanged;

 

            var linkController = (LinkElementController) vm.Controller;
            linkController.AnnotationChanged += LinkControllerOnAnnotationChanged;
          //  linkController.PositionChanged += LinkControllerOnPositionChanged;

            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };
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

            Canvas.SetZIndex(this, -10);

            var vm = (LinkViewModel)DataContext;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                if (vm.IsSelected)
                {
              
                    if (SessionController.Instance.SessionView.ModeInstance?.Mode != ModeType.EXPLORATION)
                    {
                        this.Annotation.Activate();
                        AnnotationContainer.Visibility = Visibility.Visible;
                    }
                    

                    if (((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain != null)
                    {

                        switch(((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Type)
                        {
                            case Region.RegionType.Rectangle:
                                //var imageRegionModel = (PdfNodeModel)SessionController.Instance.IdToControllers[model.OutAtomId].Model;
                                var imageRegionModel = ((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain;
                                var modelId = imageRegionModel.Id;
                                var model = ((LinkModel)(DataContext as LinkViewModel).Model);
                                ;
                                var list =
                                    SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Where(
                                        item => ((ElementViewModel)item.DataContext).Model.Id == SessionController.Instance.IdToControllers[model.OutAtomId].Model.Id);
                                var view = list?.First();
                                if (view == null)
                                {
                                    return;
                                }

     
                                //await ((ImageNodeView)view).onGoTo(imageRegionModel);

                                break;
                            default:
                                break;

                        }
                        //((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain.Select();
                        //this.JumpToLinkedTime();
                    }
                    /*

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
                        */
                    }
                }
                else
                {
                    if (Annotation.Text == "")
                    {
                        AnnotationContainer.Visibility = Visibility.Collapsed;
                    }

                    this.Annotation.DeActivate();
                    if (((LinkModel)(DataContext as LinkViewModel).Model).InFineGrain != null)
                    {
                          
                    }

                    if (((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod != null)
                    {
                        ((LinkModel)(DataContext as LinkViewModel).Model).RectangleMod.Model.Deselect();
                    }
                }
            }
        

        private void JumpToLinkedTime()
        {

            /*
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
            */
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
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

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

        private void Annotation_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BezierLink_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION)
            {
                // Handles exploration mode
                var vm = DataContext as LinkViewModel;
                Debug.Assert(vm != null);
                Canvas.SetZIndex(this, -10);
                SessionController.Instance.SessionView.Explore(vm);
            }
        }
    }
}