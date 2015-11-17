using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
        //private Point _delta;
        private Point _childPos;
        private bool _isOpen;
        private Point _orgChildPos;
        private DispatcherTimer _timer;

        public GroupTagNodeView(GroupTagNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            IsDoubleTapEnabled = true;
            
            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                Num.Text = vm.AtomViewList.Count.ToString();
                Canvas.SetLeft(NumBorder, Title.ActualWidth-10);
            };
           
     
            vm.AtomViewList.CollectionChanged +=
                delegate(object sender, NotifyCollectionChangedEventArgs e)
                {
                    Num.Text = vm.AtomViewList.Count.ToString();
                    if (_isOpen)
                    {
                        ShowChildren();
                    }
                    else
                    {
                        if (e.NewItems != null)
                        {
                            var child = (UserControl)e.NewItems[0];
                            AnimateChild(child, TitleBorder.ActualWidth/2, TitleBorder.ActualHeight/2, 0, 0, 1, 6, true);
                            
                        }
                    }
                };

            nodeTpl.OnTemplateReady += delegate { nodeTpl.resizer.Visibility = Visibility.Collapsed; };

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

        public void ToggleExpand()
        {
            if (!_isOpen)
            {
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
            var vm = (GroupTagNodeViewModel) DataContext;
            var numChildren = vm.AtomViewList.Count;
            var currentWidth = 0.0;
            for (var i = 0; i < numChildren; i++)
            {
                var child = vm.AtomViewList[i];
                child.Visibility = Visibility.Visible;
                AnimateChild(child, TitleBorder.ActualWidth / 2, TitleBorder.ActualHeight/2, 0, 0, i, numChildren, true);
            }
        }

        private void ShowChildren(bool fromCenter = false)
        {
            var vm = (GroupTagNodeViewModel) DataContext;
            var numChildren = vm.AtomViewList.Count;
            var currentWidth = 0.0;
            for (var i = 0; i < numChildren; i++)
            {
                var child = vm.AtomViewList[i];
                child.Visibility = Visibility.Visible;
                var x = 0.0;
                var y = 0.0;
                var scaleX = 0.0;
                var scaleY = 0.0;
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


        private void AnimateChild(UserControl child, double tx, double ty, double sx, double sy, int index, int numChildren, bool useBy = false)
        {
            var targetSize = 80;
            var largerSide = child.Width > child.Height ? child.Height : child.Width;
            var scaleRatio = targetSize/largerSide;


            var compositeTranform = new CompositeTransform();
            child.RenderTransform = compositeTranform;

            var duration = 400;

            var animX = new Storyboard();
            var animXAnim = new DoubleAnimation();
            animXAnim.Duration = TimeSpan.FromMilliseconds(duration);
            animXAnim.EasingFunction = new QuinticEase();
            if (useBy)
            {
                animXAnim.To = tx;
                animXAnim.From = Title.ActualWidth / 2.0 + Math.Sin(index * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) -
                               targetSize / 2.0;
            } else { 
            animXAnim.From = tx;
            animXAnim.To = Title.ActualWidth/2.0 + Math.Sin(index*Math.PI*2.0/numChildren)*(Title.ActualWidth + 80) -
                           targetSize/2.0;
            }
            animX.Children.Add(animXAnim);
            Storyboard.SetTargetProperty(animXAnim, "TranslateX");
            Storyboard.SetTarget(animXAnim, compositeTranform);
            animX.Begin();

            var animY = new Storyboard();
            var animYAnim = new DoubleAnimation();
            animYAnim.EasingFunction = new QuinticEase();
            animYAnim.Duration = TimeSpan.FromMilliseconds(duration);
            if (useBy)
            {
                animYAnim.To = ty;
                animYAnim.From = Title.ActualHeight / 2.0 + Math.Cos(index * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) -
                               targetSize / 2.0;
            }
            else
            {
                animYAnim.From = ty;
                animYAnim.To = Title.ActualHeight/2.0 +
                               Math.Cos(index*Math.PI*2.0/numChildren)*(Title.ActualHeight + 80) -
                               targetSize/2.0;
            }
            animY.Children.Add(animYAnim);
            Storyboard.SetTargetProperty(animYAnim, "TranslateY");
            Storyboard.SetTarget(animYAnim, compositeTranform);
            animY.Begin();

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
            Storyboard.SetTarget(animScaleY, compositeTranform);
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
            Storyboard.SetTarget(animScaleX, compositeTranform);
            animScaleX.Begin();
        }
    }
}