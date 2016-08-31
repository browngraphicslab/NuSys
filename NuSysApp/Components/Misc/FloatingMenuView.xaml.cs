using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;
using SharpDX.Direct2D1;
using Image = Windows.UI.Xaml.Controls.Image;
using SolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public enum Options
    {
        Idle,
        SelectNode,
        PenGlobalInk,
        Exploration,
        PanZoomOnly,
        Presentation
    }

    public sealed partial class FloatingMenuView : UserControl
    {
        private FreeFormViewer _freeFormViewer;
        private FrameworkElement _dragItem;
        private NusysConstants.ElementType _elementType;
        private LibraryView _lib;
        private LibraryElementPropertiesWindow libProp;
        private bool IsPenMode;
        private bool checkPointerAdded;
        private CompositeTransform floatingTransform;
        private Point _exportPos;
    

        public FloatingMenuView()
        {
            floatingTransform = new CompositeTransform();
            RenderTransform = floatingTransform;
            InitializeComponent();

            btnLibrary.Tapped += BtnLibrary_Tapped;
            btnAddNode.Tapped += BtnAddNode_Tapped;
            btnPen.Tapped += BtnPen_Tapped;
            btnSearch.Tapped += BtnSearch_Tapped;
             
            libProp = new LibraryElementPropertiesWindow();
            _lib = new LibraryView(new LibraryBucketViewModel(), libProp, this);
            xWrapper.Children.Add(_lib);
           
            xWrapper.Children.Add(libProp);
            libProp.Visibility = _lib.Visibility = Visibility.Collapsed;
            xAddNodeMenu.Visibility = Visibility.Collapsed;

            Canvas.SetTop(_lib, 100);
            //Canvas.SetLeft(_lib, 100);
            Canvas.SetTop(libProp, 100);
            Canvas.SetLeft(libProp, 450);
            AddNodeSubmenuButton(btnAddTextNode);
            AddNodeSubmenuButton(btnAddRecordingNode);
            AddNodeSubmenuButton(btnAddCollectionNode);
            AddNodeSubmenuButton(btnAddTools);
        }

        private void CheckPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var mainCanvas = SessionController.Instance.SessionView.MainCanvas;
            var position = e.GetCurrentPoint(mainCanvas).Position;

            var xdiff = addMenuTransform.X;
            var ydiff = addMenuTransform.Y;

            var xpos = floatingTransform.TranslateX + Canvas.GetLeft(SessionController.Instance.SessionView.FloatingMenu);
            var ypos = floatingTransform.TranslateY + Canvas.GetTop(SessionController.Instance.SessionView.FloatingMenu);

            var lefttop = new Point(xpos+xdiff, ypos+ydiff);

            if (position.X > lefttop.X && position.X < lefttop.X + 400)
            {
                if (position.Y > lefttop.Y && position.Y < lefttop.Y + 150) return;
            }

            //do we want this for the library?
            if (position.X > xpos && position.X < xpos + 450)
            {
                if (position.Y > ypos + 100 && position.Y < ypos + 650) return;
            }

            Reset();

        }

        /// <summary>
        /// Called when the user taps on the search icon in the floating menu view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSearch_Tapped(object sender, TappedRoutedEventArgs e)
        {

            var SearchViewer = SessionController.Instance.SessionView.SearchView;

            if (SearchViewer.Visibility == Visibility.Collapsed)
            {
                SearchViewer.Visibility = Visibility.Visible;
                SearchViewer.SetFocusOnSearchBox();
            }
            else
            {
                SearchViewer.Visibility = Visibility.Collapsed;
            }
        }

        public void Reset()
        {
            btnLibrary.Icon = "ms-appx:///Assets/icon_library.png";
            _lib.Visibility = _lib.Visibility = Visibility.Collapsed;
            xAddNodeMenu.Visibility = Visibility.Collapsed;
            libProp.Visibility = Visibility.Collapsed;
        }

        private void BtnPen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ActivatePenMode(!IsPenMode);
        }

        public void ActivatePenMode(bool val)
        {

            if (val)
            {
                if (IsPenMode)
                    return;
              //  SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Ink;
                IsPenMode = true;
                btnPen.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 197, 118, 97));

            }
            else
            {
                if (!IsPenMode)
                    return;
              //  SessionController.Instance.SessionView.FreeFormViewer.InqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
                IsPenMode = false;
                btnPen.Background = new Windows.UI.Xaml.Media.SolidColorBrush(Color.FromArgb(255, 197, 158, 156));
               

            }
        }
        private void AddNodeSubmenuButton(FrameworkElement btn)
        {
            btn.ManipulationMode = ManipulationModes.All;
            btn.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            btn.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            btn.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;
        }

        /// <summary>
        /// Called when the user taps on the add node icon in the floating menu view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddNode_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _lib.Visibility = Visibility.Collapsed;
            btnLibrary.Icon = "ms-appx:///Assets/icon_library.png";
            if (xAddNodeMenu.Visibility == Visibility.Visible)
            {
                xAddNodeMenu.Visibility = Visibility.Collapsed;
            }
            else {
                xAddNodeMenu.Visibility = Visibility.Visible;
            }

            //collapse things as necessary
            if (checkPointerAdded != true)
            {
                SessionController.Instance.SessionView.MainCanvas.PointerPressed += CheckPointerPressed;
                checkPointerAdded = true;
            }
            
        }

        /// <summary>
        /// Called when the user taps on the library icon in the floating menu view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnLibrary_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _lib.ToggleVisiblity();
            xAddNodeMenu.Visibility = Visibility.Collapsed;
            if (_lib.Visibility == Visibility.Visible)
            {
                btnLibrary.Icon = "ms-appx:///Assets/icon_whitex.png";
            }
            else
            {
                btnLibrary.Icon = "ms-appx:///Assets/icon_library.png";
            }

            //collapse things as necessary
            if (checkPointerAdded != true)
            {
                SessionController.Instance.SessionView.MainCanvas.PointerPressed += CheckPointerPressed;
                checkPointerAdded = true;
            }

        }

        /// <summary>
        /// When the user finishes dragging a button from the floating menu view, this method creates an element at the desired location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void BtnAddNodeOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            // Hide the library dragging rect
            var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;
            rect.Hide();

            // Add the element at the dropped location
            var p = args.Container.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.RenderCanvas).TransformPoint(_exportPos);
            var dropPoint = SessionController.Instance.SessionView.FreeFormViewer.InitialCollection.ScreenPointToObjectPoint(new Vector2((float)p.X, (float)p.Y));

            await AddElementToCollection(new Point(dropPoint.X, dropPoint.Y));

            args.Handled = true;

        }

        /// <summary>
        /// Called when the user moves the pointer after pressing a floating menu view icon, translates the libraryDragElement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            // Obtain the library dragging rectangle  
            var view = SessionController.Instance.SessionView;
            var rect = view.LibraryDraggingRectangle;

            // Update its transform
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += e.Delta.Translation.X;
            t.TranslateY += e.Delta.Translation.Y;
            
            // Update the position instance variable
            _exportPos.X += e.Delta.Translation.X;
            _exportPos.Y += e.Delta.Translation.Y;

            // Handled!
            e.Handled = true;
        }

        /// <summary>
        /// Called when the user presses an icon, instantiates libraryDragElement
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BtnAddNodeOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {

            // set the _elementType based on the sender
            if (sender == btnAddTextNode)
            {
                _elementType = NusysConstants.ElementType.Text;
            }
            else if (sender == btnAddRecordingNode)
            {
                _elementType = NusysConstants.ElementType.Recording;
            }
            else if (sender == btnAddCollectionNode)
            {
                _elementType = NusysConstants.ElementType.Collection;
            }
            else if (sender == btnAddTools)
            {
                _elementType = NusysConstants.ElementType.Tools;
            }
            else
            {
                Debug.Fail($"We do not have support for {_elementType} yet please add it yourself in the if statement above, the SwitchType method below and in OnManipulationCompleted");
            }

            // add the icon and start controlling the icon rect
            var view = SessionController.Instance.SessionView;
            view.LibraryDraggingRectangle.SetIcon(_elementType);
            view.LibraryDraggingRectangle.Show();
            var rect = view.LibraryDraggingRectangle;
            Canvas.SetZIndex(rect, 3);

            // Make the rectangle movable and set its position
            rect.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)rect.RenderTransform;
            t.TranslateX += _exportPos.X;
            t.TranslateY = _exportPos.Y;
            args.Handled = true;
        }

        /// <summary>
        /// Adds the element from the floating menu view to the collection at the specified point
        /// where the point is in workspace coordinates not the session view
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private async Task AddElementToCollection(Point position)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            // take care of filtering out elements that do not require requests to the server
            switch (_elementType)
            {
                case NusysConstants.ElementType.Collection:
                    break;
                case NusysConstants.ElementType.Text:
                    break;
                case NusysConstants.ElementType.Tools:
                    ToolFilterView filter = new ToolFilterView(position.X, position.Y);
                    vm.AtomViewList.Add(filter);
                    return;
                case NusysConstants.ElementType.Recording:
                    // add a recording node view to the collection
                    var r = new RecordingNodeView(new RecordingNodeViewModel(position.X, position.Y));
                    vm.AtomViewList.Add(r);
                    return;
                default:
                    Debug.Fail($"We do not support adding {_elementType} to the collection yet, please add support for it here");
                    return;
            }
            // Create a new content request
            var createNewContentRequestArgs = new CreateNewContentRequestArgs
            {
                LibraryElementArgs = new CreateNewLibraryElementRequestArgs
                {
                    AccessType =
                        SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                    LibraryElementType = _elementType,
                    Title = _elementType == NusysConstants.ElementType.Collection ? "Unnamed Collection" : "Unnamed Text",
                    LibraryElementId = SessionController.Instance.GenerateId()
                },
                ContentId = SessionController.Instance.GenerateId()
            };

            // execute the content request
            var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
            contentRequest.AddReturnedLibraryElementToLibrary();

            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                Height = Constants.DefaultNodeSize,
                Width = Constants.DefaultNodeSize,
                X = position.X,
                Y = position.Y
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();

            // remove any selections from the activeFreeFormViewer
            vm.ClearSelection();

        }

        public FrameworkElement Panel
        {
            get { return FloatingMenuPanel; }
        }

        public SessionView SessionView
        {
            get;set;
        }

        public LibraryView Library
        {
            get { return _lib; }
        }

        private void btnAdd_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var view = SessionController.Instance.SessionView;
            _exportPos.X = e.GetCurrentPoint(view).Position.X - 25;
            _exportPos.Y = e.GetCurrentPoint(view).Position.Y - 25;
            e.Handled = true;
        }
    }
}