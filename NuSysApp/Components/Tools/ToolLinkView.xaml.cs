using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public sealed partial class ToolLinkView : AnimatableUserControl
    {


        public ToolLinkView(ToolLinkViewModel vm)
        {
            this.DataContext = vm;
            vm.PropertyChanged += OnPropertyChanged;
            this.InitializeComponent();

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                UpdateControlPoints();
            };





        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.UpdateControlPoints();
        }

        private void UpdateControlPoints()
        {
            this.UpdateEndPoints();

            var vm = (ToolLinkViewModel)this.DataContext;

            var anchor1 = new Point(vm.InToolElementController.Model.X + vm.InToolElementController.Model.Width / 2, vm.InToolElementController.Model.Y + vm.InToolElementController.Model.Height / 2);
            var anchor2 = new Point(vm.OutToolElementController.Model.X + vm.OutToolElementController.Model.Width / 2, vm.OutToolElementController.Model.Y + vm.OutToolElementController.Model.Height / 2);

            var distanceX = anchor1.X - anchor2.X;
            var distanceY = anchor1.Y - anchor2.Y;

            curve.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curve.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);
            curveInner.Point2 = new Point(anchor1.X - distanceX / 2, anchor2.Y);
            curveInner.Point1 = new Point(anchor2.X + distanceX / 2, anchor1.Y);


        }

        private void UpdateEndPoints()
        {
            var vm = (ToolLinkViewModel) this.DataContext;
            var anchor1 = new Point(vm.InToolElementController.Model.X + vm.InToolElementController.Model.Width / 2, vm.InToolElementController.Model.Y + vm.InToolElementController.Model.Height / 2);
            var anchor2 = new Point(vm.OutToolElementController.Model.X + vm.OutToolElementController.Model.Width / 2, vm.OutToolElementController.Model.Y + vm.OutToolElementController.Model.Height / 2);


            pathfigure.StartPoint = anchor1;
            curve.Point3 = anchor2;

            pathfigureInner.StartPoint = anchor1;
            curveInner.Point3 = anchor2;

        }


        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Annotation_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
    