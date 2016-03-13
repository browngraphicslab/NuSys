using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using MyToolkit.UI;
using MyToolkit.Utilities;
using NuSysApp.Components.Nodes.GroupNode;

namespace NuSysApp
{
    public sealed partial class GroupNodeTimelineView : AnimatableUserControl
    {
        private List<Tuple<FrameworkElement, Object>> _atomList;
        private List<FrameworkElement> _panelNodes;
        private ElementModel _nodeModel;
        private TimelineItemView _view;
        private GroupNodeTimelineViewModel _vm;
        private string _sortBy;
        private Line _moveLine;
        private int _originalXPos;
        private int _moveToXPos;
        private HashSet<string> _metaDataButtons;
        private int _custom;
        private const int TimelineNodeWidth = 140;

        public GroupNodeTimelineView(GroupNodeTimelineViewModel viewModel)
        {
            this.InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;
            var model = _vm.Model;
            _vm.Controller.SizeChanged += GroupNode_SizeChanged;
            _sortBy = "node_creation_date";
            _custom = 0;

            Canvas.SetTop(TimelinePanel, (model.Height - 80) / 2);

            _metaDataButtons = new HashSet<string>();
            _panelNodes = new List<FrameworkElement>();
            _atomList = new List<Tuple<FrameworkElement, Object>>();
            _vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;

            TagBlock.Content = _sortBy;
            TagBlock.Tapped += TagBlock_Tapped;

            // line for rearranging elements
            _moveLine = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Coral)
            };

            // Panning / Zooming
            TimelineGrid.ManipulationMode = ManipulationModes.All;
            TimelineGrid.ManipulationDelta += TimelineGrid_ManipulationDelta;
            TimelineGrid.ManipulationStarting += TimelineGrid_ManipulationStarting;
            TimelineGrid.PointerWheelChanged += TimelineGrid_PointerWheelChanged;
        }

        private void GroupNode_SizeChanged(object source, double width, double height)
        {
            var rect = new RectangleGeometry();
            rect.Rect = new Rect(0, 0, width, height);
            TimelineGrid.Clip = rect;

            Canvas.SetTop(TimelinePanel, (height - 80) / 2);
            _vm.CompositeTransform.CenterY = height / 2;
        }

        private void TagBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TagPanel.Opacity = TagPanel.Opacity == 0 ? 1 : 0;
        }

        private void MetaButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button b1 = (Button)sender;
            TagBlock.Content = b1.Content.ToString();
            ResortTimeline(b1.Content.ToString());
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

        public static bool IsType(object value)
        {
            return IsNum(value)
                   || value is string
                   || value is DateTime;
        }
        public static bool IsNum(object value)
        {
            return value is sbyte
                   || value is byte
                   || value is short
                   || value is ushort
                   || value is int
                   || value is uint
                   || value is long
                   || value is ulong
                   || value is float
                   || value is double
                   || value is decimal;
        }

        #region Sort Timeline
        private void ResortTimeline(String dataName)
        {
            _sortBy = dataName;
            _atomList.Clear();
            _panelNodes.Clear();
            ClearTimelineChild();
            foreach (var atom in _vm.AtomViewList)
            {
                var vm = (ElementViewModel)atom.DataContext; //access viewmodel
                vm.Height = 80;
                vm.Width = 130;
                _nodeModel = (ElementModel)vm.Model; // access model

                Object metaData = _nodeModel.GetMetaData(dataName);
                Tuple<FrameworkElement, Object> tuple = new Tuple<FrameworkElement, Object>(atom, metaData);
                _atomList.Add(tuple);
            }

            // sort tuple list
            _atomList.Sort(TimelineSortComparer.sortTimeline());

            for (int i = 0; i < _atomList.Count; i++)
            {
                Object secondItem = _atomList.ElementAt(i).Item2 ?? "None";
                _view = new TimelineItemView(_atomList.ElementAt(i).Item1, secondItem);

                //_view.ManipulationMode = ManipulationModes.All;
                //_view.ManipulationDelta += TimelineNode_ManipulationDelta;
                //_view.ManipulationCompleted += TimelineNode_ManipulationCompleted;
                //_view.ManipulationStarting += TimelineNode_ManipulationStarting;
                _view.VerticalAlignment = VerticalAlignment.Center;

                TimelinePanel.Children.Add(_view);
                _panelNodes.Add(_view);
                Canvas.SetLeft(_view, i * TimelineNodeWidth);
            }
            _vm.DataList = _atomList;
        }

        private async void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems == null)
                return;
            ResortTimeline(_sortBy);
            AddMetaDataButtons(e.NewItems);
        }

        private void AddMetaDataButtons(IList list)
        {
            foreach (var item in list)
            {
                var atom = (FrameworkElement)item;
                var vm = (ElementViewModel)atom.DataContext;
                var model = (ElementModel)vm.Model;
                string[] keys = model.GetMetaDataKeys();

                Debug.WriteLine("key length: " + keys.Length);
                foreach (var metadatatitle in keys)
                {
                    Debug.WriteLine(metadatatitle);
                }

                foreach (var metadatatitle in keys)
                {
                    if (IsType(model.GetMetaData(metadatatitle)) && !_metaDataButtons.Contains(metadatatitle))
                    {
                        Button bb = new Button()
                        {
                            Content = metadatatitle,
                            Width = 100,
                        };
                        bb.Tapped += MetaButton_Tapped;
                        TagPanel.Children.Add(bb);
                        _metaDataButtons.Add(metadatatitle);
                    }
                }
            }
        }
        #endregion

        #region Timeline Rearrange
        private void TimelineNode_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            TimelineItemView item = (TimelineItemView)sender;
            _originalXPos = (int)Canvas.GetLeft(item);
        }

        private void TimelineNode_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            TimelineItemView item = (TimelineItemView)sender;
            Canvas.SetTop(item, Canvas.GetTop(item) + e.Delta.Translation.Y);
            Canvas.SetLeft(item, Canvas.GetLeft(item) + e.Delta.Translation.X);
            var nodeWidth = TimelineNodeWidth;
            var countLimit = _panelNodes.Count + 1;

            int index = (int)Math.Round(Canvas.GetLeft(item) / TimelineNodeWidth);
            int originalIndex = _originalXPos / TimelineNodeWidth;

            if (Canvas.GetLeft(item) % nodeWidth <= 10 &&
                Canvas.GetLeft(item) >= 0 &&
                Canvas.GetLeft(item) <= countLimit * nodeWidth &&
                index != originalIndex + 1)
            {
                _moveToXPos = (int)index * nodeWidth;
                _moveLine.X1 = index * nodeWidth;
                _moveLine.X2 = index * nodeWidth;
                _moveLine.Y1 = Canvas.GetTop(TimelinePanel);
                _moveLine.Y2 = Canvas.GetTop(TimelinePanel) + 130;
                _moveLine.StrokeThickness = 3;
            }
        }

        private void TimelineNode_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            TimelineItemView item = (TimelineItemView)sender;
            _moveLine.StrokeThickness = 0;

            if (_moveToXPos == _originalXPos) // same place
            {
                Canvas.SetLeft(item, _originalXPos);
                Canvas.SetTop(item, 0);
            }
            else
            {
                if (_originalXPos < _moveToXPos) // moving from left to right
                {
                    for (int i = (_originalXPos / TimelineNodeWidth) + 1; i < _moveToXPos / TimelineNodeWidth; i++)
                    {
                        var element = _panelNodes.ElementAt(i);
                        Canvas.SetLeft(element, Canvas.GetLeft(element) - TimelineNodeWidth);
                        _panelNodes.RemoveAt(i);
                        _panelNodes.Insert(i - 1, element);
                    }
                    Canvas.SetLeft(item, _moveToXPos - TimelineNodeWidth);
                    Canvas.SetTop(item, 0);
                }
                else // moving from right to left
                {
                    for (int i = (_moveToXPos / TimelineNodeWidth); i < _originalXPos / TimelineNodeWidth; i++)
                    {
                        var element = _panelNodes.ElementAt(i);
                        Canvas.SetLeft(element, Canvas.GetLeft(element) + TimelineNodeWidth);
                    }
                    Canvas.SetLeft(item, _moveToXPos);
                    Canvas.SetTop(item, 0);
                    _panelNodes.RemoveAt(_originalXPos / TimelineNodeWidth);
                    _panelNodes.Insert(_moveToXPos / TimelineNodeWidth, item);
                }
            }

            String custom = "custom" + _custom;
            if (!TagBlock.Content.ToString().Contains("custom"))
            {
                // if not custom, create a new custom
                // add metadata button
                Button bb = new Button()
                {
                    Content = custom,
                    Width = 100,
                };
                bb.Tapped += MetaButton_Tapped;
                TagPanel.Children.Add(bb);
                _metaDataButtons.Add(custom);

                // Change view to custom
                TagBlock.Content = custom;

                _custom++;
            }

            // set new metadata
            var index = 0;
            foreach (var node in _panelNodes)
            {
                var atom = node.FindVisualChild("TimelineNode").GetVisualChild(0);
                var vm = (ElementViewModel)atom.DataContext;
                var nodeModel = (ElementModel)vm.Model;
                nodeModel.SetMetaData(custom, index);
                index++;
            }
        }
        #endregion

        #region Searchbox
        private void AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                //sender.ItemsSource = blah;
            }
        }

        private void AutoSuggestBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                MoveToSearch(args.ChosenSuggestion.ToString());
            }
            else
            {
                MoveToSearch(args.QueryText);
            }
        }

        private void MoveToSearch(String s)
        {
            //TODO: multiple search terms
            var index = 0;
            Boolean contains = false;

            //based on if custom or not
            foreach (var view in _panelNodes)
            {
                TextBlock tb = (TextBlock)view.FindVisualChild("TextBlock");
                if (tb.Text.Equals(s))
                {
                    var prevZoom = _vm.CompositeTransform.ScaleX;
                    _vm.CompositeTransform.ScaleX = 1;
                    _vm.CompositeTransform.ScaleY = 1;
                    // Translate X to center
                    _vm.CompositeTransform.TranslateX = (index * -TimelineNodeWidth) + (TimelineGrid.Clip.Rect.Width - TimelineNodeWidth) / 2;
                    // Zoom in on center
                    _vm.CompositeTransform.CenterX = (TimelineGrid.Clip.Rect.Width / 2 - _vm.CompositeTransform.TranslateX);
                    _vm.CompositeTransform.ScaleX = prevZoom;
                    _vm.CompositeTransform.ScaleY = prevZoom;

                    // Animate element being search for
                    TimelineItemView element = (TimelineItemView)_panelNodes.ElementAt(index);
                    Grid timelineNode = (Grid)element.FindVisualChild("TimelineNode");

                    Anim.FromTo(timelineNode, "Opacity", 0, 1, 1200, null);

                    contains = true;
                    break;
                }
                index++;
            }

            if (!contains)
            {
                var messageDialog = new MessageDialog("Search term doesn't exist");
                messageDialog.Commands.Add(new UICommand("Ok"));
                messageDialog.ShowAsync();
            }

        }

        private void AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem.ToString();
        }
        #endregion

        #region Pan/Zoom for timeline
        private void TimelineGrid_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            var compositeTransform = _vm.CompositeTransform;
            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var cent = e.GetCurrentPoint(TimelineCanvas).Position;
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

            var direction = Math.Sign((double)e.GetCurrentPoint(TimelineCanvas).Properties.MouseWheelDelta);
            var zoomspeed = direction < 0 ? 0.95 : 1.05;//0.08 * direction;
            var translateSpeed = 10;
            compositeTransform.ScaleX *= zoomspeed;
            compositeTransform.ScaleY *= zoomspeed;

            compositeTransform.CenterX = cent.X;
            _vm.CompositeTransform = compositeTransform;
        }

        private void TimelineGrid_ManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            e.Container = TimelineCanvas;
        }

        private void TimelineGrid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var compositeTransform = _vm.CompositeTransform;
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

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;

            //And consider a translational shift
            if (((FrameworkElement)e.OriginalSource).DataContext == TimelineGrid.DataContext)
            {
                compositeTransform.TranslateX += e.Delta.Translation.X;
            }
            //Debug.WriteLine(_vm.CompositeTransform.TranslateX - _vm.TranslateTransform.X);
            Debug.WriteLine(compositeTransform.TranslateX);
            _vm.CompositeTransform = compositeTransform;
        }
        #endregion
    }

    #region Comparer
    public class TimelineSortComparer : IComparer<Tuple<FrameworkElement, Object>>
    {
        public int Compare(Tuple<FrameworkElement, Object> a, Tuple<FrameworkElement, Object> b)
        {
            if (a.Item2 == null && b.Item2 == null)
            {
                return 0;
            }
            if (a.Item2 == null)
            {
                return 1;
            }
            if (b.Item2 == null)
            {
                return -1;
            }

            if (a.Item2 is String)
            {
                string str1 = (string)a.Item2;
                string str2 = (string)b.Item2;
                return str1.CompareTo(str2);
            }
            else if (GroupNodeTimelineView.IsNum(a.Item2))
            {
                double double1;
                double double2;
                Double.TryParse(a.Item2.ToString(), out double1);
                Double.TryParse(b.Item2.ToString(), out double2);
                return double1.CompareTo(double2);
            }
            else if (a.Item2 is DateTime)
            {
                DateTime date1 = (DateTime)a.Item2;
                DateTime date2 = (DateTime)b.Item2;
                return date1.CompareTo(date2);
            }
            else
            {
                Debug.WriteLine("weird object - ERROR: " + a.Item2.GetType());
                return 1;
            }
        }

        public static IComparer<Tuple<FrameworkElement, Object>> sortTimeline()
        {
            return new TimelineSortComparer();
        }
    }
    #endregion
}