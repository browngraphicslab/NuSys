using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Components.Nodes.GroupNode;

namespace NuSysApp
{
    public sealed partial class GroupNodeTimelineView : AnimatableUserControl
    {
        private List<Tuple<FrameworkElement, DateTime>> _atomList;
        private ElementInstanceModel _nodeModel;
        private TimelineItemView _view;
        private GroupNodeTimelineViewModel _vm;
        private CompositeTransform _compositeTransform;
        public static Boolean onTimeline { get; set; }

        public GroupNodeTimelineView(GroupNodeTimelineViewModel viewModel)
        {
            this.InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;

            _atomList = new List<Tuple<FrameworkElement, DateTime>>();
            _vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;            

            // Composite transform
            _compositeTransform = new CompositeTransform
            {
                TranslateX = 0,
                TranslateY = 0,
                CenterX = this.ActualWidth / 2,
                CenterY = this.ActualHeight / 2,
                ScaleX = 1,
                ScaleY = 1
            };

            TimelineGrid.RenderTransform = _compositeTransform;

            // Panning / Zooming
            //TimelineGrid.ManipulationMode = ManipulationModes.All;
            TimelineGrid.ManipulationMode = ManipulationModes.None;
            TimelineGrid.ManipulationDelta += TimelineGrid_ManipulationDelta;
            TimelineGrid.ManipulationStarting += TimelineGrid_ManipulationStarting;
            TimelineGrid.PointerWheelChanged += TimelineGrid_PointerWheelChanged;
        }

        public void ClearTimelineChild()
        {
            foreach (var view in TimelinePanel.Children)
            {
                var blah = (TimelineItemView)view;
                blah.clearChild();
            }
            TimelinePanel.Children.Clear();
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;

            ClearTimelineChild(); // clear children

            foreach (var atom in _vm.AtomViewList)
            {
                var vm = (ElementInstanceViewModel)atom.DataContext; //access viewmodel
                vm.X = 0;
                vm.Y = 0;
                vm.CanEdit = EditStatus.No;
                vm.Height = 80;
                vm.Width = 130;
                _nodeModel = (ElementInstanceModel)vm.Model; // access model

                DateTime timeStamp = (DateTime)_nodeModel.GetMetaData("node_creation_date");

                Tuple<FrameworkElement, DateTime> tuple = new Tuple<FrameworkElement, DateTime>(atom, timeStamp);
                _atomList.Add(tuple);
            }
            _atomList = SortDateTime(_atomList); // sort tuple list

            foreach (var tuple in _atomList)
            {
                _view = new TimelineItemView(tuple.Item1, tuple.Item2);
                _view.Margin = new Thickness(20, 0, 20, 50);
                _view.VerticalAlignment = VerticalAlignment.Center;
                TimelinePanel.Children.Add(_view);
            }
            _atomList.Clear();
        }

        private void TimelineGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("Pointerwheelchanged");

            var compositeTransform = _compositeTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var cent = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(TimelineGrid).Position);

            var localPoint = tmpTranslate.Inverse.TransformPoint(cent);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - cent.X,
                worldPoint.Y - cent.Y);

            //...amd balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;
            var direction = Math.Sign((double)e.GetCurrentPoint(TimelineGrid).Properties.MouseWheelDelta);

            var zoomspeed = direction < 0 ? 0.95 : 1.05;//0.08 * direction;
            var translateSpeed = 10;

            var center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(TimelineGrid).Position);
            compositeTransform.ScaleX *= zoomspeed;
            compositeTransform.ScaleY *= zoomspeed;

            compositeTransform.CenterX = cent.X;
            compositeTransform.CenterY = cent.Y;

            _compositeTransform = compositeTransform;
            e.Handled = true;
        }

        private void TimelineGrid_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Container = TimelineGrid;
        }

        private void TimelineGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var compositeTransform = _compositeTransform;
            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(e.Position);

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift
            if (((FrameworkElement)e.OriginalSource).DataContext == TimelineGrid.DataContext)
            {
                compositeTransform.TranslateX += e.Delta.Translation.X;
                compositeTransform.TranslateY += e.Delta.Translation.Y;
            }

           _compositeTransform = compositeTransform;
        }


        private List<Tuple<FrameworkElement, DateTime>> SortDateTime(List<Tuple<FrameworkElement, DateTime>> list)
        {
            list.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            return list;
        }
    }
}
