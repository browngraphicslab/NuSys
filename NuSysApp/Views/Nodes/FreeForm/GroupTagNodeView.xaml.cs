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
    public sealed partial class GroupTagNodeView : UserControl
    {
        private Point _prevPos = new Point();
        private bool _isOpen;

        public GroupTagNodeView(GroupTagNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            IsDoubleTapEnabled = true;
            
            Loaded += delegate(object sender, RoutedEventArgs args)
            {  
                Canvas.SetLeft(NumBorder, Title.ActualWidth-10);
            };

            var groupNodeModel = (GroupModel)vm.Model;
            groupNodeModel.Children.CollectionChanged += OnChildrenChanged;
          //  groupNodeModel.PositionChanged += OnPositionChanged;
            nodeTpl.OnTemplateReady += delegate { nodeTpl.resizer.Visibility = Visibility.Collapsed; nodeTpl.tags.Visibility = Visibility.Collapsed; };

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

        private void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var groupNodeModel = (GroupModel)((GroupViewModel)DataContext).Model;
            groupNodeModel.Children.CollectionChanged += OnChildrenChanged;

            if (_isOpen)
            {
                ShowChildren();
            }
            else
            {
                if (e.NewItems == null) return;
                var child = (UserControl) e.NewItems[0];
                AnimateChild(child, groupNodeModel.X + TitleBorder.ActualWidth/2,
                    groupNodeModel.Y + TitleBorder.ActualHeight/2, 0, 0, 1 + (new Random().Next()%10), 10, true);
            }
        }

        public void ToggleExpand()
        {
            if (!_isOpen)
            {
                var model = (GroupModel)((GroupViewModel)DataContext).Model;
                _prevPos = new Point(model.X, model.Y);
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
            var groupNodeModel = (GroupModel)vm.Model;
            var numChildren = children.Count;
            for (var i = 0; i < numChildren; i++)
            {
                var child = children[i];
                child.Visibility = Visibility.Visible;
                AnimateChild(child, groupNodeModel.X + TitleBorder.ActualWidth / 2, groupNodeModel.Y + TitleBorder.ActualHeight/2, 0, 0, i, numChildren, true);
            }
        }

        private void ShowChildren(bool fromCenter = false)
        {
            var children = GetChildren();
            var numChildren = children.Count;
            
            var currentWidth = 0.0;
            for (var i = 0; i < numChildren; i++)
            {
                var child = children[i];
                child.Visibility = Visibility.Visible;
                var x = 0.0;
                var y = 0.0;
                var scaleX = 1.0;
                var scaleY = 1.0;
                if (child.RenderTransform is CompositeTransform)
                {
                    if (!fromCenter)
                    {
                        x = ((CompositeTransform) child.RenderTransform).TranslateX;
                        y = ((CompositeTransform) child.RenderTransform).TranslateY;
                    }
                    scaleX = ((CompositeTransform) child.RenderTransform).ScaleX;
                    scaleY = ((CompositeTransform) child.RenderTransform).ScaleY;
                }
                AnimateChild(child, x, y, scaleX, scaleY, i, numChildren);
            }


        }

        private List<UserControl> GetChildren()
        {
            var vm = (GroupTagNodeViewModel) DataContext;
            var groupNodeModel = (GroupModel) vm.Model;

            var children = new List<UserControl>();
            foreach (var atomView in SessionController.Instance.ActiveWorkspace.AtomViewList)
            {
                var atomId = ((AtomViewModel) atomView.DataContext).ID;
                if (groupNodeModel.Children.ContainsKey(atomId))
                    children.Add(atomView);
            }
            return children;
        }


        private void AnimateChild(UserControl child, double tx, double ty, double sx, double sy, int index, int numChildren, bool useBy = false)
        {
            var vm = (GroupTagNodeViewModel) DataContext;
            var groupNodeModel = (GroupModel)vm.Model;

            var targetSize = 80;
            var largerSide = child.Width > child.Height ? child.Height : child.Width;
            var scaleRatio = targetSize/largerSide;


            var duration = 400;

            var childModel = (NodeModel)((NodeViewModel) DataContext).Model;
            var animX = new Storyboard();
            var animXAnim = new DoubleAnimation();
            animXAnim.EnableDependentAnimation = true;
            animXAnim.Duration = TimeSpan.FromMilliseconds(duration);
            animXAnim.EasingFunction = new QuinticEase();
            if (useBy)
            {
                animXAnim.To = tx;
                animXAnim.From = groupNodeModel.X + Title.ActualWidth / 2.0 + Math.Sin(index * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - targetSize / 2.0;
            } else { 
                animXAnim.From = tx;
                animXAnim.To = groupNodeModel.X + Title.ActualWidth/2.0 + Math.Sin(index*Math.PI*2.0/numChildren)*(Title.ActualWidth + 80) - targetSize/2.0;
            }
            animX.Children.Add(animXAnim);
            Storyboard.SetTargetProperty(animXAnim, "X");
            Storyboard.SetTarget(animXAnim, child);
            animX.Begin();

            var animY = new Storyboard();
            var animYAnim = new DoubleAnimation();
            animYAnim.EnableDependentAnimation = true;
            animYAnim.EasingFunction = new QuinticEase();
            animYAnim.Duration = TimeSpan.FromMilliseconds(duration);
            if (useBy)
            {
                animYAnim.To = ty;
                animYAnim.From = groupNodeModel.Y + Title.ActualHeight / 2.0 + Math.Cos(index * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - targetSize / 2.0;
            }
            else
            {
                animYAnim.From = ty;
                animYAnim.To = groupNodeModel.Y + Title.ActualHeight/2.0 + Math.Cos(index*Math.PI*2.0/numChildren)*(Title.ActualHeight + 80) - targetSize/2.0;
            }
            animY.Children.Add(animYAnim);
            Storyboard.SetTargetProperty(animYAnim, "Y");
            Storyboard.SetTarget(animYAnim, child);
            animY.Begin();

            /*
            var animScaleY = new Storyboard();
            var animScaleYAnim = new DoubleAnimation();
            animScaleYAnim.EasingFunction = new QuinticEase();
            animScaleYAnim.Duration = TimeSpan.FromMilliseconds(duration);
            if (useBy)
            {
                animScaleYAnim.To = sy;
                animScaleYAnim.From = scaleRatio;
            }
            else
            {
                animScaleYAnim.From = sy;
                animScaleYAnim.To = scaleRatio;
            }
            animScaleY.Children.Add(animScaleYAnim);
            Storyboard.SetTargetProperty(animScaleY, "ScaleY");
            Storyboard.SetTarget(animScaleY, child.RenderTransform);
            animScaleY.Begin();

            var animScaleX = new Storyboard();
            var animScaleXAnim = new DoubleAnimation();
            animScaleXAnim.EasingFunction = new QuinticEase();
            animScaleXAnim.Duration = TimeSpan.FromMilliseconds(duration);
            if (useBy)
            {
                animScaleXAnim.To = sx;
                animScaleXAnim.From = scaleRatio;
            }
            else
            {
                animScaleXAnim.From = sx;
                animScaleXAnim.To = scaleRatio;
            }
            animScaleX.Children.Add(animScaleXAnim);
            Storyboard.SetTargetProperty(animScaleX, "ScaleX");
            Storyboard.SetTarget(animScaleX, child.RenderTransform);
            animScaleX.Begin();

    */
        }

    }
}