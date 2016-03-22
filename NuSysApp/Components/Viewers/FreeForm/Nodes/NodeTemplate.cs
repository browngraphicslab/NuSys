using System;
using System.Collections.Generic;
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
        public Canvas xCanvas = null;
        public Button DuplicateElement = null;
        public Button Link = null;

        private Image _dragItem;

        private enum DragMode { Duplicate, Tag, Link };
        private DragMode _currenDragMode = DragMode.Duplicate;

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

            DuplicateElement = (Button)GetTemplateChild("DuplicateElement");
            Link = (Button)GetTemplateChild("Link");
            xCanvas = (Canvas)GetTemplateChild("xCanvas");

            DuplicateElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            Link.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);


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
            vm.Controller.LibraryElementModel.OnLightupContent += delegate (bool lightup)
             {
                 highlight.Visibility = lightup ? Visibility.Visible : Visibility.Collapsed;
                 highlight.BorderThickness = new Thickness(5);
                 highlight.BorderBrush = new SolidColorBrush(Colors.Aqua);
             };
            vm.PropertyChanged += OnPropertyChanged;
            base.OnApplyTemplate();
            OnTemplateReady?.Invoke();
        }

        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(null).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));

            if (_currenDragMode == DragMode.Duplicate)
            {
                
                var vm = (ElementViewModel)DataContext;
                vm.Controller.RequestDuplicate(r.X, r.Y, new Message(await vm.Model.Pack()));
            }

            /*
            if (_currenDragMode == DragMode.Tag)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                // hitsStart = hitsStart.Where(uiElem => uiElem);
                hitsStart = hitsStart.Where(ui => (ui as FrameworkElement).Name == "Tags");
                if (hitsStart.Any())
                {
                    var el = (FrameworkElement)hitsStart.First();
                    var vm = (ElementViewModel)el.DataContext;
                    var tags = (List<string>)vm.Model.GetMetaData("tags");
                    tags.Add(_text);
                    vm.Controller.SetMetadata("tags", tags);

                }
                else {

                    var contentId = SessionController.Instance.GenerateId();

                    var dict = new Message();
                    dict["width"] = 300;
                    dict["height"] = 150;
                    dict["nodeType"] = ElementType.Tag.ToString();
                    dict["x"] = r.X;
                    dict["y"] = r.Y;
                    dict["title"] = _text;
                    dict["contentId"] = contentId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Tag, dict.ContainsKey("title") ? dict["title"].ToString() : null));
                }
            }
            */

            if (_currenDragMode == DragMode.Link)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();
                if (hitsStart.Any())
                {
                    var first = (FrameworkElement)hitsStart.First();
                    var dc = (ElementViewModel)first.DataContext;
                    var vm = (ElementViewModel)DataContext;
                    if (vm == dc || (dc is FreeFormViewerViewModel))
                    {
                        return;
                    }
                    vm.Controller.RequestLinkTo(dc.Id);
                }
            }

            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));

        }

        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth / 2;
            t.TranslateY = p.Y - _dragItem.ActualHeight / 2;
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {
            CapturePointer(args.Pointer);

            if (sender == DuplicateElement)
            {
                _currenDragMode = DragMode.Duplicate;
            }

            if (sender == Link)
            {
                _currenDragMode = DragMode.Link;
            }


            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);
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
                    bg.BorderBrush = new SolidColorBrush(Color.FromArgb(255,156,197,194));
                    bg.BorderThickness = new Thickness(2);
                    hitArea.Visibility = Visibility.Visible;
                }
                if (vm.IsEditing)
                {
                    bg.BorderBrush = new SolidColorBrush(Color.FromArgb(255,197,158,156));
                    bg.BorderThickness = new Thickness(2);
                    hitArea.Visibility = Visibility.Collapsed;
                }
                if (!(vm.IsEditing || vm.IsSelected))
                {
                    bg.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 96, 109, 122));
                    bg.BorderThickness = new Thickness(2);
                    hitArea.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
