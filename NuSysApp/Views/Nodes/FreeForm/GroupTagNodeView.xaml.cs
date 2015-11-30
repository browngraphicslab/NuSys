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
    public sealed partial class GroupTagNodeView : AnimatableUserControl
    {
        private bool _isOpen;

        public GroupTagNodeView(GroupTagNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            IsDoubleTapEnabled = true;
            RenderTransformOrigin = new Point(0.5,0.5);
            vm.Alpha = 0;
            var groupNodeModel = (GroupModel)vm.Model;
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
            
            vm.ChildAdded += delegate(object source, AnimatableUserControl node)
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

        public void ChildAdded(AnimatableUserControl child)
        {
        
        }


        private void OnChildrenChanged(AnimatableUserControl child)
        {
            var groupNodeModel = (GroupModel) ((GroupViewModel) DataContext).Model;
            var childVm = (NodeViewModel)child.DataContext;
            if (!((NodeModel)childVm.Model).GetMetaData("tags").Contains(groupNodeModel.Title))
                return;

            if (_isOpen)
            {
                var numChildren = groupNodeModel.Children.Count;
                var i = numChildren - 1;
                var tx = groupNodeModel.X + Title.ActualWidth / 2.0 + Math.Sin(i * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - 80 / 2.0;
                var ty = groupNodeModel.Y + Title.ActualHeight / 2.0 + Math.Cos(i * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - 80 / 2.0;

         
                Anim.To(child, "X", tx, 350, new QuinticEase());
                Anim.To(child, "Y", ty, 350, new QuinticEase());
                Anim.To(child, "ScaleX", 0, 350, new QuinticEase());
                Anim.To(child, "ScaleY", 0, 350, new QuinticEase());

                ShowChildren();
            }
            else
            {
          
               Anim.To(child, "X", groupNodeModel.X + TitleBorder.ActualWidth / 2, 350, new QuinticEase());
               Anim.To(child, "Y", groupNodeModel.Y + TitleBorder.ActualHeight / 2, 350, new QuinticEase());
               Anim.To(child, "ScaleX", 0, 350, new QuinticEase());
               Anim.To(child, "ScaleY", 0, 350, new QuinticEase());
               
            }
        }

        public void ToggleExpand()
        {
            if (!_isOpen)
            {
                var model = (GroupModel) ((GroupViewModel) DataContext).Model;
                ShowChildren();
            }
            else
            {
                HideChildren();
            }

            _isOpen = !_isOpen;
        }

        private void HideChildren()
        {
            var children = GetChildren();
            var vm = (GroupTagNodeViewModel) DataContext;
            var groupNodeModel = (GroupModel) vm.Model;
            var numChildren = children.Count;
            for (var i = 0; i < numChildren; i++)
            {
                var child = children[i];
                child.Visibility = Visibility.Visible;
                Anim.To(child, "X", groupNodeModel.X + TitleBorder.ActualWidth / 2, 400, new QuinticEase());
                Anim.To(child, "Y", groupNodeModel.Y + TitleBorder.ActualHeight / 2, 400, new QuinticEase());
                Anim.To(child, "ScaleX", 0, 400, new QuinticEase());
                Anim.To(child, "ScaleY", 0, 400, new QuinticEase());
            }
        }

        private void ShowChildren(bool fromCenter = false)
        {
            var vm = (GroupTagNodeViewModel)DataContext;
            var groupNodeModel = (GroupModel)vm.Model;
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

                var tx = groupNodeModel.X + Title.ActualWidth / 2.0 + Math.Sin(i * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - targetSize / 2.0;
                var ty = groupNodeModel.Y + Title.ActualHeight / 2.0 + Math.Cos(i * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - targetSize / 2.0;
                
                Anim.To(child, "X", tx, 400, new QuinticEase());
                Anim.To(child, "Y", ty, 400, new QuinticEase());
                Anim.To(child, "ScaleX", scaleRatio, 400, new QuinticEase());
                Anim.To(child, "ScaleY", scaleRatio, 400, new QuinticEase());
       
            }
        }

        private List<AnimatableUserControl> GetChildren()
        {
            var vm = (GroupTagNodeViewModel) DataContext;
            var groupNodeModel = (GroupModel) vm.Model;

            var children = new List<AnimatableUserControl>();
            foreach (var atomView in SessionController.Instance.ActiveWorkspace.AtomViewList)
            {
                var atomId = ((AtomViewModel) atomView.DataContext).ID;
                if (groupNodeModel.Children.ContainsKey(atomId))
                    children.Add((AnimatableUserControl)atomView);
            }
            return children;
        }
    }
}