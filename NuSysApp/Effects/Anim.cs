using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class Anim
    {

        public static Storyboard FromTo(DependencyObject target, string property, double from, double to, int duration,
            EasingFunctionBase easing = null, EventHandler<object> callback = null)
        {
            easing = easing ?? new QuadraticEase();
            var storyboard = new Storyboard();
            
            if (callback != null)
                storyboard.Completed += callback;

            var anim = new DoubleAnimation();
            anim.EnableDependentAnimation = true;
            anim.Duration = TimeSpan.FromMilliseconds(duration);
            anim.EasingFunction = easing;
            anim.From = from;
            anim.To = to;
            storyboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, property);
            Storyboard.SetTarget(anim, target);
            storyboard.Begin();
            return storyboard;
        }

        public static Storyboard To(FrameworkElement target, string property, double to, int duration,
    EasingFunctionBase easing = null, EventHandler<object> callback = null)
        {
            var transform = (CompositeTransform) target.RenderTransform;
            double from = 0.0;
            switch (property)
            {
                case "X":
                    from = transform.TranslateX;
                    break;
                case "Y":
                    from = transform.TranslateY;
                    break;
                case "ScaleX":
                    from = transform.ScaleX;
                    break;
                case "ScaleY":
                    from = transform.ScaleY;
                    break;
                case "Alpha":
                    from = target.Opacity;
                    break;
                default:
                    throw new Exception("Unsupported proptery");

            }

            easing = easing ?? new QuadraticEase();
            var storyboard = new Storyboard();

            if (callback != null)
                storyboard.Completed += callback;

            var anim = new DoubleAnimation();
            anim.EnableDependentAnimation = true;
            anim.Duration = TimeSpan.FromMilliseconds(duration);
            anim.EasingFunction = easing;
            anim.From = from;
            anim.To = to;
            storyboard.Children.Add(anim);
            Storyboard.SetTargetProperty(anim, property);
            Storyboard.SetTarget(anim, target);
            storyboard.Begin();
            return storyboard;
        }
    }
}
