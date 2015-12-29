﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NuSysApp
{
    [TemplatePart(Name = "inkCanvas", Type =typeof(InqCanvasView))]
    [TemplatePart(Name = "btnDelete", Type = typeof(Button))]
    [TemplatePart(Name = "resizer", Type = typeof(Path))]
    [TemplatePart(Name = "bg", Type = typeof(Grid))]
    public sealed class NodeTemplate : ContentControl
    {

        public event TemplateReady OnTemplateReady;
        public delegate void TemplateReady();

        public InqCanvasView inkCanvas = null;
        public Button btnDelete = null;
        public Path resizer = null;
        public Grid bg = null;
        public TextBlock tags = null;

        public NodeTemplate()
        {
            this.DefaultStyleKey = typeof(NodeTemplate);
            SubMenu = null;
            Inner = null;
        }

        public static readonly DependencyProperty SubMenuProperty = DependencyProperty.Register("SubMenu",
            typeof (object), typeof (NodeTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty InnerProperty = DependencyProperty.Register("Inner", typeof (object),
            typeof (NodeTemplate), new PropertyMetadata(null));

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
            inkCanvas = (InqCanvasView)GetTemplateChild("inkCanvas");
           
            bg = (Grid)GetTemplateChild("bg");
            
            btnDelete = (Button)GetTemplateChild("btnDelete");
            btnDelete.Click += OnBtnDeleteClick;

            resizer = (Path)GetTemplateChild("Resizer");
            resizer.ManipulationDelta += OnResizerManipulationDelta;

            tags = (TextBlock)GetTemplateChild("Tags");
            var t = new TranslateTransform {X = 0, Y = 25};
            tags.RenderTransform = t;
            
         

           // ManipulationMode = ManipulationModes.All;
            //ManipulationDelta += OnManipulationDelta;

            //ManipulationCompleted += OnManipulationCompleted;

            var vm = (NodeViewModel)this.DataContext;
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_MultiSelectionChanged);

            base.OnApplyTemplate();

            OnTemplateReady?.Invoke();

        }

        public void ToggleInkMode()
        {
            var vm = (NodeViewModel)this.DataContext;
            vm.ToggleEditingInk();
            inkCanvas.IsEnabled = vm.IsEditingInk;
        }

        private void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var model = (NodeModel)((NodeViewModel) this.DataContext).Model;
            NetworkConnector.Instance.RequestDeleteSendable(model.Id);
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (NodeViewModel)this.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
           // e.Handled = true;

        }

        private void OnResizerManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;
            
            var vm = (NodeViewModel)this.DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            e.Handled = true; 

        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //TODO: re-add
            /*
            var vm = (NodeViewModel)this.DataContext;
            if (vm.WorkSpaceViewModel != null) { 
                vm.CreateAnnotation();
                vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
            }
            */
            //e.Handled = true;
        }

        private void Node_MultiSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsMultiSelected"))
            {
            }
            
        }
    }
}
