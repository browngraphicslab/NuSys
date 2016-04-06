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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LabelNodeView : AnimatableUserControl
    {
        private bool _isOpen;

        public LabelNodeView(LabelNodeViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
            
            RenderTransformOrigin = new Point(0.5,0.5);
            vm.Alpha = 0;
         
            Loaded += delegate
            {
                Anim.FromTo(this, "Alpha", 0, 1, 350, new QuinticEase() { EasingMode = EasingMode.EaseIn });
            };
            
            TitleBorder.PointerEntered += delegate
            {
                Title.Foreground = new SolidColorBrush(Constants.color6);
                TitleBorder.Background = new SolidColorBrush(Colors.DarkRed);
            };

            TitleBorder.PointerExited += delegate
            {
                Title.Foreground = new SolidColorBrush(Colors.Black);
                TitleBorder.Background = new SolidColorBrush(Colors.Transparent);
            };
        }

        public Point GetCenter()
        {
            var vm = (LabelNodeViewModel)DataContext;
            var groupNodeModel = (CollectionElementModel)vm.Model;
            return new Point(groupNodeModel.X + TitleBorder.ActualWidth/2.0, groupNodeModel.Y + TitleBorder.ActualHeight / 2.0);
        }

        public Size GetTagSize()
        {
            return new Size(TitleBorder.ActualWidth, TitleBorder.ActualHeight);
        }

        public void SetNum(int num)
        {
            Num.Text = num.ToString();
        }

        private void UIElement_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (LabelNodeViewModel)DataContext;
            vm.Title = (sender as TextBlock).Text;
        }

    }
}