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
        public static readonly DependencyProperty IconProperty = DependencyProperty.RegisterAttached("Icon", typeof(string), typeof(NodeMenuButtonView), null);
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.RegisterAttached("Caption", typeof(string), typeof(NodeMenuButtonView), null);

        public NodeMenuButtonView()
        {
            this.InitializeComponent();
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
    }
}
