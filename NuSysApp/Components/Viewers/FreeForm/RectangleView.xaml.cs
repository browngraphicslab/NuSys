using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Viewers.FreeForm
{
    public sealed partial class RectangleView : UserControl
    {
        private RectangleViewModel _vm;
        public RectangleView(RectangleViewModel vm)
        {
            _vm = vm;
            DataContext = _vm;
            this.InitializeComponent();
            _vm.Model.OnSelect += OnLinkSelect;
            _vm.Model.OnDeselect += OnLinkDeselect;

            setupRectangle();
        }

        private void setupRectangle()
        {
            rectangle.StrokeThickness = 2;
            rectangle.Stroke = new SolidColorBrush(Colors.Black);
            rectangle.Fill = new SolidColorBrush(Colors.Transparent);

            Canvas.SetLeft(this, _vm.Left);
            Canvas.SetTop(this, _vm.Top);
   
            Binding widthBinding = new Binding();
            widthBinding.Path = new PropertyPath("RectWidth");
            Binding heightBinding = new Binding();
            heightBinding.Path = new PropertyPath("RectHeight");

            rectangle.SetBinding(Rectangle.WidthProperty, widthBinding);
            rectangle.SetBinding(Rectangle.HeightProperty, heightBinding);
        }

        private void OnLinkSelect()
        {
            Debug.WriteLine("SELECT!!");
            rectangle.Fill = new SolidColorBrush(Colors.Yellow);
            rectangle.Opacity = 0.2;
            Debug.WriteLine("rectangle view link selected!!");
        }

        private void OnLinkDeselect()
        {
            Debug.WriteLine("DESLECT!!!");
            rectangle.Fill = new SolidColorBrush(Colors.Transparent);
            rectangle.Opacity = 1;
            Debug.WriteLine("rectangle view link deselected!");
        }

        public void setRectangleSize(double nodeWidth, double nodeHeight)
        {
            Debug.WriteLine("set rectangle size!!");

            _vm.NodeHeight = nodeHeight;
            _vm.NodeWidth = nodeWidth;

            _vm.Left = _vm.NodeWidth*_vm.LeftRatio;
            _vm.Top = _vm.NodeHeight*_vm.TopRatio;
            _vm.RectWidth = _vm.RectWidthRatio*_vm.NodeWidth;
            _vm.RectHeight = _vm.RectHeightRatio*_vm.NodeHeight;

            Canvas.SetLeft(this, _vm.Left);
            Canvas.SetTop(this, _vm.Top);
            rectangle.Width = _vm.RectWidth;
            rectangle.Height = _vm.RectHeight;
        }

        public void changeColor()
        {
            rectangle.Fill = new SolidColorBrush(Colors.Yellow);
        }
    }
}
