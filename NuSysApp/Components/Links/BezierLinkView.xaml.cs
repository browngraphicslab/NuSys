﻿using System;
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
           
          //  vm.Controller.LibraryElementModel.OnTitleChanged+= ControllerOnTitleChanged;
            vm.Controller.Disposed += OnDisposed;

             Title.SizeChanged += delegate (object sender, SizeChangedEventArgs args)
            {
                Rect.Width = args.NewSize.Width;
                Rect.Height = args.NewSize.Height;
            };

            //    Annotation.Text = vm.Annotation;
            Title.TextChanged += TitleOnTextChanged;
 
            Canvas.SetZIndex(this, -2);//temporary fix to make sure events are propagated to nodes

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };

        }

        private void LinkControllerOnAnnotationChanged(string text)
        {
            var vm = (LinkViewModel)DataContext;
             Title.Text = text;
            if (text != "" || vm.IsSelected)
            {
                 Title.Visibility = Visibility.Visible;
            }
            else
            {
                 Title.Visibility = Visibility.Collapsed;
            }
        }

        private void TitleOnTextChanged(object sender, TextChangedEventArgs e)
        {

            var vm = DataContext as LinkViewModel;
            vm.UpdateTitle(Title.Text);
        }

        private void OnDisposed(object source)
        {
            var vm = (ElementViewModel)DataContext;
            vm.PropertyChanged -= OnPropertyChanged;
            vm.Controller.Disposed -= OnDisposed;
            DataContext = null;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            
            this.UpdateControlPoints();

            Canvas.SetZIndex(this, -10);

            var vm = DataContext as LinkViewModel;

            if (propertyChangedEventArgs.PropertyName == "IsSelected")
            {
                // if the vm is selected make sure title is read only in exploration mode
                if (vm?.IsSelected ?? false)
                {
                    Title.IsReadOnly = SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION;
                    
                }
            }
            else
            {
                (vm?.Model as LinkModel)?.RectangleModel?.Model.Deselect();
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

            Canvas.SetLeft(TitleContainer, anchor1.X - distanceX / 2 - Rect.ActualWidth / 2);
            Canvas.SetTop(TitleContainer, anchor1.Y - distanceY / 2 - Rect.ActualHeight * 1.5);

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

        private void BezierLink_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            var linkController = SessionController.Instance.LinkController.GetLinkLibraryElementController(vm.Model.LibraryId);
            SessionController.Instance.SessionView.ShowDetailView(linkController);
        }
    }
}