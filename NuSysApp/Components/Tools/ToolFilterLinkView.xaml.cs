﻿using System;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ToolFilterLinkView : AnimatableUserControl
    {
        public ToolFilterLinkView(ToolFilterLinkViewModel vm)
        {
            this.DataContext = vm;
            vm.PropertyChanged += OnPropertyChanged;
            this.InitializeComponent();
            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };
            vm.InToolController.Disposed += Tool_Disposed;
            vm.OutTool.Disposed += Tool_Disposed;

        }

        public void Tool_Disposed(string id)
        {
            if (id != "ToolFilterView")
            {
                (DataContext as ToolFilterLinkViewModel).OutTool.RemoveParentTool((DataContext as ToolFilterLinkViewModel).InTool);
            }
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as ToolFilterLinkViewModel).Dispose();
            (DataContext as ToolFilterLinkViewModel).InToolController.Disposed -= Tool_Disposed;
            (DataContext as ToolFilterLinkViewModel).OutTool.Disposed -= Tool_Disposed;
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }
        

        private void UpdateControlPoints()
        {
            this.UpdateEndPoints();
            this.UpdateArrow();


            var vm = (ToolFilterLinkViewModel)this.DataContext;

            var inToolVM = vm.InTool;
            var outToolVM = vm.OutTool;


            var anchor1 = new Point(inToolVM.X + inToolVM.Width / 2, inToolVM.Y + inToolVM.Height / 2);
            var anchor2 = new Point(outToolVM.X + outToolVM.Width / 2, outToolVM.Y + outToolVM.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            curveInner.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curveInner.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);


        }
        private void UpdateArrow()
        {
            var center = new Point((pathfigure.StartPoint.X + curve.Point3.X) / 2.0, (pathfigure.StartPoint.Y + curve.Point3.Y) / 2.0);
            var xDiff = curve.Point3.X - pathfigure.StartPoint.X;
            var yDiff = curve.Point3.Y - pathfigure.StartPoint.Y;
            var angle = Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
            var tranformGroup = new TransformGroup();
            tranformGroup.Children.Add(new RotateTransform { Angle = angle, CenterX = 20, CenterY = 20 });
            tranformGroup.Children.Add(new TranslateTransform { X = center.X - 20, Y = center.Y - 20 });

            arrow.RenderTransform = tranformGroup;
        }

        private void UpdateEndPoints()
        {
            var vm = (ToolFilterLinkViewModel)this.DataContext;

            var inToolVM = vm.InTool;
            var outToolVM = vm.OutTool;

            var anchor1 = new Point(inToolVM.X + inToolVM.Width / 2, inToolVM.Y + inToolVM.Height / 2);
            var anchor2 = new Point(outToolVM.X + outToolVM.Width / 2, outToolVM.Y + outToolVM.Height / 2);


            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;

            pathfigureInner.StartPoint = anchor1;
            curveInner.Point3 = anchor2;

        }

    }
}
