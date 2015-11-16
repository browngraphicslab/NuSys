using Windows.UI;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using System.Numerics;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupTagNodeView : UserControl
    {
        private DispatcherTimer _timer;
        //private Point _delta;
        private Point _childPos;
        private Point _orgChildPos;
        public GroupTagNodeView(GroupTagNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            vm.AtomViewList.CollectionChanged +=
            delegate (object sender, NotifyCollectionChangedEventArgs args)
            {
                ArrangeChildren();
            };
            
            nodeTpl.OnTemplateReady += delegate
            {
                nodeTpl.resizer.Visibility = Visibility.Collapsed;
            };

            Title.PointerPressed += delegate(object sender, PointerRoutedEventArgs args)
            {
                Title.Foreground = new SolidColorBrush(Colors.DarkRed);
            };

            Title.PointerExited += delegate (object sender, PointerRoutedEventArgs args)
            {
                Title.Foreground = new SolidColorBrush(Colors.Black);
            };

       

        }

        private void ArrangeChildren()
        {
            var vm = (GroupTagNodeViewModel)DataContext;
            var numChildren = vm.AtomViewList.Count;
            var currentWidth = 0.0;
            for (int i = 0; i < numChildren; i++)
            {
                var model = vm.AtomViewList[i];

                var x = 2.0;
                var y = 0.0;
                var scaleX = 0.0;
                var scaleY = 0.0;
                if (model.RenderTransform is CompositeTransform)
                {
                    x = ((CompositeTransform) model.RenderTransform).TranslateX;
                    y = ((CompositeTransform) model.RenderTransform).TranslateY;
                    scaleX = ((CompositeTransform) model.RenderTransform).ScaleX;
                    scaleY = ((CompositeTransform) model.RenderTransform).ScaleY;
                }

                var targetSize = 80;
                var largerSide = model.Width > model.Height ? model.Height : model.Width;
                var scaleRatio = targetSize/largerSide;


                var compositeTranform = new CompositeTransform();
                model.RenderTransform = compositeTranform;

                var duration = 400;
                
                Storyboard animX = new Storyboard();
                DoubleAnimation animXAnim = new DoubleAnimation();
                animXAnim.Duration = TimeSpan.FromMilliseconds(duration);
                animXAnim.EasingFunction = new QuinticEase();
                animXAnim.From = x;
                animXAnim.To = Title.ActualWidth/2.0 + Math.Sin(i * Math.PI * 2.0 / numChildren) * (Title.ActualWidth + 80) - targetSize/2.0;
                animX.Children.Add(animXAnim);
                Storyboard.SetTargetProperty(animXAnim, "TranslateX");
                Storyboard.SetTarget(animXAnim, compositeTranform);
                animX.Begin();

                Storyboard animY = new Storyboard();
                DoubleAnimation animYAnim = new DoubleAnimation();
                animYAnim.EasingFunction = new QuinticEase();
                animYAnim.Duration = TimeSpan.FromMilliseconds(duration);
                animYAnim.From = y;
                animYAnim.To = Title.ActualHeight / 2.0 + Math.Cos(i * Math.PI * 2.0 / numChildren) * (Title.ActualHeight + 80) - targetSize/2.0;
                animY.Children.Add(animYAnim);
                Storyboard.SetTargetProperty(animYAnim, "TranslateY");
                Storyboard.SetTarget(animYAnim, compositeTranform);
                animY.Begin();

                Storyboard animScaleY = new Storyboard();
                DoubleAnimation animScaleYAnim = new DoubleAnimation();
                animScaleYAnim.EasingFunction = new QuinticEase();
                animScaleYAnim.Duration = TimeSpan.FromMilliseconds(duration);
                animScaleYAnim.From = scaleY;
                animScaleYAnim.To = scaleRatio;
                animScaleY.Children.Add(animScaleYAnim);
                Storyboard.SetTargetProperty(animScaleY, "ScaleY");
                Storyboard.SetTarget(animScaleY, compositeTranform);
                animScaleY.Begin();

                Storyboard animScaleX = new Storyboard();
                DoubleAnimation animScaleXAnim = new DoubleAnimation();
                animScaleXAnim.EasingFunction = new QuinticEase();
                animScaleXAnim.Duration = TimeSpan.FromMilliseconds(duration);
                animScaleXAnim.From = scaleX;
                animScaleXAnim.To = scaleRatio;
                animScaleX.Children.Add(animScaleXAnim);
                Storyboard.SetTargetProperty(animScaleX, "ScaleX");
                Storyboard.SetTarget(animScaleX, compositeTranform);
                animScaleX.Begin();

                //TitleBorder.Width = Title.ActualWidth + 50;
                //TitleBorder.Height = Title.ActualHeight + 50;
                //TitleBorder.Background = new SolidColorBrush(Colors.Transparent);
                
                /*
                var transMat = ((MatrixTransform)model.RenderTransform).Matrix;
                transMat.OffsetX = Math.Sin(i * Math.PI * 2 / numChildren) * Width;
                transMat.OffsetY = Math.Cos(i * Math.PI * 2 / numChildren) * Height;
                model.RenderTransform = new MatrixTransform
                {
                    Matrix = transMat
                };
                */

            }


        }
    }
}