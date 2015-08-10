﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NuSysApp
{
    [TemplatePart(Name = "inkCanvas", Type =typeof(InkCanvas))]
    [TemplatePart(Name = "btnDelete", Type = typeof(Button))]
    [TemplatePart(Name = "resizer", Type = typeof(Path))]
    [TemplatePart(Name = "bg", Type = typeof(Grid))]
    public sealed class NodeTemplate : ContentControl
    {
        public InkCanvas inkCanvas;
        public Button btnDelete;
        public Path resizer;
        public Grid bg;

        public NodeTemplate()
        {
            this.DefaultStyleKey = typeof(NodeTemplate);
        }

        public static readonly DependencyProperty SubMenuProperty = DependencyProperty.Register("SubMenu", typeof(object),    typeof(NodeTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty InnerProperty = DependencyProperty.Register("Inner", typeof(object), typeof(NodeTemplate), new PropertyMetadata(null));

        public object SubMenu
        {
            get { return (object)GetValue(SubMenuProperty); }
            set { SetValue(SubMenuProperty, value); }
        }

        public object Inner
        {
            get { return (object)GetValue(InnerProperty); }
            set { SetValue(InnerProperty, value); }
        }

        protected override void OnApplyTemplate()
        {
            inkCanvas = (InkCanvas)GetTemplateChild("inkCanvas");
            inkCanvas.InkPresenter.IsInputEnabled = false;
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Touch; //This line is setting the Devices that can be used to display ink

            bg = (Grid)GetTemplateChild("bg");
            
            btnDelete = (Button)GetTemplateChild("btnDelete");
            btnDelete.Click += OnBtnDeleteClick;

            resizer = (Path)GetTemplateChild("Resizer");
            resizer.ManipulationDelta += OnResizerManipulationDelta;

            this.ManipulationMode = ManipulationModes.All;
            this.ManipulationDelta += OnManipulationDelta;

            var vm = (NodeViewModel)this.DataContext;
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);

            base.OnApplyTemplate();
        }

        public void ToggleInkMode()
        {
            if (ManipulationMode == ManipulationModes.All)
            {
                ManipulationMode = ManipulationModes.None;
            }
            else
            {
                ManipulationMode = ManipulationModes.All;
            }

            var vm = (NodeViewModel)this.DataContext;
            vm.ToggleEditingInk();
            inkCanvas.InkPresenter.IsInputEnabled = vm.IsEditingInk;
        }

        private void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Remove();
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void OnResizerManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true;
        }

        private void Node_SelectionChanged(object sender, PropertyChangedEventArgs e)
        {     
            var slidein = (Storyboard)GetTemplateChild("slidein");
            var slideout = (Storyboard)GetTemplateChild("slideout");

            if (e.PropertyName.Equals("IsSelected"))
            {
                var vm = (NodeViewModel)this.DataContext;

                if (vm.IsSelected)
                {
                    slideout.Begin();
                }
                else
                {
                    slidein.Begin();
                }
            }
        }
    }
}
