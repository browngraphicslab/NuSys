using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Nodes;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Nodes.AudioNode;
using Windows.Graphics.Imaging;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.Storage.Pickers;
using MyToolkit.UI;
using NuSysApp.Viewers;
using System.Threading.Tasks;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NuSysApp
{
    [TemplatePart(Name = "inkCanvas", Type = typeof(InqCanvasView))]
    [TemplatePart(Name = "resizer", Type = typeof(Path))]
    public sealed class NodeTemplate : ContentControl
    {

        public event TemplateReady OnTemplateReady;
        public delegate void TemplateReady();

        //public InqCanvasView inkCanvas = null;
        public Polygon resizer = null;
        public Rectangle hitArea = null;
        //public TextBlock tags = null;
        public Grid titleContainer = null;
        public Grid bg = null;
        public TextBox title = null;
        public Border highlight = null;
        public ItemsControl tags = null;
        public TextBlock userName = null;
        public Canvas xCanvas = null;

        public Canvas xContent = null;
        //public Button DuplicateElement = null;
        // public Button Link = null;
        // public Button PresentationLink = null;
        //public Button PresentationMode = null;


        public Button isSearched = null;
        
        public NodeTemplate()
        {
            this.DefaultStyleKey = typeof(NodeTemplate);
            SubMenu = null;
            Inner = null;
        }

        public void Dispose()
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.PropertyChanged -= OnPropertyChanged;

            if (vm.Controller.LibraryElementController != null)
            {
                vm.Controller.LibraryElementController.UserChanged -= ControllerOnUserChanged;
                vm.Controller.LibraryElementController.TitleChanged -= LibraryElementModelOnOnTitleChanged;
            }
            if (title != null)
                title.TextChanged -= TitleOnTextChanged;

        }

        public static readonly DependencyProperty SubMenuProperty = DependencyProperty.Register("SubMenu",
            typeof(object), typeof(NodeTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty InnerProperty = DependencyProperty.Register("Inner", typeof(object),
            typeof(NodeTemplate), new PropertyMetadata(null));

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
            hitArea = (Rectangle)GetTemplateChild("HitArea");
            isSearched = (Button)GetTemplateChild("isSearched");

            //inkCanvas = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(vm.Width, vm.Height)));

            //(GetTemplateChild("xContainer") as Grid).Children.Add(inkCanvas);

            //inkCanvas.IsEnabled = false;
            //inkCanvas.Background = new SolidColorBrush(Colors.Aqua);
            //Canvas.SetZIndex(inkCanvas, -5);

            //DuplicateElement = (Button)GetTemplateChild("DuplicateElement");
            // Link = (Button)GetTemplateChild("Link");
            //PresentationLink = (Button)GetTemplateChild("PresentationLink");
            xCanvas = (Canvas)GetTemplateChild("xCanvas");

            /*
            DuplicateElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            Link.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationLink.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            PresentationLink.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationMode = (Button) GetTemplateChild("PresentationMode");
            PresentationMode.Click += OnPresentationClick;
            */

            bg = (Grid)GetTemplateChild("bg");
            resizer = (Polygon)GetTemplateChild("Resizer");

            resizer.ManipulationDelta += OnResizerManipulationDelta;
            highlight = (Border)GetTemplateChild("xHighlight");
            userName = (TextBlock)GetTemplateChild("xUserName");
            xContent = (Canvas)GetTemplateChild("xContent");

            //tags = (TextBlock)GetTemplateChild("Tags");
            //var t = new TranslateTransform {X = 0, Y = 25};
            //tags.RenderTransform = t;

            tags = (ItemsControl)GetTemplateChild("Tags");
          


            title = (TextBox)GetTemplateChild("xTitle");
            title.KeyUp += TitleOnTextChanged;

            var vm = DataContext as ElementViewModel;

            vm.Controller.SizeChanged += OnSizeChanged;
            SessionController.Instance.SessionView.FreeFormViewer.PanZoom.Update += OnPanZoom;
            if (vm?.Controller.LibraryElementModel != null)
            {
                vm.Controller.LibraryElementController.TitleChanged += LibraryElementModelOnOnTitleChanged;
            }
            titleContainer = (Grid)GetTemplateChild("xTitleContainer");

            title.Loaded += delegate (object sender, RoutedEventArgs args)
            {
                highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                //      highlight.Height = vm.Height + title.ActualHeight - 5;
            };

            Canvas.SetLeft(resizer, vm.Width - 60);
            Canvas.SetTop(resizer, vm.Height - 60);

            hitArea.Width = vm.Width;
            hitArea.Height = vm.Height + 70 * SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var hitAreaTsfm = (TranslateTransform)hitArea.RenderTransform;
         //  hitAreaTsfm.Y = -50;

            //vm.Controller.LibraryElementController.UserChanged += ControllerOnUserChanged;

            vm.PropertyChanged += OnPropertyChanged;
            base.OnApplyTemplate();
            Rearrange(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);
            OnTemplateReady?.Invoke();
        }
        private void OnSizeChanged(object source, double width, double height)
        {
            hitArea.Width = width;
            hitArea.Height = height + 50;
            var hitAreaTsfm = (TranslateTransform)hitArea.RenderTransform;
           // hitAreaTsfm.Y = -50 * SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
           // Debug.WriteLine(hitAreaTsfm.Y);
            Rearrange(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);
        }

        private void Rearrange(CompositeTransform transform)
        {
            var vm = DataContext as ElementViewModel;
            if (vm == null)
                return;
            var resizerTsfm = (ScaleTransform)resizer.RenderTransform;
            var titleTsfm = (ScaleTransform)title.RenderTransform;
            var hitAreaTsfm = (TranslateTransform)hitArea.RenderTransform;
            resizerTsfm.ScaleX = resizerTsfm.ScaleY = 1 / transform.ScaleX * 97 / 72.0;
            titleTsfm.ScaleX = titleTsfm.ScaleY = 1 / transform.ScaleX * 97 / 72.0;
            
            if (title.Text.Length > 0) { 
                var rect = title.GetRectFromCharacterIndex(title.Text.Length - 1, true);
                var p = title.Padding;
                var w = rect.X + rect.Width + p.Left + p.Right + 10;
                w = w < 1 ? 100 : w;
                title.Width = w;
                Canvas.SetLeft(title, (vm.Width - w) / 2);
            }

            Canvas.SetLeft(resizer, vm.Width - 60);
            Canvas.SetTop(resizer, vm.Height - 60);


            Canvas.SetTop(hitArea, -70 * 97 / 72.0 / transform.ScaleX);
            hitArea.Width = vm.Width;
            hitArea.Height = vm.Height + 70 * 97 / 72.0 / transform.ScaleX;

            // Fade out content if it occupies to little screen space
            /*                        
            var screenW = SessionController.Instance.SessionView.ActualWidth;
            var screenH = SessionController.Instance.SessionView.ActualHeight;
            var areaRatio = (Math.Sqrt(hitArea.Width * hitArea.Height) * transform.ScaleX) / Math.Sqrt(screenW * screenH);
            bg.BorderThickness = new Thickness(2 / transform.ScaleX, 2 / transform.ScaleX, 2 / transform.ScaleX, 2 / transform.ScaleX);


            if (areaRatio <= 0.1) { 
                xContent.Opacity = Math.Pow(areaRatio / 0.1, 6);
              // Canvas.SetTop(title, (-50 - (50 * Math.Pow(areaRatio / 0.1, 6) - 50)) / transform.ScaleX);
            }
            else { 
                xContent.Opacity = 1;
                Canvas.SetTop(title, -50 / transform.ScaleX);
            }
            */
        }

        private void OnPanZoom(CompositeTransform transform)
        {
            Rearrange(transform);            
        }

        private void TitleOnTextChanged(object sender, object args)
        {
            var vm = (ElementViewModel)this.DataContext;
            highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            highlight.Height = vm.Height + title.ActualHeight - 5;
            vm.Controller.LibraryElementController.SetTitle(title.Text);
        }

        private void LibraryElementModelOnOnTitleChanged(object sender, string newTitle)
        {
            var vm = (ElementViewModel)this.DataContext;
            if (title.Text != newTitle)
            {
                title.TextChanged -= TitleOnTextChanged;
                title.Text = vm.Controller.LibraryElementModel.Title;
                title.TextChanged += TitleOnTextChanged;
                
            }
            Rearrange(SessionController.Instance.ActiveFreeFormViewer.CompositeTransform);

        }

        private void ControllerOnUserChanged(object sender, NetworkUser user)
        {
            var vm = (ElementViewModel)this.DataContext;
            if (user == null)
            {
                userName.Foreground = new SolidColorBrush(Colors.Transparent);
                highlight.Visibility = Visibility.Collapsed;
            }
            else
            {
                highlight.Visibility = Visibility.Visible;
                highlight.BorderBrush = new SolidColorBrush(user.Color);
                userName.Foreground = new SolidColorBrush(user.Color);
                userName.Text = user?.Name ?? "";
            }
            
            
        }

        private void LibraryElementModelOnSearched(LibraryElementModel model, bool searched)
        {
            isSearched.Visibility = searched ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ToggleInkMode()
        {
            var vm = (ElementViewModel)this.DataContext;
            //vm.ToggleEditingInk();
            //inkCanvas.IsEnabled = vm.IsEditingInk;
        }

        

        private void OnExplorationClick(object sender, RoutedEventArgs e)
        {

            var vm = ((ElementViewModel)this.DataContext);
            var sv = SessionController.Instance.SessionView;

            // unselect start element
            vm.IsSelected = false;
            vm.IsEditing = false;
            highlight.Visibility = Visibility.Collapsed;

            sv.EnterExplorationMode(vm);
        }

        private void OnResizerManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (ElementViewModel)this.DataContext;

            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = vm.Model.Width + e.Delta.Translation.X / zoom;
            var resizeY = vm.Model.Height + e.Delta.Translation.Y / zoom;
            if (resizeY > 0 && resizeX > 0)
            {
                var ratio = vm.GetRatio();
                if(ratio == 1.01010101)
                {
                    vm.Controller.SetSize(resizeX, resizeY);
                }
                else
                {
                    vm.Controller.SetSize(resizeX, resizeX * ratio);
                }
            }

            SessionController.Instance.SessionView.FreeFormViewer.UpdateNodePosition();
            e.Handled = true;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
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
                    highlight.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 156, 197, 194));
                    highlight.BorderThickness = new Thickness(2);
                    highlight.Background = new SolidColorBrush(Colors.Transparent);
                    hitArea.Visibility = Visibility.Visible;
                }
                if (vm.IsEditing)
                {
                    highlight.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 131, 166, 163));
                    highlight.BorderThickness = new Thickness(2);
                    highlight.Background = new SolidColorBrush(Colors.Transparent);
                    await Task.Delay(200);
                    hitArea.Visibility = Visibility.Collapsed;
                }
                if (!(vm.IsEditing || vm.IsSelected))
                {
                    highlight.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 131, 166, 163));
                    highlight.BorderThickness = new Thickness(1);
                    hitArea.Visibility = Visibility.Visible;
                }
            }
        }       
    }
}

