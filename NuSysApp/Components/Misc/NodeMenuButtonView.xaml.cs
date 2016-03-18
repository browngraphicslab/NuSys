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
    public partial class NodeMenuButtonView : UserControl
    {
        public static readonly DependencyProperty WindowProperty = DependencyProperty.RegisterAttached("Window", typeof(UserControl), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached("Caption", typeof(string), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty ParentButtonProperty = DependencyProperty.RegisterAttached("ParentButton", typeof(NodeMenuButtonView), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty IsModeProperty = DependencyProperty.RegisterAttached("IsMode", typeof(bool), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty IsSubButtonProperty = DependencyProperty.RegisterAttached("IsSubButton", typeof (bool), typeof (NodeMenuButtonView), null);
        private static readonly SolidColorBrush ColoredBorder = new SolidColorBrush(Color.FromArgb(255, 215, 231, 230));

        public NodeMenuButtonView()
        {
            this.InitializeComponent();
        }

        public bool IsSubButton
        {
            get { return (bool)GetValue(IsSubButtonProperty); }
            set
            {
                SetValue(IsSubButtonProperty, value);
                if (value)
                {
                    Style style = this.Resources["SubButton"] as Style;
                    btn.Style = style;
                    icon.Visibility = Visibility.Collapsed;
                }
            }
        }

        public virtual bool Active
        {
            set
            {
                btn.BorderBrush = value ? btn.BorderBrush = ColoredBorder : btn.BorderBrush = null;

                if (value && ParentButton != null)
                {
                    ParentButton.icon.Source = icon.Source;
                    ParentButton.Caption = btnCaption.Text;
                }
            }
        }

        public virtual string Icon
        {
            get { return (string)GetValue(IconProperty); }
            set {
                SetValue(IconProperty, value);
                icon.Source = new BitmapImage(new Uri(value)); ;
            }
        }

        public void SetIcon(ImageSource src)
        {
            icon.Source = src;
        }

        public virtual string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set
            {
                SetValue(CaptionProperty, value);
                btnCaption.Text = value; ;
            }
        }

        public NodeMenuButtonView ParentButton
        {
            get { return (NodeMenuButtonView)GetValue(ParentButtonProperty); }
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
