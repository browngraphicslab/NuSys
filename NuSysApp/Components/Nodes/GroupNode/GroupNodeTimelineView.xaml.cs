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
        private AtomModel _nodeModel;
        private TimelineItemView _view;
        private GroupNodeTimelineViewModel _vm;
        private string _sortBy;
        private string _viewBy;
        private Line _line;
        private Line _moveLine;
        private int _originalXPos;
        private int _moveToXPos;
        private HashSet<string> _metaDataButtons; // for sort metadata
        private HashSet<string> _metaDataViewButtons; // for view by metadata
        private int _custom;
        private double _groupNodeWidth;

        public GroupNodeTimelineView(GroupNodeTimelineViewModel viewModel)
        {
            this.InitializeComponent();
            _vm = viewModel;
            DataContext = _vm;
            NodeContainerModel model = (NodeContainerModel)_vm.Model;
            model.SizeChanged += GroupNode_SizeChanged;
            _sortBy = "node_creation_date";
            _viewBy = "node_creation_date";
            _custom = 0;

            _metaDataButtons = new HashSet<string>();
            _metaDataViewButtons = new HashSet<string>();

            _panelNodes = new List<FrameworkElement>();
            _atomList = new List<Tuple<FrameworkElement, Object>>();
            _vm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;

            TagBlock.Content = _sortBy;
            MetaExpandBlock.Content = _viewBy;
            SortBlock.Tapped += SortBlock_Tapped;
            MetaBlock.Tapped += MetaBlock_Tapped;

            // line for rearranging elements
            _moveLine = new Line()
            {
                Stroke = new SolidColorBrush(Colors.Coral)
            };

            // line across center for timeline
            _line = new Line()
            {
                X1 = -50000,
                X2 = 50000,
                Y1 = 92.5,
                Y2 = 92.5,
                StrokeThickness = 5,
                Stroke = new SolidColorBrush(Colors.Black)
            };
            TimelineCanvas.Children.Add(_line);
            TimelineCanvas.Children.Add(_moveLine);

            // Panning / Zooming
            //TimelineGrid.ManipulationMode = ManipulationModes.All;
            //TimelineGrid.ManipulationDelta += TimelineGrid_ManipulationDelta;
            //TimelineGrid.ManipulationStarting += TimelineGrid_ManipulationStarting;
            //TimelineGrid.PointerWheelChanged += TimelineGrid_PointerWheelChanged;
        }
        

        private void GroupNode_SizeChanged(object source, WidthHeightUpdateEventArgs e)
        {
            var rect = new RectangleGeometry();
            rect.Rect = new Rect(0, 0, e.Width - 40, e.Height - 40);
            TimelineGrid.Clip = rect;

            var rect1 = new RectangleGeometry();
            var width = e.Width <= 140 ? 0 : e.Width - 140; 
            rect1.Rect = new Rect(0, 0, width, 30);
            SortCanvas.Clip = rect1;

            var rect2 = new RectangleGeometry();
            rect2.Rect = new Rect(0, 0, width, 30);
            MetaCanvas.Clip = rect2;

            Canvas.SetTop(TimelinePanel, (e.Height - 130) / 2);
            Canvas.SetTop(_line, (e.Height - 130) / 2);
            _vm.CompositeTransform.CenterY = e.Height/2;
            _groupNodeWidth = e.Width;
        }

        private void MetaBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            MetaPanel.Opacity = MetaPanel.Opacity == 0 ? 1 : 0;
            MetaPanel.IsHitTestVisible = MetaPanel.Opacity == 0 ? false : true;
            MetaExpandBlock.Opacity = MetaExpandBlock.Opacity == 0 ? 1 : 0;
        }

        private void SortBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            TagPanel.Opacity = TagPanel.Opacity == 0 ? 1 : 0;
            TagPanel.IsHitTestVisible = TagPanel.Opacity == 0 ? false : true;
            TagBlock.Opacity = TagBlock.Opacity == 0 ? 1 : 0;
        }

        private void MetaViewButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button b1 = (Button)sender;
            MetaExpandBlock.Content = b1.Content.ToString();
            ChangeMetaDataTimeline(b1.Content.ToString());
            MetaExpandBlock.Opacity = 1;
            MetaPanel.Opacity = 0;
            MetaPanel.IsHitTestVisible = false;
        }

        private void MetaButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Button b1 = (Button)sender;
            TagBlock.Content = b1.Content.ToString();
            ResortTimeline(b1.Content.ToString());
            TagBlock.Opacity = 1;
            TagPanel.Opacity = 0;
            TagPanel.IsHitTestVisible = false;
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

        private void ChangeMetaDataTimeline(String dataName)
        {
            _viewBy = dataName;
            var index = 0;
            foreach (var node in _panelNodes)
            {
                var atom = node.FindVisualChild("TimelineNode").GetVisualChild(0);
                var vm = (AtomViewModel)atom.DataContext;
                var nodeModel = (AtomModel)vm.Model;
                TextBlock tb = (TextBlock)node.FindVisualChild("TextBlock");
                var text = nodeModel.GetMetaData(dataName).ToString();
                tb.Text = text;
                index++;
            }
        }

        #region Sort Timeline
        private void ResortTimeline(String dataName)
        {
            _sortBy = dataName; //need?
            _atomList.Clear();
            _panelNodes.Clear();
            ClearTimelineChild();
            foreach (var atom in _vm.AtomViewList)
            {
                var vm = (AtomViewModel)atom.DataContext; //access viewmodel
                vm.X = 0;
                vm.Y = 0;
                vm.CanEdit = Sendable.EditStatus.No;
                vm.Height = 80;
                vm.Width = 130;
                _nodeModel = (AtomModel)vm.Model; // access model

                Object metaData = _nodeModel.GetMetaData(dataName);
                Tuple<FrameworkElement, Object> tuple = new Tuple<FrameworkElement, Object>(atom, metaData);
                _atomList.Add(tuple);
            }

            // sort tuple list
            _atomList.Sort(TimelineSortComparer.sortTimeline());

            for (int i=0; i < _atomList.Count; i++)
            {
                if (dataName.Contains("custom"))
                {
                    Tuple<int, Object> tuple = (Tuple<int, Object>)_atomList.ElementAt(i).Item2;
                    Object secondItem = tuple.Item2 ?? "None";
                    _view = new TimelineItemView(_atomList.ElementAt(i).Item1, secondItem);
                }
                else
                {
                    Object secondItem = _atomList.ElementAt(i).Item2 ?? "None";
                    _view = new TimelineItemView(_atomList.ElementAt(i).Item1, secondItem);
                }

                _view.ManipulationMode = ManipulationModes.All;
                _view.ManipulationDelta += TimelineNode_ManipulationDelta;
                _view.ManipulationCompleted += TimelineNode_ManipulationCompleted;
                _view.ManipulationStarting += TimelineNode_ManipulationStarting;
                _view.Margin = new Thickness(20, 0, 20, 50);
                _view.VerticalAlignment = VerticalAlignment.Center;

                TimelinePanel.Children.Add(_view);
                _panelNodes.Add(_view);
                Canvas.SetLeft(_view, i * 174);
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
                var vm = (AtomViewModel)atom.DataContext;
                AtomModel model = (AtomModel)vm.Model;
                string[] keys = model.GetMetaDataKeys();
                foreach (var metadatatitle in keys)
                {
                    if (IsType(model.GetMetaData(metadatatitle)) && !_metaDataButtons.Contains(metadatatitle))
                    {
                        Button bb = new Button()
                        {
                            Content = metadatatitle,
                            Width = 100,
                            Height = 30
                        };
                        bb.Tapped += MetaButton_Tapped;
                        TagPanel.Children.Add(bb);
                        Button bb1 = new Button()
                        {
                            Content = metadatatitle,
                            Width = 100,
                            Height = 30
                        };
                        bb1.Tapped += MetaViewButton_Tapped;
                        MetaPanel.Children.Add(bb1);
                        _metaDataButtons.Add(metadatatitle);
                        _metaDataViewButtons.Add(metadatatitle);
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
            var nodeWidth = 174;
            var countLimit = _panelNodes.Count + 1;

            int index = (int)Math.Round(Canvas.GetLeft(item) / 174);
            int originalIndex = _originalXPos / 174;

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
                    for (int i = (_originalXPos / 174) + 1; i < _moveToXPos / 174; i++)
                    {
                        var element = _panelNodes.ElementAt(i);
                        Canvas.SetLeft(element, Canvas.GetLeft(element) - 174);
                        _panelNodes.RemoveAt(i);
                        _panelNodes.Insert(i - 1, element);
                    }
                    Canvas.SetLeft(item, _moveToXPos - 174);
                    Canvas.SetTop(item, 0);
                }
                else // moving from right to left
                {
                    for (int i = (_moveToXPos / 174); i < _originalXPos / 174; i++)
                    {
                        var element = _panelNodes.ElementAt(i);
                        Canvas.SetLeft(element, Canvas.GetLeft(element) + 174);
                    }
                    Canvas.SetLeft(item, _moveToXPos);
                    Canvas.SetTop(item, 0);
                    _panelNodes.RemoveAt(_originalXPos / 174);
                    _panelNodes.Insert(_moveToXPos / 174, item);
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
                    Height = 30
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
                var vm = (AtomViewModel)atom.DataContext;
                var nodeModel = (AtomModel)vm.Model;
                TextBlock tb = (TextBlock) node.FindVisualChild("TextBlock");
                var text = tb.Text;

                nodeModel.SetMetaData(TagBlock.Content.ToString(), new Tuple<int, Object>(index, text));
                index++;
            }           
        }
        #endregion

        #region Handlers for searchbox
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
                    _vm.CompositeTransform.TranslateX = (index * -174) + (TimelineGrid.Clip.Rect.Width - 174) / 2;
                    // Zoom in on center
                    _vm.CompositeTransform.CenterX = (TimelineGrid.Clip.Rect.Width / 2 - _vm.CompositeTransform.TranslateX);
                    _vm.CompositeTransform.ScaleX = prevZoom;
                    _vm.CompositeTransform.ScaleY = prevZoom;

                    // Animate element being search for
                    TimelineItemView element = (TimelineItemView)_panelNodes.ElementAt(index);
                    Grid timelineNode = (Grid)element.FindVisualChild("TimelineNode");
                    Storyboard storyboard = new Storyboard();
                    ColorAnimation animation = new ColorAnimation();
                    animation.From = Colors.Orange;
                    animation.To = Colors.Transparent;
                    storyboard.Children.Add(animation);
                    Storyboard.SetTarget(animation, timelineNode);
                    PropertyPath p = new PropertyPath("(timelineNode.Background).(SolidColorBrush.Color)");
                    Storyboard.SetTargetProperty(animation, p.Path);
                    storyboard.Begin();

                    contains = true;
                    break;
                }
                index++;
            }

            //foreach (var tuple in _atomList)
            //{
            //    if (tuple.Item2.ToString() == s)
            //    {
            //        var prevZoom = _vm.CompositeTransform.ScaleX;
            //        _vm.CompositeTransform.ScaleX = 1;
            //        _vm.CompositeTransform.ScaleY = 1;
            //        // Translate X to center
            //        _vm.CompositeTransform.TranslateX = (index * -174) + (TimelineGrid.Clip.Rect.Width - 174) / 2;
            //        // Zoom in on center
            //        _vm.CompositeTransform.CenterX = (TimelineGrid.Clip.Rect.Width / 2 - _vm.CompositeTransform.TranslateX);
            //        _vm.CompositeTransform.ScaleX = prevZoom;
            //        _vm.CompositeTransform.ScaleY = prevZoom;

            //        // Animate element being search for
            //        TimelineItemView element = (TimelineItemView)_panelNodes.ElementAt(index);
            //        Grid timelineNode = (Grid)element.FindVisualChild("TimelineNode");
            //        Storyboard storyboard = new Storyboard();
            //        ColorAnimation animation = new ColorAnimation();
            //        animation.From = Colors.Orange;
            //        animation.To = Colors.Transparent;
            //        storyboard.Children.Add(animation);
            //        Storyboard.SetTarget(animation, timelineNode);
            //        PropertyPath p = new PropertyPath("(timelineNode.Background).(SolidColorBrush.Color)");
            //        Storyboard.SetTargetProperty(animation, p.Path);
            //        storyboard.Begin();

            //        contains = true;
            //        break;
            //    }
            //    index++;
            //}

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

        private void TagPanel_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var buttonCount = _metaDataButtons.Count;
            var overflow = buttonCount*100 > _groupNodeWidth - 100;
            var leftOverflow = _vm.TranslateTransform.X + e.Delta.Translation.X > 0;
            var rightOverflow = 100 * buttonCount + _vm.TranslateTransform.X + 140 + e.Delta.Translation.X < _groupNodeWidth;

            if (!leftOverflow && overflow && !rightOverflow)
            {
                _vm.TranslateTransform.X += e.Delta.Translation.X;
            }
        }

        private void MetaPanel_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var buttonCount = _metaDataViewButtons.Count;
            var overflow = buttonCount * 100 > _groupNodeWidth - 100;
            var leftOverflow = _vm.MetaTranslateTransform.X + e.Delta.Translation.X > 0;
            var rightOverflow = 100 * buttonCount + _vm.MetaTranslateTransform.X + 140 + e.Delta.Translation.X < _groupNodeWidth;

            if (!leftOverflow && overflow && !rightOverflow)
            {
                _vm.MetaTranslateTransform.X += e.Delta.Translation.X;
            }
        }
    }

#region Comparer
    public class TimelineSortComparer : IComparer<Tuple<FrameworkElement,Object>>
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
                string str1 = (string) a.Item2;
                string str2 = (string) b.Item2;
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
                DateTime date1 = (DateTime) a.Item2;
                DateTime date2 = (DateTime) b.Item2;
                return date1.CompareTo(date2);
            }
            else if (a.Item2 is Tuple<int, Object>)
            {
                Tuple<int, Object> tuple1 = (Tuple<int,Object>) a.Item2;
                Tuple<int, Object> tuple2 = (Tuple<int, Object>) b.Item2;
                int first = tuple1.Item1;
                int second = tuple2.Item1;
                return first.CompareTo(second);
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
