using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextNodeView2 : UserControl
    {

        public TextNodeView2(TextNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;          
        }

        private void OnEditClick(object sender, RoutedEventArgs e)
        {
            TextNodeViewModel vm = (TextNodeViewModel)this.DataContext;
            if (vm.IsEditingInk == true)
            {
                nodeTpl.ToggleInkMode();
            }
            vm.ToggleEditing();
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            nodeTpl.ToggleInkMode();            
        }

        private void FormatText(object sender, RoutedEventArgs e)
        {
            
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var vm = (TextNodeViewModel)this.DataContext;
            vm.CreateAnnotation();
            if (vm.IsAnnotation)
            {
                nodeTpl.bg.Background = new SolidColorBrush(Color.FromArgb(100, 255, 235, 205));
                this.textBlock.Foreground = new SolidColorBrush(Colors.Black);
                this.textBox.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

    }
}
