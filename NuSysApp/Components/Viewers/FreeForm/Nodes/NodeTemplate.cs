using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
using Newtonsoft.Json;
using NusysIntermediate;

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
        public Button ExplorationMode = null;

        public Button isSearched = null;

        /// <summary>
        /// Used to hold the icon indicating the position and type of anything being dragged from the node template
        /// </summary>
        private Image _dragItem;

        /// <summary>
        /// An enum of the differnt types of dragging that can occur on element view models
        /// </summary>
        private enum DragMode
        {
            /// <summary>
            /// Drag Mode for adding a duplicate of an element view model
            /// </summary>
            Duplicate,
            /// <summary>
            /// Unused drag mode. If this changes, change this comment.
            /// </summary>
            Tag,
            /// <summary>
            /// Drag mode for creating a new link between two element view models
            /// </summary>
            Link,
            /// <summary>
            /// Drag mode for creating a new presentation link between two element view models
            /// </summary>
            PresentationLink
        }
        /// <summary>
        /// The drag mode we are currently in, used in switch cases to determine drag bheavior
        /// </summary>
        private DragMode _currentDragMode = DragMode.Duplicate;

        public NodeTemplate()
        {
            this.DefaultStyleKey = typeof(NodeTemplate);
            SubMenu = null;
            TopMenu = null;
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

        public static readonly DependencyProperty TopMenuProperty = DependencyProperty.Register("TopMenu",
            typeof(object), typeof(NodeTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty InnerProperty = DependencyProperty.Register("Inner", typeof(object),
            typeof(NodeTemplate), new PropertyMetadata(null));

        public object SubMenu
        {
            get { return (object)GetValue(SubMenuProperty); }
            set { SetValue(SubMenuProperty, value); }
        }

        public object TopMenu
        {
            get { return (object)GetValue(TopMenuProperty); }
            set { SetValue(TopMenuProperty, value); }
        }

        public object Inner
        {
            get { return (object)GetValue(InnerProperty); }
            set { SetValue(InnerProperty, value); }
        }

        public void HideResizer()
        {
            resizer.Visibility = Visibility.Collapsed;
        }

        protected override void OnApplyTemplate()
        {
            bg = (Grid)GetTemplateChild("bg");
            hitArea = (Rectangle)GetTemplateChild("HitArea");

            isSearched = (Button) GetTemplateChild("isSearched");
            
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

            ExplorationMode = (Button) GetTemplateChild("ExplorationMode");
            ExplorationMode.Click += OnExplorationClick;

            

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
            tags.Tapped += Tags_Tapped;

            title = (TextBox)GetTemplateChild("xTitle");
            title.KeyUp += TitleOnTextChanged;

            title.GotFocus += Title_GotFocus;
            title.LostFocus += Title_LostFocus;

            if ((DataContext as ElementViewModel)?.Controller.LibraryElementModel != null)
            {
                (DataContext as ElementViewModel).Controller.LibraryElementController.TitleChanged +=
                    LibraryElementModelOnOnTitleChanged;
            }
            titleContainer = (Grid)GetTemplateChild("xTitleContainer");

            title.Loaded += delegate (object sender, RoutedEventArgs args)
            {
                titleContainer.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
                highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            };

            var vm = (ElementViewModel)this.DataContext;
            if (vm?.Controller?.LibraryElementModel != null)
            {
                vm.Controller.LibraryElementController.UserChanged += ControllerOnUserChanged;
            }

            (DataContext as BaseINPC).PropertyChanged += OnPropertyChanged;
            base.OnApplyTemplate();
            OnTemplateReady?.Invoke();
        }



        private void Tags_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var selectedTag = (e.OriginalSource as TextBlock)?.Text;
            if (selectedTag != null)
            {
                MetadataToolModel model = new MetadataToolModel();
                MetadataToolController controller = new MetadataToolController(model);
                MetadataToolViewModel viewmodel = new MetadataToolViewModel(controller);

                viewmodel.Filter = ToolModel.ToolFilterTypeTitle.AllMetadata;

                controller.SetSelection(new Tuple<string, HashSet<string>>("Keywords", new HashSet<string>() { }));

                var wvm = SessionController.Instance.ActiveFreeFormViewer;
                var width = SessionController.Instance.SessionView.ActualWidth;
                var height = SessionController.Instance.SessionView.ActualHeight;
                var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));
                MetadataToolView view = new MetadataToolView(viewmodel, centerpoint.X, centerpoint.Y);
                wvm.AtomViewList.Add(view);
            }
        }

        private void OnTagTemplateTapped(object sender, TappedRoutedEventArgs e)
        {
            var panel = sender as WrapPanel;
            var text = panel.Children;
        }


        private void TitleOnTextChanged(object sender, object args)
        {
            var vm = (ElementViewModel)this.DataContext;
            titleContainer.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            highlight.RenderTransform = new TranslateTransform { X = 0, Y = -title.ActualHeight + 5 };
            highlight.Height = vm.Height + title.ActualHeight - 5;
            //vm.LibraryElementController.SetTitle(title.Text);
            vm.Controller.LibraryElementController.TitleChanged -= LibraryElementModelOnOnTitleChanged;
            vm.Controller.LibraryElementController.SetTitle(title.Text);
            vm.Controller.LibraryElementController.TitleChanged += LibraryElementModelOnOnTitleChanged;
        }

        private void LibraryElementModelOnOnTitleChanged(object sender, string newTitle)
        {
            var vm = (ElementViewModel)this.DataContext;
            if (title.Text != newTitle)
            {
                title.TextChanged -= TitleOnTextChanged;
                title.Text = newTitle;
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
                userName.Text = user?.DisplayName ?? "";
            }
            
            
        }

        private void LibraryElementModelOnSearched(LibraryElementModel model, bool searched)
        {
            isSearched.Visibility = searched ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Single method used for any drag and drop button, add functionality by creating a DragMode enum
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            // Remove the drag button from the session
            xCanvas.Children.Remove(_dragItem);

            // Find the coordinates of the point at which the client dropped the button, and use those coordinates in switch below depending on the currenDragMode
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));

            // vm is the element view model we are dragging from
            var vm = (ElementViewModel)DataContext;
            // used to hold the first element returned from the VisualTreeHelper
            FrameworkElement first;
            switch (_currentDragMode)
            {
                case DragMode.Duplicate:
                    // use hitsStart to determine if we are dragging a duplicate into a GroupNodeView (collection)
                    var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null).ToList();
                    hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement) is GroupNodeView).ToList();

                    // if we have dragged the duplicate into a groupNodeView, we just create the duplicate anyway, so this might be useless
                    if (hitsStart.Any())
                    {
                        first = (FrameworkElement)hitsStart.First();
                        var groupnode = (GroupNodeView)first;
                        var canvas = groupnode.FreeFormView.AtomContainer;
                        var targetPoint = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(canvas).TransformPoint(p);
                        vm.Controller.RequestDuplicate(targetPoint.X, targetPoint.Y);
                    }
                    // if we have not dragged the duplicate into a groupNodeView then we request a duplicate using the rectangle of the drag release coordinates r
                    else
                    {
                        vm.Controller.RequestDuplicate(r.X, r.Y);
                    }
                    break;
                case DragMode.Link:
                    // get a list of UIElements which exist at the current point
                    hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null).ToList();
                    // get a list of any ElementViewModels at the current point
                    var hitsStartElements = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();
                    // get a list of any RegionViewModels at the current point
                    var hitStartRegions = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is RegionViewModel).ToList();
                    // create a new instance of LinkLibraryElementRequestArgs, and pass in the id of the current element view model as the ElementViewModelInId.
                    var createNewLinkLibraryElementRequestArgs = new CreateNewLinkLibraryElementRequestArgs();
                    createNewLinkLibraryElementRequestArgs.LibraryElementModelInId = vm.LibraryElementId;
                    createNewLinkLibraryElementRequestArgs.LibraryElementType = NusysConstants.ElementType.Link;

                    if (hitStartRegions.Any())
                    {
                        // If it hits a region then pass in information about that region to createNewLinkLibraryElementRequestArgs
                        first = (FrameworkElement)hitStartRegions.First();
                        var dc = (RegionViewModel)first.DataContext;
                        createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId = dc.RegionLibraryElementController.LibraryElementModel.LibraryElementId;
                        createNewLinkLibraryElementRequestArgs.Title = $"Link from {vm.Model.Title} to {dc.Model.Title}";
                    }
                    else if (hitsStartElements.Any())
                    {
                        // if it hits an element then pass in information about that element to createNewLinkLibraryElementRequestArgs
                        first = (FrameworkElement)hitsStartElements.First();
                        var dc = (ElementViewModel)first.DataContext;

                        // Diable linking to links, tools and collections
                        // TODO: Enable linking to links 
                        if (dc.ElementType == NusysConstants.ElementType.Link || dc.ElementType == NusysConstants.ElementType.Tools  || dc.ElementType == NusysConstants.ElementType.Collection)
                        {
                            break;
                        }

                        createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId = dc.LibraryElementId;
                        createNewLinkLibraryElementRequestArgs.Title = $"Link from {vm.Model.Title} to {dc.Model.Title}";
                    }
                    else
                    {
                        // if we didn't hit an element or a region then break here
                        break;
                    }
                    // if the link is between two different libary element models then execute the create link request
                    if (createNewLinkLibraryElementRequestArgs.LibraryElementModelInId != createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId && 
                        SessionController.Instance.LinksController.GetLinkLibraryElementControllerBetweenContent(createNewLinkLibraryElementRequestArgs.LibraryElementModelInId,createNewLinkLibraryElementRequestArgs.LibraryElementModelOutId) != null)
                    {
                        var contentRequestArgs = new CreateNewContentRequestArgs();
                        contentRequestArgs.LibraryElementArgs = createNewLinkLibraryElementRequestArgs;
                        var request = new CreateNewContentRequest(contentRequestArgs);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                        request.AddReturnedLibraryElementToLibrary();
                    }
                    break;
                case DragMode.PresentationLink:
                    // get a list of UIElements which exist at the current point
                    hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null).ToList();
                    // get a list of any ElementViewModels at the current point
                    hitsStartElements = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();

                    // create a new instance of CreateNewPresentationLinkRequestArgs
                    var createNewPresentationLinkRequestArgs = new CreateNewPresentationLinkRequestArgs();
                    // pass in the id of the current element view model as the ElementViewModelInId.
                    createNewPresentationLinkRequestArgs.ElementViewModelInId = vm.Id;
                    // pass in the parent collection id of the element model as the parent collection id
                    createNewPresentationLinkRequestArgs.ParentCollectionId = vm.Model.ParentCollectionId;

                    // if an element exists at the current point
                    if (hitsStartElements.Any())
                    {
                        first = (FrameworkElement) hitsStartElements.First();
                        var dc1 = (ElementViewModel) first.DataContext;

                        // If trying to create a presentation link to collection, do nothing.
                        if (dc1.Model.ElementType == NusysConstants.ElementType.Collection)
                        {
                            break;
                        }
                        createNewPresentationLinkRequestArgs.ElementViewModelOutId = dc1.Id;
                    }
                    else
                    {
                        // if we didn't hit an element or a region then break here
                        break;
                    }

                    // if the link is between two different libary element models then execute the create link request
                    if (createNewPresentationLinkRequestArgs.ElementViewModelInId != createNewPresentationLinkRequestArgs.ElementViewModelOutId)
                    {
                        var request = new CreateNewPresentationLinkRequest(createNewPresentationLinkRequestArgs);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                        request.AddPresentationLinkToLibrary();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_currentDragMode), $"The passed in currentDragMode {_currentDragMode} has no case in the switch statement. Add its behavior to the switch statement.");
            }

            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));
        }

        /// <summary>
        /// creates a presentation link between to two elements whose id's are passed in
        /// </summary>
        /// <param name="elementId1"></param>
        /// <param name="elementId2"></param>
        private void AddPresentationLink(string elementId1, string elementId2)
        {
            var currentCollection = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;

            Debug.Assert(elementId1 != null);
            Debug.Assert(elementId2 != null);
            Debug.Assert(currentCollection != null);
            Debug.Assert(SessionController.Instance.IdToControllers.ContainsKey(elementId1));
            Debug.Assert(SessionController.Instance.IdToControllers.ContainsKey(elementId2));

            //SessionController.Instance.NuSysNetworkSession.AddPresentationLink(elementId1, elementId2, currentCollection);
        }

        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform) _dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth/2;
            t.TranslateY = p.Y - _dragItem.ActualHeight/2;
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {
            Debug.WriteLine("Starting once!");

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            CapturePointer(args.Pointer);

            if (sender == DuplicateElement)
            {
                _currentDragMode = DragMode.Duplicate;
            }

            if (sender == Link)
            {
                _currentDragMode = DragMode.Link;
            }

            if (sender == PresentationLink)
            {
                _currentDragMode = DragMode.PresentationLink;
            }


            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement) sender);
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
            var vm = (ElementViewModel) this.DataContext;
            //vm.ToggleEditingInk();
            //inkCanvas.IsEnabled = vm.IsEditingInk;
        }

        private void OnBtnDeleteClick(object sender, RoutedEventArgs e)
        {

           
            var vm = (ElementViewModel)this.DataContext;
            var model = (ElementModel) vm.Model;
            
            //Creates a DeleteElementAction
            var removeElementAction = new DeleteElementAction(vm.Controller);

            //Creates an undo button and places it in the correct position.

            var position = new Point(model.X, model.Y);
            var workspace = SessionController.Instance.ActiveFreeFormViewer;
            var undoButton = new UndoButton();
            workspace.AtomViewList.Add(undoButton);
            undoButton.MoveTo(position);
            undoButton.Activate(removeElementAction);
            //TODO fix this 817
            
            vm.Controller.RequestDelete();




        }

        private void OnPresentationClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ElementViewModel;
            Debug.Assert(vm != null);

            var sv = SessionController.Instance.SessionView;

            // unselect start element
            vm.IsSelected = false;
            vm.IsEditing = false;
            highlight.Visibility = Visibility.Collapsed;

            sv.EnterPresentationMode(vm);
        }

        private void OnExplorationClick(object sender, RoutedEventArgs e)
        {
            var vm = ((ElementViewModel) this.DataContext);

            // unselect start element
            vm.IsSelected = false;
            vm.IsEditing = false;
            highlight.Visibility = Visibility.Collapsed;

            SessionController.Instance.SwitchMode(Options.Exploration);         
        }

        private void OnResizerManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (ElementViewModel) this.DataContext;

            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = vm.Model.Width + e.Delta.Translation.X/zoom;
            var resizeY = vm.Model.Height + e.Delta.Translation.Y/zoom;
            if (resizeY > Constants.MinNodeSize && resizeX > Constants.MinNodeSize)
            {
                vm.Controller.SetSize(resizeX, resizeY);
            }
            //   inkCanvas.Width = vm.Width;
            //   inkCanvas.Height = vm.Height;
            e.Handled = true;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = (ElementViewModel) this.DataContext;
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
                    highlight.BorderBrush = new SolidColorBrush(Colors.Black);
                    highlight.BorderThickness = new Thickness(1);
                    highlight.Visibility = Visibility.Collapsed;
                    hitArea.Visibility = Visibility.Visible;
                    bg.BorderBrush = new SolidColorBrush(Colors.Black);
                    bg.BorderThickness = new Thickness(1);
                }
            }
        }

        private void Title_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            Debug.Assert(tb != null);
            tb.IsReadOnly = true;
        }

        private void Title_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            Debug.Assert(tb != null);
            tb.IsReadOnly = false;
        }
    }
}

