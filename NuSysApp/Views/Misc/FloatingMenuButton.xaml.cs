using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace NuSysApp
{
    public sealed partial class FloatingMenuButton : UserControl
    {
        public static readonly DependencyProperty WindowProperty = DependencyProperty.RegisterAttached("Window", typeof(UserControl), typeof(FloatingMenuButton), null);
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(FloatingMenuButton), null);
        public static readonly DependencyProperty ParentButtonProperty = DependencyProperty.RegisterAttached("ParentButton", typeof(FloatingMenuButton), typeof(FloatingMenuButton), null);
        public static readonly DependencyProperty IsModeProperty = DependencyProperty.RegisterAttached("IsMode", typeof(bool), typeof(FloatingMenuButton), null);

        private static readonly SolidColorBrush ColoredBorder = new SolidColorBrush(Color.FromArgb(255, 194, 251, 255));

        public FloatingMenuButton()
        {
            this.InitializeComponent();
        }

        public bool Active
        {
            set
            {
                btn.BorderBrush = value ? btn.BorderBrush = ColoredBorder : btn.BorderBrush = null;

                if (value && ParentButton != null)
                {
                    ParentButton.icon.Source = icon.Source;
                }
            }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set {
                SetValue(IconProperty, value);
                icon.Source = new BitmapImage(new Uri(value)); ;
            }
        }

        public FloatingMenuButton ParentButton
        {
            get { return (FloatingMenuButton)GetValue(ParentButtonProperty); }
            set
            {
                SetValue(ParentButtonProperty, value);
            }
        }

        public UserControl Window
        {
            get { return (UserControl)GetValue(WindowProperty); }
            set
            {
                SetValue(WindowProperty, value);
            }
        }

        public bool IsMode
        {
            get { return (bool)GetValue(IsModeProperty); }
            set
            {
                SetValue(IsModeProperty, value);
            }
        }
    }
}
