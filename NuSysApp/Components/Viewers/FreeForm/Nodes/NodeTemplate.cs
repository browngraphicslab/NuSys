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
using NuSysApp.Viewers;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace NuSysApp
{
    [TemplatePart(Name = "inkCanvas", Type = typeof(InqCanvasView))]
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
        public Button PresentationLink = null;
        public Button PresentationMode = null;

        public Button isSearched = null;


        private Image _dragItem;

        private enum DragMode { Duplicate, Tag, Link, PresentationLink };
        private DragMode _currenDragMode = DragMode.Duplicate;

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
            vm.Controller.LibraryElementController.UserChanged -= ControllerOnUserChanged;
            vm.Controller.LibraryElementController.TitleChanged -= LibraryElementModelOnOnTitleChanged;

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
            var vm = (ElementViewModel)this.DataContext;

            bg = (Grid)GetTemplateChild("bg");
            hitArea = (Rectangle)GetTemplateChild("HitArea");

            isSearched = (Button) GetTemplateChild("isSearched");
            
            //inkCanvas = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(vm.Width, vm.Height)));

            //(GetTemplateChild("xContainer") as Grid).Children.Add(inkCanvas);

            //inkCanvas.IsEnabled = false;
            //inkCanvas.Background = new SolidColorBrush(Colors.Aqua);
            //Canvas.SetZIndex(inkCanvas, -5);

            DuplicateElement = (Button)GetTemplateChild("DuplicateElement");
            Link = (Button)GetTemplateChild("Link");
            PresentationLink = (Button)GetTemplateChild("PresentationLink");
            xCanvas = (Canvas)GetTemplateChild("xCanvas");

            DuplicateElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            Link.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationLink.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            PresentationLink.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            PresentationMode = (Button) GetTemplateChild("PresentationMode");
            PresentationMode.Click += OnPresentationClick;

            btnDelete = (Button)GetTemplateChild("btnDelete");
            btnDelete.Click += OnBtnDeleteClick;

            resizer = (Path)GetTemplateChild("Resizer");
            resizer.ManipulationDelta += OnResizerManipulationDelta;

            highlight = (Border)GetTemplateChild("xHighlight");
            userName = (TextBlock)GetTemplateChild("xUserName");

            //tags = (TextBlock)GetTemplateChild("Tags");
            //var t = new TranslateTransform {X = 0, Y = 25};
            //tags.RenderTransform = t;

            tags = (ItemsControl)GetTemplateChild("Tags");

            title = (TextBox)GetTemplateChild("xTitle");
            title.KeyUp += TitleOnTextChanged;

            if (vm.Controller.LibraryElementModel != null)
                vm.Controller.LibraryElementController.TitleChanged += LibraryElementModelOnOnTitleChanged;
            titleContainer = (Grid)GetTemplateChild("xTitleContainer");

            title.Loaded += delegate (object sender, RoutedEventArgs args)
            {
                titleContainer.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
          //      highlight.Height = vm.Height + title.ActualHeight - 5;
            };


            //vm.Controller.LibraryElementController.UserChanged += ControllerOnUserChanged;

            vm.PropertyChanged += OnPropertyChanged;
            base.OnApplyTemplate();
            OnTemplateReady?.Invoke();
        }
        
        private void TitleOnTextChanged(object sender, object args)
        {
            var vm = (ElementViewModel)this.DataContext;
            titleContainer.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            highlight.Height = vm.Height + title.ActualHeight - 5;
            //vm.Controller.SetTitle(title.Text);
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
        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement) sender;
            if (_currenDragMode == DragMode.Duplicate)
            {
               
                var vm = (ElementViewModel)DataContext;
                

                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement) is GroupNodeView).ToList();

                if (hitsStart.Any())
                {
                    var first = (FrameworkElement) hitsStart.First();
                    var vm1 = (GroupNodeViewModel) first.DataContext;
                    var groupnode = (GroupNodeView)first;
                    var np = new Point(p.X - vm1.Model.Width / 2, p.Y - vm1.Model.Height / 2);
                    var canvas = groupnode.FreeFormView.AtomContainer;
                    var targetPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(canvas).TransformPoint(p);
                    p = args.GetCurrentPoint(first).Position; ;
                   
                   vm.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y, new Message(await vm.Model.Pack()));
                }
                else
                {
                    vm.Controller.RequestDuplicate(r.X, r.Y, new Message(await vm.Model.Pack()));
                }
            }
            
         

            if (_currenDragMode == DragMode.Link || _currenDragMode == DragMode.PresentationLink)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();

                var hitsStart2 = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart2 = hitsStart2.Where(uiElem => (uiElem as FrameworkElement).DataContext is LinkedTimeBlockViewModel).ToList();

                var hitRectangleView = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitRectangleView = hitRectangleView.Where(uiElem => (uiElem as FrameworkElement).DataContext is RectangleViewModel).ToList();
                
                if (hitsStart.Any()){
                    var first = (FrameworkElement)hitsStart.First();
                    var dc = (ElementViewModel)first.DataContext;
                    var vm = (ElementViewModel)DataContext;

                    if (vm == dc || (dc is FreeFormViewerViewModel) || dc is LinkViewModel)
                    {
                        return;
                    }

                    if (hitRectangleView.Any())
                    {
                        foreach (var element in hitRectangleView)
                        {
                            if (element is RectangleView)
                            {
                                Dictionary<string, object> inFgDictionary = vm.Controller.CreateTextDictionary(200, 100,
                                    100,
                                    200);
                                Dictionary<string, object> outFgDictionary = vm.Controller.CreateTextDictionary(100, 100,
                                    100,
                                    100);
                                if (_currenDragMode == DragMode.PresentationLink)
                                {
                                    vm.Controller.RequestPresentationLinkTo(dc.Id, (RectangleView)element, null, inFgDictionary,
                                        outFgDictionary);
                                }
                                else
                                {
                                    vm.Controller.RequestLinkTo(dc.Id, (RectangleView) element, null, inFgDictionary,
                                        outFgDictionary);
                                }
                            }
                        }
                    } else if (hitsStart2.Any()){
                        foreach (var element in hitsStart2)
                        {
                            if (element is LinkedTimeBlock)
                            {
                                Dictionary<string, object> inFgDictionary = vm.Controller.CreateTextDictionary(200, 100,
                                    100,
                                    200);
                                Dictionary<string, object> outFgDictionary = vm.Controller.CreateTextDictionary(100, 100,
                                    100,
                                    100);
                                Debug.WriteLine("test");
                                if (_currenDragMode == DragMode.PresentationLink)
                                {
                                    vm.Controller.RequestPresentationLinkTo(dc.Id, null, (LinkedTimeBlock)element, inFgDictionary,
                                           outFgDictionary);
                                }
                                else
                                {
                                    vm.Controller.RequestLinkTo(dc.Id, null, (LinkedTimeBlock) element, inFgDictionary,
                                        outFgDictionary);
                                }
                                //(element as LinkedTimeBlock).changeColor();
                                //vm.Controller.RequestLinkTo(dc.Id, (LinkedTimeBlock)element);

                            }
                        }
                    }
                    else
                    {
                        if (dc.LinkList.Where(c => c.OutElement.Model.Id == vm.Id).Count() > 0 || vm.LinkList.Where(c => c.OutElement.Model.Id == dc.Id).Count() > 0)
                        {
                            return;
                        }


                        if (_currenDragMode == DragMode.Link)
                            vm.Controller.RequestLinkTo(dc.Id);
                        if (_currenDragMode == DragMode.PresentationLink)
                            vm.Controller.RequestPresentationLinkTo(dc.Id);
                    }
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
            Debug.WriteLine("Starting once!");

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            CapturePointer(args.Pointer);

            if (sender == DuplicateElement)
            {
                _currenDragMode = DragMode.Duplicate;
            }

            if (sender == Link)
            {
                _currenDragMode = DragMode.Link;
            }

            if (sender == PresentationLink)
            {
                _currenDragMode = DragMode.PresentationLink;
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
            var model = (ElementModel)((ElementViewModel)this.DataContext).Model;
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(model.Id));
        }

        private void OnPresentationClick(object sender, RoutedEventArgs e)
        {
            
            var vm = ((ElementViewModel)this.DataContext);
            var sv = SessionController.Instance.SessionView;

            // unselect start element
            vm.IsSelected = false;
            vm.IsEditing = false;
            highlight.Visibility = Visibility.Collapsed;
            
            sv.EnterPresentationMode(vm);
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
                    highlight.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 156, 197, 194));
                    highlight.BorderThickness = new Thickness(2);
                    highlight.Background = new SolidColorBrush(Colors.Transparent);
                    bg.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 156, 197, 194));
                    bg.BorderThickness = new Thickness(2);
                    hitArea.Visibility = Visibility.Visible;
                }
                if (vm.IsEditing)
                {
                    highlight.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 131, 166, 163));
                    highlight.BorderThickness = new Thickness(2);
                    highlight.Background = new SolidColorBrush(Colors.Transparent);
                    bg.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 197, 158, 156));
                    bg.BorderThickness = new Thickness(2);
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

