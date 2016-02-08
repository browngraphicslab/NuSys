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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
   
namespace NuSysApp
{
    public sealed partial class SubMenuButtonView : UserControl
    {
        public static readonly DependencyProperty WindowProperty = DependencyProperty.RegisterAttached("Window", typeof(UserControl), typeof(FloatingMenuButtonView), null);
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(FloatingMenuButtonView), null);
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached("Caption", typeof(string), typeof(FloatingMenuButtonView), null);
        public static readonly DependencyProperty ParentButtonProperty = DependencyProperty.RegisterAttached("ParentButton", typeof(FloatingMenuButtonView), typeof(FloatingMenuButtonView), null);
        public static readonly DependencyProperty IsModeProperty = DependencyProperty.RegisterAttached("IsMode", typeof(bool), typeof(FloatingMenuButtonView), null);

        private static readonly SolidColorBrush ColoredBorder = new SolidColorBrush(Color.FromArgb(255, 215, 231, 230));

        public SubMenuButtonView()
        {
            this.InitializeComponent();
        }

        public bool Active
        {
            set
            {
                ParentButton.BorderBrush = value ? ParentButton.BorderBrush = ColoredBorder : ParentButton.BorderBrush = null;

                if (value && ParentButton != null)
                {
                    ParentButton.ButtonIcon.Source = icon.Source;
                }
            }
        }

        public string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set
            {
                SetValue(IconProperty, value);
                icon.Source = new BitmapImage(new Uri(value)); ;
            }
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set
            {
                SetValue(CaptionProperty, value);
                btn.Content = value;
            }
        }

        public FloatingMenuButtonView ParentButton
        {
            get { return (FloatingMenuButtonView)GetValue(ParentButtonProperty); }
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
