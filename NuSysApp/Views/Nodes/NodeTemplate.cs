using System.ComponentModel;
using System.Diagnostics;
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

            ManipulationMode = ManipulationModes.All;
            ManipulationDelta += OnManipulationDelta;

            PointerReleased += OnPointerReleased;

            ManipulationCompleted += OnManipulationCompleted;

            var vm = (NodeViewModel)this.DataContext;
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_SelectionChanged);
            vm.PropertyChanged += new PropertyChangedEventHandler(Node_MultiSelectionChanged);

            base.OnApplyTemplate();

            OnTemplateReady?.Invoke();
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
            inkCanvas.IsEnabled = vm.IsEditingInk;
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
        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
//            var vm = (NodeViewModel)this.DataContext;
//            if (vm.WorkSpaceViewModel != null) { 
//                vm.CreateAnnotation();
//                vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
//            }
//                        if (vm.IsAnnotation)
//                        {
//                            SolidColorBrush backgroundColorBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(100, 111, 138, 150));
//                            nodeTpl.Background = backgroundColorBrush;
//                       }
            
            e.Handled = true;
        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            /*
            var vm = (NodeViewModel)this.DataContext;
            if (vm.WorkSpaceViewModel != null) { 
                vm.CreateAnnotation();
                vm.WorkSpaceViewModel.CheckForNodeNodeIntersection(vm); //TODO Eventually need to remove 
            }*/
            e.Handled = true;
        }

        private void Node_MultiSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("IsMultiSelected"))
            {
            }
            
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
                    if (vm.IsEditingInk == true)
                    {
                        vm.ToggleEditingInk();
                        inkCanvas.IsEnabled = vm.IsEditingInk;
                    if (ManipulationMode == ManipulationModes.All)
                    {
                        ManipulationMode = ManipulationModes.None;
                    }
                    else
                    {
                        ManipulationMode = ManipulationModes.All;
                    }
                    }
                }
                if (vm.GetType() == typeof (TextNodeViewModel))
                {

                    if (vm.IsEditing == true)
                    {
                        vm.ToggleEditing();
                        vm.IsEditing = false;
                    }
                }
            }
        }
    }
}
