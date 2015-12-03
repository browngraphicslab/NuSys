using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class IntersectGroupNodeMode : AbstractWorkspaceViewMode
    {
        private GroupTagNodeView _intersectedGroupNode;
        private GroupTagNodeView _initialGroupNode;
        private bool _isIntersecting;
        private List<UserControl> _searchList = new List<UserControl>();
        private GroupTagNodeView _generatedGroupTag;

        private readonly DispatcherTimer _timer = new DispatcherTimer();

        public IntersectGroupNodeMode(WorkspaceView view) : base(view)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += delegate
            {
                _timer.Stop();

                ((GroupTagNodeViewModel)_initialGroupNode.DataContext).EnableChildMove = false;
                ((GroupTagNodeViewModel)_intersectedGroupNode.DataContext).EnableChildMove = false;

                var vm = (GroupTagNodeViewModel)_initialGroupNode.DataContext;
                var n1 = vm.Title;
                var otherVm = (GroupTagNodeViewModel)_intersectedGroupNode.DataContext;
                var n2 = otherVm.Title;
                var props = new Dictionary<string, string>();
                props["isTemporary"] = "True";

                var children0 = vm.Model.Children.Values.ToList();
                var children1 = otherVm.Model.Children.Values.ToList();
                var intersection = new List<NodeModel>();
                foreach (var child0 in children0)
                {
                    var c0 = (NodeModel) child0;
                    foreach (var child1 in children1)
                    {
                        var c1 = (NodeModel) child1;
                        if (c0 == c1 || c0.GetMetaData("visualCopyOf") == c1.GetMetaData("visualCopyOf") ||
                            c1.GetMetaData("visualCopyOf") == c0.GetMetaData("visualCopyOf"))
                        {
                            intersection.Add(c0);
                            intersection.Add(c1);
                        }
                    }
                }

                var initialTransform = (CompositeTransform)_initialGroupNode.RenderTransform;
                var intersectedTransform = (CompositeTransform)_intersectedGroupNode.RenderTransform;

                var callback = new Action<string>(s =>
                {
                    UITask.Run(() =>
                    {
                        var newGroupTagModel = (GroupModel)SessionController.Instance.IdToSendables[s];
                        _generatedGroupTag = (GroupTagNodeView)SessionController.Instance.GetUserControlById(s);
                        _generatedGroupTag.Loaded += delegate(object sender, RoutedEventArgs args)
                        {
                            newGroupTagModel.X -= _generatedGroupTag.GetTagSize().Width/2.0;
                            var intersectionNodes = new List<AnimatableUserControl>();

                            foreach (var control in SessionController.Instance.ActiveWorkspace.AtomViewList)
                            {
                                foreach (var nodeModel in intersection)
                                {
                                    if (((AtomViewModel)control.DataContext).Model == nodeModel)
                                    {
                                        intersectionNodes.Add((AnimatableUserControl)control);
                                    }
                                }
                            }

                            var numChildren = intersectionNodes.Count;
                            var center = _generatedGroupTag.GetCenter();
                            var tagSize = _generatedGroupTag.GetTagSize();
                            var targetSize = 80;
                            var c = -1;
                            for (var i = 0; i < numChildren; i++)
                            {
                                //  Anim.To((AnimatableUserControl) control, "scaleX", 1, 1000);
                                // Anim.To((AnimatableUserControl) control, "scaleY", 1, 1000);
                                if (i % 2 == 0)
                                    c++;

                                var control = intersectionNodes[i];
                                control.Tag = "intersected";
                                var largerSide = control.Width > control.Height ? control.Height : control.Width;
                                var scaleRatio = targetSize / largerSide;
                                var cos = Math.Cos(c * Math.PI * 2.0 / numChildren*2 + Math.PI);
                                var sin = Math.Sin(c * Math.PI * 2.0 / numChildren*2 + Math.PI);
                                var tx = center.X + cos * (tagSize.Width / 2 + control.ActualWidth * scaleRatio / 2 + 20) - control.ActualWidth * scaleRatio / 4;
                                var ty = center.Y + sin * (tagSize.Height / 2 + control.ActualHeight * scaleRatio / 2 + 20);
                               // var tx = center.X + Math.Sin(i * Math.PI * 2.0 / numChildren) * tagSize.Width/2 ;
                                //var ty = center.Y + Math.Cos(i * Math.PI * 2.0 / numChildren) * tagSize.Height/2;
                                Anim.To(control, "X", tx, 600, new QuinticEase());
                                Anim.To(control, "Y", ty, 600, new QuinticEase());
                      

                            }

                            _generatedGroupTag.SetNum(numChildren/2);
                            _initialGroupNode.ShowChildren();
                            _intersectedGroupNode.ShowChildren();
                        };

                        
                    });
                });


                var minX = Math.Min(initialTransform.TranslateX, intersectedTransform.TranslateX);
                var maxX = Math.Max(initialTransform.TranslateX + _initialGroupNode.ActualWidth, intersectedTransform.TranslateX + _intersectedGroupNode.ActualWidth);

                var avgX = (minX + (maxX-minX)/2.0);
                var avgY = (initialTransform.TranslateY + intersectedTransform.TranslateY)/2.0;

                NetworkConnector.Instance.RequestNewGroupTag(avgX.ToString(), (avgY + 450).ToString(), n1 + " ∩ " + n2, props, callback);
            };
        }

        

        public override async Task Activate()
        {
            var wvm = (WorkspaceViewModel) _view.DataContext;
            wvm.AtomViewList.CollectionChanged += OnWorkspaceChildrenChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s is GroupTagNodeView))
            {
                userControl.PointerEntered += OnAtomPressed;
                userControl.PointerMoved += OnPointerMoved;
                userControl.PointerExited += OnAtomReleased;
            }
        }

        public override async Task Deactivate()
        {
            var wvm = (WorkspaceViewModel) _view.DataContext;
            wvm.AtomViewList.CollectionChanged -= OnWorkspaceChildrenChanged;
            foreach (var userControl in wvm.AtomViewList.Where(s => s is GroupTagNodeView))
            {
                userControl.PointerEntered -= OnAtomPressed;
                userControl.PointerMoved -= OnPointerMoved;
                userControl.PointerExited -= OnAtomReleased;
            }
        }



        private void OnWorkspaceChildrenChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            if (notifyCollectionChangedEventArgs.NewItems == null)
                return;

            foreach (var newItem in notifyCollectionChangedEventArgs.NewItems)
            {
                if (!(((FrameworkElement) newItem) is GroupTagNodeView)) continue;
                var item = (UserControl) newItem;
                item.PointerPressed += OnAtomPressed;
                item.PointerMoved += OnPointerMoved;
                item.PointerReleased += OnAtomReleased;
            }
        }


        private void OnPointerMoved(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            //    Debug.WriteLine(sender);
            
        }

        private async void OnAtomReleased(object sender, PointerRoutedEventArgs e)
        {
            var obj = (GroupTagNodeView)sender;
            var vm = (GroupTagNodeViewModel)obj.DataContext;

            if (vm.IsTemporary)
                return;

            const double threshold = 300;
            var x = ((CompositeTransform)obj.RenderTransform).TranslateX;
            var y = ((CompositeTransform)obj.RenderTransform).TranslateY;



            if (_intersectedGroupNode != null)
            {
                var t = (CompositeTransform)_intersectedGroupNode.RenderTransform;
                var dx = Math.Abs(x - t.TranslateX);
                var dy = Math.Abs(y - t.TranslateY);
                if (Math.Sqrt(dx * dx + dy * dy) > threshold)
                {
                    Debug.WriteLine("Done intersecting");
                    _timer.Stop();

                    if (_generatedGroupTag != null)
                    {
                        Anim.To(_generatedGroupTag, "Alpha", 0, 800);

                        ((GroupTagNodeViewModel) _initialGroupNode.DataContext).EnableChildMove = true;
                        ((GroupTagNodeViewModel)_intersectedGroupNode.DataContext).EnableChildMove = true;
                        _initialGroupNode.UnIntersect();
                        _initialGroupNode.ShowChildren();
                        _intersectedGroupNode.UnIntersect();
                        _intersectedGroupNode.ShowChildren();
                    }
                    _intersectedGroupNode = null;
                    return;
                }
            }

            foreach (var userControl in _searchList)
            {
                if (userControl == obj)
                    continue;

                var otherVm = (GroupTagNodeViewModel)userControl.DataContext;
                if (otherVm.IsTemporary)
                    continue;

                var transform = (CompositeTransform)userControl.RenderTransform;
                var distX = Math.Abs(x - transform.TranslateX);
                var distY = Math.Abs(y - transform.TranslateY);

                if (_intersectedGroupNode == null && Math.Sqrt(distX * distX + distY * distY) < threshold)
                {
                    _timer.Start();
                    _intersectedGroupNode = (GroupTagNodeView)userControl;
                    break;
                }
            }

        }

        private void OnAtomPressed(object sender, PointerRoutedEventArgs e)
        {
            _initialGroupNode = (GroupTagNodeView)sender;
            var wvm = (WorkspaceViewModel) _view.DataContext;
            _searchList = wvm.AtomViewList.Where(s => s is GroupTagNodeView).ToList();
        }
    }
}