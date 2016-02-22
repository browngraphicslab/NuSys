using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public class AnimatableNodeView : UserControl
    {
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(double), typeof(AnimatableNodeView), new PropertyMetadata(0.0,
OnXPropertyChanged));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(double), typeof(AnimatableNodeView), new PropertyMetadata(0.0,
      OnYPropertyChanged));

        public static readonly DependencyProperty ScaleXProperty = DependencyProperty.Register("ScaleX", typeof(double), typeof(AnimatableNodeView), new PropertyMetadata(1.0,
OnScaleXPropertyChanged));

        public static readonly DependencyProperty ScaleYProperty = DependencyProperty.Register("ScaleY", typeof(double), typeof(AnimatableNodeView), new PropertyMetadata(1.0,
OnScaleYPropertyChanged));

        public static readonly DependencyProperty AlphaProperty = DependencyProperty.Register("Alpha", typeof(double), typeof(AnimatableNodeView), new PropertyMetadata(1.0,
OnAlphaPropertyChanged));

        protected static void OnXPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableNodeView)dd;
            ((ElementInstanceModel)((AtomViewModel)d.DataContext).Model).X = (double)e.NewValue;
        }

        protected static void OnYPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableNodeView)dd;
            ((ElementInstanceModel)((AtomViewModel)d.DataContext).Model).Y = (double)e.NewValue;
        }

        protected static void OnScaleXPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableNodeView)dd;
            ((ElementInstanceModel)((AtomViewModel)d.DataContext).Model).ScaleX = (double)e.NewValue;
        }

        protected static void OnScaleYPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableNodeView)dd;
            ((ElementInstanceModel)((AtomViewModel)d.DataContext).Model).ScaleY = (double)e.NewValue;
        }

        protected static void OnAlphaPropertyChanged(DependencyObject dd, DependencyPropertyChangedEventArgs e)
        {
            var d = (AnimatableNodeView)dd;
            ((ElementInstanceModel)((AtomViewModel)d.DataContext).Model).Alpha = (double)e.NewValue;
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
