using System;
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
                var dvm = new GroupNodeDataGridViewModel((ElementCollectionController) vm.Controller);
                await dvm.CreateChildren();
                dataGridView = new GroupNodeDataGridView(dvm);
                ExpandedGrid.Children.Add(dataGridView);

                dataGridView.Visibility = Visibility.Visible;
           //     freeFormView.Visibility = Visibility.Collapsed;

                PositionResizer();
            };

            vm.Controller.Disposed += ControllerOnDisposed;

            //DefaultButton.AddHandler(TappedEvent,
            //    new TappedEventHandler(MenuDetailButton_Tapped), true);
            TimeLineButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            ListButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);
            FreeFormButton.AddHandler(TappedEvent,
                new TappedEventHandler(MenuDetailButton_Tapped), true);


   

        }

        private void ControllerOnDisposed(object source)
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

            if (timelineView != null)
                timelineView.Visibility = Visibility.Collapsed;
            if (dataGridView != null)
                dataGridView.Visibility = Visibility.Collapsed;
            if (freeFormView != null)
                freeFormView.Visibility = Visibility.Collapsed;


            if (tb.Name == "TimeLineButton")
            {
                if (timelineView == null)
                {
                    var tvm = new GroupNodeTimelineViewModel((ElementCollectionController) vm.Controller);
                    await tvm.CreateChildren();
                    timelineView = new GroupNodeTimelineView(tvm);
                    await timelineView.ResortTimeline();
                    ExpandedGrid.Children.Add(timelineView);
                }
                timelineView.Visibility = Visibility.Visible;
            }
            else if (tb.Name == "ListButton")
            {
                dataGridView.Visibility = Visibility.Visible;
            }
            else if (tb.Name == "FreeFormButton")
            {
                if (freeFormView == null)
                {
                    var fvm = new AreaNodeViewModel((ElementCollectionController) vm.Controller);
                    await fvm.CreateChildren();
                    freeFormView = new AreaNodeView(fvm);
                    ExpandedGrid.Children.Add(freeFormView);
                }
                freeFormView.Visibility = Visibility.Visible;
            }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel)DataContext;
            vm.Controller.RequestDelete();
        }


    }
}
