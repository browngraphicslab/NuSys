using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using MyToolkit.UI;
using NuSysApp.Nodes.AudioNode;
using SharpDX.Direct2D1;
using Image = Windows.UI.Xaml.Controls.Image;
using SolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Nodes
{
    public sealed partial class LinkedTimeBlock : UserControl
    {

        public LinkedTimeBlock(LinkedTimeBlockViewModel vm)
        {
            DataContext = vm;
            InitializeComponent();
            setUpLine(vm);
            vm.Model.OnSelect += OnLinkSelect;
            vm.Model.OnDeselect += OnLinkDeselect;
            
        }

        private void OnLinkSelect(LinkedTimeBlockModel model)
        {
            line.Stroke = new SolidColorBrush(Colors.Red);
            Debug.WriteLine("1");
        }

        private void OnLinkDeselect(LinkedTimeBlockModel model)
        {
            line.Stroke = new SolidColorBrush(Colors.Yellow);
            Debug.WriteLine("2");
        }

        public Line getLine()
        {
            return line;
        }

        public void setUpLine(LinkedTimeBlockViewModel vm)
        {

            this.DataContext = vm;
            line.StrokeThickness = (double)vm.Line1["StrokeThickness"];
            line.Stroke = (SolidColorBrush)vm.Line1["Stroke"];
            line.Opacity = (double)vm.Line1["Opacity"];
            //line.Detailx1 = (double)vm.Line1["Detailx1"];
            //Binding b = new Binding();
            //b.Source = "Detailx2";
            //Debug.WriteLine(d["Detailx2"]);
            //b.Mode = BindingMode.TwoWay;
            //line.Detailx2 = vm.Detailx2;
                //.SetBinding(Line.X2Property, b);
            Binding b1 = new Binding();
            b1.Path = new PropertyPath("Detailx1");
            line.SetBinding(Line.X1Property, b1);
            Binding b2 = new Binding();
            b2.Path = new PropertyPath("Detailx2"); 
            line.SetBinding(Line.X2Property, b2);

            line.Y1 = (double)vm.Line1["Y"];
            line.Y2 = (double)vm.Line1["Y"];
            line.Margin = new Thickness(0, (double)vm.Line1["TopMargin"], 0, 0);
        }

        public void changeColor()
        {
            //line.Fill = new SolidColorBrush(Colors.Red);
        }
    }
}
