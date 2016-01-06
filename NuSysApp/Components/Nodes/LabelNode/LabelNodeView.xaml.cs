using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LabelNodeView : AnimatableNodeView
    {
        private bool _isOpen;

        public LabelNodeView(LabelNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            IsDoubleTapEnabled = true;
            RenderTransformOrigin = new Point(0.5,0.5);
            vm.Alpha = 0;
            var groupNodeModel = (NodeContainerModel)vm.Model;
            Loaded += delegate
            {
                Canvas.SetLeft(NumBorder, Title.ActualWidth - 10);
                Anim.FromTo(this, "Alpha", 0, 1, 350, new QuinticEase() { EasingMode = EasingMode.EaseIn });
            };
            
            groupNodeModel.ModeChanged += delegate
            {
                var t = groupNodeModel.IsTemporary ? 0.3 : 1;
                Anim.FromTo(this, "Alpha", 0, t, 500);
            };
            
            vm.ChildAdded +=  async delegate (object source, AnimatableUserControl node)
            {
                Debug.WriteLine("VIEW = CHILD ADDED");
                OnChildrenChanged(node);
            };



            nodeTpl.OnTemplateReady += delegate
            {
                nodeTpl.resizer.Visibility = Visibility.Collapsed;
                nodeTpl.tags.Visibility = Visibility.Collapsed;
            };

            PointerEntered += delegate
            {
                Title.Foreground = new SolidColorBrush(Colors.White);
                TitleBorder.Background = new SolidColorBrush(Colors.DarkRed);
            };

            PointerExited += delegate
            {
                Title.Foreground = new SolidColorBrush(Colors.Black);
                TitleBorder.Background = new SolidColorBrush(Colors.Transparent);
            };
        }

        private void OnChildrenChanged(AnimatableUserControl child)
        {
            var groupNodeModel = (NodeContainerViewModel) DataContext;
            Num.Text = groupNodeModel.AtomViewList.Count.ToString();
            var childVm = (NodeViewModel)child.DataContext;;
            var tags = (List<string>)((NodeModel)childVm.Model).GetMetaData("tags");
            if (!(tags.Contains(groupNodeModel.Title)))
                return;

            if (_isOpen)
            {
                var numChildren = groupNodeModel.Children.Count;
                var i = numChildren - 1;
                var targetSize = 80;
                var largerSide = child.Width > child.Height ? child.Height : child.Width;
                var scaleRatio = targetSize / largerSide;
                var cos = Math.Cos(i * Math.PI * 2.0 / numChildren);
                var sin = Math.Sin(i * Math.PI * 2.0 / numChildren);
                // var tx = groupNodeModel.X + Title.ActualWidth / 2.0 + Math.Sin(i * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - 80 / 2.0;
                // var ty = groupNodeModel.Y + Title.ActualHeight / 2.0 + Math.Cos(i * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - 80 / 2.0;
                var tx = GetCenter().X + sin * (GetTagSize().Width / 2 + child.ActualWidth * scaleRatio / 2 + 40);
                var ty = GetCenter().Y + cos * (GetTagSize().Height / 2 + child.ActualHeight * scaleRatio / 2 + 30);

                Anim.To(child, "X", tx, 700, new QuinticEase());
                Anim.To(child, "Y", ty, 700, new QuinticEase());
                Anim.To(child, "ScaleX", 0, 700, new QuinticEase());
                Anim.To(child, "ScaleY", 0, 700, new QuinticEase());

                ShowChildren();
            }
            else
            {
          
               Anim.To(child, "X", groupNodeModel.X + TitleBorder.ActualWidth / 2, 700, new QuinticEase());
               Anim.To(child, "Y", groupNodeModel.Y + TitleBorder.ActualHeight / 2, 700, new QuinticEase());
               Anim.To(child, "ScaleX", 0, 700, new QuinticEase());
               Anim.To(child, "ScaleY", 0, 700, new QuinticEase());
               
            }


        }

        public void ToggleExpand()
        {
            if (!_isOpen)
            {
                var model = (NodeContainerModel) ((NodeContainerViewModel) DataContext).Model;
                ShowChildren();
            }
            else
            {
                HideChildren();
            }

            _isOpen = !_isOpen;
        }

        public void HideChildren()
        {
            var children = GetChildren();
            var vm = (LabelNodeViewModel) DataContext;
            var groupNodeModel = (NodeContainerModel) vm.Model;
            var numChildren = children.Count;
            for (var i = 0; i < numChildren; i++)
            {
                var child = children[i];
                child.Visibility = Visibility.Visible;
                Anim.To(child, "X", GetCenter().X + 40, 400, new QuinticEase());
                Anim.To(child, "Y", GetCenter().Y + 40, 400, new QuinticEase());
                Anim.To(child, "ScaleX", 0, 400, new QuinticEase());
                Anim.To(child, "ScaleY", 0, 400, new QuinticEase());
            }
        }

        public void UnIntersect()
        {
            var vm = (LabelNodeViewModel)DataContext;

            foreach (var atomView in SessionController.Instance.ActiveWorkspace.Children.Values)
            {
                var atomId = ((AtomViewModel)atomView.DataContext).Id;
                var l = vm.Children.Values.Where((s =>
                {
                    var v = (AtomViewModel) s.DataContext;
                    return v.Id == atomId;
                }));
                if (l.Any() && atomView.Tag == "intersected")
                    atomView.Tag = null;
            }
        }

        public void ShowChildren(bool fromCenter = false)
        {
            var vm = (LabelNodeViewModel)DataContext;
            var groupNodeModel = (NodeContainerModel)vm.Model;
            var targetSize = 80;
            
            var children = GetChildren();
            var numChildren = children.Count;

            var currentWidth = 0.0;
            for (var i = 0; i < numChildren; i++)
            {
                var child = children[i];
                child.Visibility = Visibility.Visible;
   
                var largerSide = child.Width > child.Height ? child.Height : child.Width;
                var scaleRatio = targetSize / largerSide;

               // var tx = groupNodeModel.X + Title.ActualWidth / 2.0 + Math.Sin(i * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - targetSize / 2.0;
                //var ty = groupNodeModel.Y + Title.ActualHeight / 2.0 + Math.Cos(i * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - targetSize / 2.0;
                var cos = Math.Cos(i*Math.PI*2.0/numChildren + Math.PI);
                var sin = Math.Sin(i*Math.PI*2.0/numChildren + Math.PI);
                var tx = GetCenter().X + sin * (GetTagSize().Width / 2 + child.ActualWidth * scaleRatio / 2 + 40) - child.ActualWidth * scaleRatio / 4;
                var ty = GetCenter().Y + cos*(GetTagSize().Height/2 + child.ActualHeight*scaleRatio/2 + 20);
                Anim.To(child, "X", tx, 400, new QuinticEase());
                Anim.To(child, "Y", ty, 400, new QuinticEase());
                Anim.To(child, "ScaleX", scaleRatio, 400, new QuinticEase());
                Anim.To(child, "ScaleY", scaleRatio, 400, new QuinticEase());
       
            }

            Num.Text = GetChildren().Count.ToString();
        }

        public Point GetCenter()
        {
            var vm = (LabelNodeViewModel)DataContext;
            var groupNodeModel = (NodeContainerModel)vm.Model;
            return new Point(groupNodeModel.X + TitleBorder.ActualWidth/2.0, groupNodeModel.Y + TitleBorder.ActualHeight / 2.0);
        }

        public Size GetTagSize()
        {
            return new Size(TitleBorder.ActualWidth, TitleBorder.ActualHeight);
        }

        public void SetNum(int num)
        {
            Num.Text = num.ToString();
        }

        private List<AnimatableNodeView> GetChildren()
        {
            var vm = (LabelNodeViewModel) DataContext;

            var children = new List<AnimatableNodeView>();
            foreach (var atomView in SessionController.Instance.ActiveWorkspace.Children.Values)
            {
                var atomId = ((AtomViewModel) atomView.DataContext).Id;
                var l = vm.Children.Values.Where((s =>
                {
                    var v = (AtomViewModel)s.DataContext;
                    return v.Id == atomId;
                }));
                if (l.Any() && atomView.Tag != "intersected")
                    children.Add((AnimatableNodeView)atomView);
            }
            return children;
        }
    }
}