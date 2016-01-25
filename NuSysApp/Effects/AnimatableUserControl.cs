using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class AnimatableUserControl : UserControl
    {
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(AnimatableUserControl), new PropertyMetadata(0.0,
OnXPropertyChanged));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(AnimatableUserControl), new PropertyMetadata(0.0,
      OnYPropertyChanged));

        public static readonly DependencyProperty ScaleXProperty = DependencyProperty.Register("ScaleX", typeof(double), typeof(AnimatableUserControl), new PropertyMetadata(1.0,
OnScaleXPropertyChanged));

        public static readonly DependencyProperty ScaleYProperty = DependencyProperty.Register("ScaleY", typeof(double), typeof(AnimatableUserControl), new PropertyMetadata(1.0,
OnScaleYPropertyChanged));

        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(double), typeof(AnimatableUserControl), new PropertyMetadata(1.0,
OnAlphaPropertyChanged));

        public AnimatableUserControl()
        {
            RenderTransform = new CompositeTransform();
        }

        protected static void OnXPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableUserControl)dd;
            var t = (CompositeTransform) d.RenderTransform;
            t.TranslateX = (double)e.NewValue;
        }

        protected static void OnYPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableUserControl)dd;
            var t = (CompositeTransform)d.RenderTransform;
            t.TranslateY = (double)e.NewValue;
        }

        protected static void OnScaleXPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableUserControl)dd;
            var t = (CompositeTransform)d.RenderTransform;
            t.ScaleX = (double)e.NewValue;
        }

        protected static void OnScaleYPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableUserControl)dd;
            var t = (CompositeTransform)d.RenderTransform;
            t.ScaleY = (double)e.NewValue;
        }

        protected static void OnAlphaPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableUserControl)dd;
            d.Opacity = (double)e.NewValue;
        }

        public double X
        {
            get { return (double)GetValue(XProperty); }
            set
            {
                SetValue(XProperty, value);
            }
        }

        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set
            {
                SetValue(YProperty, value);
            }
        }

        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set
            {
                SetValue(ScaleXProperty, value);
            }
        }

        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set
            {
                SetValue(ScaleYProperty, value);
            }
        }
    }
}
