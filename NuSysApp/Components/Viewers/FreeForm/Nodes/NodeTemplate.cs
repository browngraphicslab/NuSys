using System;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Devices.Input;
using Windows.Foundation;
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

        //public InqCanvasView inkCanvas = null;
        public Button btnDelete = null;
        public Path resizer = null;
        public Grid bg = null;
        public Rectangle hitArea = null;
        //public TextBlock tags = null;
        public Grid titleContainer = null;
        public TextBox title = null;
        public Border highlight = null;
        public ItemsControl tags = null;
        public TextBlock userName = null;

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
            var vm = (ElementViewModel)this.DataContext;
            
            bg = (Grid)GetTemplateChild("bg");
            hitArea = (Rectangle)GetTemplateChild("HitArea");
            
            
            //inkCanvas = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(vm.Width, vm.Height)));
        
            //(GetTemplateChild("xContainer") as Grid).Children.Add(inkCanvas);
 
            //inkCanvas.IsEnabled = false;
            //inkCanvas.Background = new SolidColorBrush(Colors.Aqua);
            //Canvas.SetZIndex(inkCanvas, -5);

            btnDelete = (Button)GetTemplateChild("btnDelete");
            btnDelete.Click += OnBtnDeleteClick;

            resizer = (Path)GetTemplateChild("Resizer");
            resizer.ManipulationDelta += OnResizerManipulationDelta;

            highlight = (Border)GetTemplateChild("xHighlight");
            userName = (TextBlock) GetTemplateChild("xUserName");

            //tags = (TextBlock)GetTemplateChild("Tags");
            //var t = new TranslateTransform {X = 0, Y = 25};
            //tags.RenderTransform = t;

            tags = (ItemsControl) GetTemplateChild("Tags");

            title = (TextBox)GetTemplateChild("xTitle");
            title.TextChanged += delegate(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs args)
            {
                titleContainer.RenderTransform = new TranslateTransform {X=0, Y= -title.ActualHeight + 5};
                highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                highlight.Height = vm.Height + title.ActualHeight - 5;
                vm.Controller.SetTitle(title.Text);
            };
            titleContainer = (Grid) GetTemplateChild("xTitleContainer");           

            title.Loaded += delegate(object sender, RoutedEventArgs args)
            {
                titleContainer.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                highlight.Height = vm.Height + title.ActualHeight - 5;
            };

            vm.Controller.UserChanged += delegate (NetworkUser user)
            {
                highlight.Visibility = vm.UserColor.Color == Colors.Transparent ? Visibility.Collapsed : Visibility.Visible;
                highlight.BorderBrush = vm.UserColor;
                userName.Foreground = vm.UserColor;
                userName.Text = user?.Name ?? "";
            };
            
            vm.PropertyChanged += OnPropertyChanged;
            base.OnApplyTemplate();
            OnTemplateReady?.Invoke();
        }

        public void ToggleInkMode()
        {
            var vm = (ElementViewModel)this.DataContext;
            //vm.ToggleEditingInk();
            //inkCanvas.IsEnabled = vm.IsEditingInk;
        }

        private void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {
            var model = (ElementModel)((ElementViewModel) this.DataContext).Model;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(model.Id));
        }


        private void OnResizerManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;
            
            var vm = (ElementViewModel)this.DataContext;

            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = vm.Model.Width + e.Delta.Translation.X/zoom;
            var resizeY = vm.Model.Height + e.Delta.Translation.Y/zoom;
            if (resizeY > 0 && resizeX > 0)
            {
            vm.Controller.SetSize(resizeX,resizeY);
            }
         //   inkCanvas.Width = vm.Width;
         //   inkCanvas.Height = vm.Height;
            e.Handled = true; 
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            if (e.PropertyName == "Height")
            {
                highlight.Height = vm.Height + title.ActualHeight - 5;
            }

            if (e.PropertyName == "IsSelected" || e.PropertyName == "IsEditing")
            {
                if (vm.IsSelected)
                {
                    bg.BorderBrush = new SolidColorBrush(Colors.CornflowerBlue);
                    bg.BorderThickness = new Thickness(3);
                    hitArea.Visibility = Visibility.Visible;
                }
                if (vm.IsEditing)
                {
                    bg.BorderBrush = new SolidColorBrush(Colors.YellowGreen);
                    bg.BorderThickness = new Thickness(3);
                    hitArea.Visibility = Visibility.Collapsed;
                }
                if (!(vm.IsEditing || vm.IsSelected))
                {
                    bg.BorderBrush = new SolidColorBrush(Colors.Transparent);
                    bg.BorderThickness = new Thickness(3);
                    hitArea.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
