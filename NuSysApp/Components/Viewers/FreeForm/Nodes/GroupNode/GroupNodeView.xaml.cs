﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Messaging;
using MyToolkit.UI;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _isExpanded;

        private GroupNodeTimelineView timelineView;
        private GroupNodeExpandedView expandedView;
        private GroupNodeDataGridView dataGridView;
        private AreaNodeView freeFormView;
        private Image _dragItem;


        private Storyboard _circleAnim;
        private Storyboard _expandedAnim;
        private Storyboard _expandedListAnim; // for data grid view
        private Storyboard _timelineAnim;

        public GroupNodeView( GroupNodeViewModel vm)
        {
            RenderTransform = new CompositeTransform();
            InitializeComponent();
            DataContext = vm;
            Resizer.ManipulationDelta += ResizerOnManipulationDelta;

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                if (vm.ActiveCollectionViewType == CollectionElementModel.CollectionViewType.Timeline)
                    vm.ActiveCollectionViewType = CollectionElementModel.CollectionViewType.FreeForm;
                SwitchView(vm.ActiveCollectionViewType);
                PositionResizer();
            };

            (vm.Controller as ElementCollectionController).CollectionViewChanged += OnCollectionViewChanged;
            vm.Controller.Disposed += ControllerOnDisposed;

            //DefaultButton.AddHandler(TappedEvent,
            //    new TappedEventHandler(MenuDetailButton_Tapped), true);
            TimeLineButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            ListButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            FreeFormButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            EnterButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);

            SetUpToolsBtn();

        }

        private void SetUpToolsBtn()
        {
            _dragItem = new Image();
            xBtnTools.ManipulationMode = ManipulationModes.All;
            xBtnTools.ManipulationStarting += BtnAddNodeOnManipulationStarting;
            xBtnTools.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            xBtnTools.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            xBtnTools.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;
        }
        public async Task AddTool(Point pos, Size size)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;
            
            ToolFilterView filter = new ToolFilterView(pos.X, pos.Y, DataContext as GroupNodeViewModel);
            vm.AtomViewList.Add(filter);
            

            

        }
        private async void BtnAddNodeOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            GroupNodeCanvas.Children.Remove(_dragItem);
            var r = GroupNodeCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(args.Position.X, args.Position.Y));
            await AddTool(new Point(r.X, r.Y), new Size(300, 300));
        }

        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            t.TranslateX += args.Delta.Translation.X / zoom;
            t.TranslateY += args.Delta.Translation.Y / zoom;
            args.Handled = true;
        }

        private void BtnAddNodeOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            _dragItem.Opacity = 0.5;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Position.X - _dragItem.ActualWidth / 2;
            t.TranslateY += args.Position.Y - _dragItem.ActualHeight / 2;
            args.Handled = true;
        }

        private async void BtnAddNodeOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        {
            if (_dragItem != null && GroupNodeCanvas.Children.Contains(_dragItem))
                GroupNodeCanvas.Children.Remove(_dragItem);
            args.Container = GroupNodeCanvas;
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            var img = new Image();
            img.Opacity = 0;
            var t = new CompositeTransform();
            img.RenderTransform = new CompositeTransform();
            img.Source = bmp;
            _dragItem = img;
            GroupNodeCanvas.Children.Add(_dragItem);
            args.Handled = true;
        }
        private void OnCollectionViewChanged(object source, CollectionElementModel.CollectionViewType type)
        {
            SwitchView(type);
        }

        private void SwitchView(CollectionElementModel.CollectionViewType type)
        {
            switch (type)
            {
                case CollectionElementModel.CollectionViewType.FreeForm:
                    CreateFreeFormView();
                    break;
                case CollectionElementModel.CollectionViewType.List:
                    CreateDataGridView();
                    break;
                case CollectionElementModel.CollectionViewType.Timeline:
                    CreateTimelineView();
                    break;
            }
        }


        private async void CreateDataGridView()
        {
            if (dataGridView != null)
            {
                dataGridView.Visibility = Visibility.Visible;
                return;
            }
            var vm = (GroupNodeViewModel)DataContext;
            var dvm = new GroupNodeDataGridViewModel((ElementCollectionController)vm.Controller);
            await dvm.CreateChildren();
        
            dataGridView = new GroupNodeDataGridView(dvm);
            ExpandedGrid.Children.Add(dataGridView);
            dataGridView.Visibility = Visibility.Visible;
        }

        private async void CreateFreeFormView()
        {
            if (freeFormView != null)
            { 
                freeFormView.Visibility = Visibility.Visible;
                return;
            }
            var vm = (GroupNodeViewModel)DataContext;
            var fvm = new AreaNodeViewModel((ElementCollectionController)vm.Controller);
            await fvm.CreateChildren();
            freeFormView = new AreaNodeView(fvm);
            ExpandedGrid.Children.Add(freeFormView);
            freeFormView.Visibility = Visibility.Visible;
        }

        private async void CreateTimelineView()
        {

            if (timelineView != null)
            {
                timelineView.Visibility = Visibility.Visible;
                return;
            }
                
            var vm = (GroupNodeViewModel)DataContext;
            var tvm = new GroupNodeTimelineViewModel((ElementCollectionController)vm.Controller);
            await tvm.CreateChildren();
            timelineView = new GroupNodeTimelineView(tvm);
            await timelineView.ResortTimeline();
            ExpandedGrid.Children.Add(timelineView);
            
        }

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (GroupNodeViewModel) DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
            dataGridView = null;
            timelineView = null;
            freeFormView = null;
            nodeTpl.Dispose();
            DataContext = null;
          
        }

        private void ResizerOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel) DataContext;
            var dx = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var dy = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;
            vm.Controller.SetSize(vm.Width + dx, vm.Height + dy);

            PositionResizer();
            e.Handled = true;
        }

        private void PositionResizer()
        {
            var vm = (GroupNodeViewModel)DataContext;
            Canvas.SetLeft(Resizer, vm.Width - 50);
            Canvas.SetTop(Resizer, vm.Height - 50);
        }
        
        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(this, width, height);
            return r;
        }

        public AreaNodeView FreeFormView => freeFormView;

        private async void MenuDetailButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel)DataContext;
            var tb = (Button)sender;

            var controller = (ElementCollectionController) vm.Controller;

            if (timelineView != null)
                timelineView.Visibility = Visibility.Collapsed;
            if (dataGridView != null)
                dataGridView.Visibility = Visibility.Collapsed;
            if (freeFormView != null)
                freeFormView.Visibility = Visibility.Collapsed;


            if (tb.Name == "TimeLineButton")
            {
                controller.SetCollectionViewType(CollectionElementModel.CollectionViewType.Timeline);
            }
            else if (tb.Name == "ListButton")
            {
                controller.SetCollectionViewType(CollectionElementModel.CollectionViewType.List);
            }
            else if (tb.Name == "FreeFormButton")
            {
                controller.SetCollectionViewType(CollectionElementModel.CollectionViewType.FreeForm);
            }
            else if (tb.Name == "EnterButton")
            {
                var id = vm.Controller.LibraryElementModel.LibraryElementId;
                var content = SessionController.Instance.ContentController.GetContent(id);
                if (content != null && content.Type == ElementType.Collection)
                {
                    List<Message> messages = new List<Message>();
                    await Task.Run(async delegate
                    {
                        messages =
                            await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(id);
                    });
                    Visibility = Visibility.Collapsed;
                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                            new UnsubscribeFromCollectionRequest(
                                SessionController.Instance.ActiveFreeFormViewer.ContentId));
                    await SessionController.Instance.SessionView.LoadWorkspaceFromServer(messages, id);
                }
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel)DataContext;
            vm.Controller.RequestDelete();
        }


    }
}
