using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Windows;
using System.ComponentModel;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BezierLink : UserControl
    {
        private LinkViewModel _vm;

        public BezierLink(LinkViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;
            _vm = vm;
            vm.Node1.PropertyChanged += new PropertyChangedEventHandler(node_PropertyChanged);
            vm.Node2.PropertyChanged += new PropertyChangedEventHandler(node_PropertyChanged);
            this.UpdateControlPoints();
        }

        private void node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }

        private void UpdateControlPoints()
        {
            var node1 = _vm.Node1;
            var node2 = _vm.Node2;
            var anchor1 = node1.Anchor;
            var anchor2 = node2.Anchor;
            var distanceX = anchor1.X - anchor2.X;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
        }


        private void SetUpBindings()
        {
            
         
        }

        
    }
}