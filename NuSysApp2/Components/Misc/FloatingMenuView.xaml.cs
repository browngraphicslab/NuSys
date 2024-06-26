﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX.Direct2D1;
using Image = Windows.UI.Xaml.Controls.Image;
using SolidColorBrush = Windows.UI.Xaml.Media.SolidColorBrush;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp2
{

    public enum Options
    {
        Idle,
        SelectNode,
        PenGlobalInk,
    }

    public sealed partial class FloatingMenuView : UserControl
    {
        private FreeFormViewer _freeFormViewer;
        private FrameworkElement _dragItem;
        private ElementType _elementType;
        private LibraryView _lib;
        private LibraryElementPropertiesWindow libProp;
        private bool IsPenMode;

        private bool checkPointerAdded;
        private CompositeTransform floatingTransform;

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
            AddNodeSubmenuButton(btnText);
            AddNodeSubmenuButton(btnRecording);
            AddNodeSubmenuButton(btnNew);
            AddNodeSubmenuButton(btnTools);
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
            btn.ManipulationStarting += BtnAddNodeOnManipulationStarting;
            btn.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            btn.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            btn.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;
        }

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

        private async void BtnAddNodeOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            xWrapper.Children.Remove(_dragItem);
           var r = xWrapper.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(args.Position.X, args.Position.Y));
           await AddNode(new Point(r.X, r.Y), new Size(300, 300), _elementType);

        }
         
        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Delta.Translation.X;
            t.TranslateY += args.Delta.Translation.Y;
            args.Handled = true;
        }

        private void BtnAddNodeOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            _dragItem.Opacity = 0.5;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Position.X - _dragItem.ActualWidth / 2;
            t.TranslateY += args.Position.Y - _dragItem.ActualHeight / 2;
            args.Handled = true;
        }

        private async void BtnAddNodeOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        {
            if (_dragItem != null && xWrapper.Children.Contains(_dragItem))
                xWrapper.Children.Remove(_dragItem);

      
            if (sender == btnText)
                _elementType = ElementType.Text;
            if (sender == btnRecording)
                _elementType = ElementType.Recording;
            if (sender == btnNew)
                _elementType = ElementType.Collection;
            if (sender == btnTools)
                _elementType = ElementType.Tools;

            args.Container = xWrapper;
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            var img = new Image();
            img.Opacity = 0;
            var t = new CompositeTransform();

            img.RenderTransform = new CompositeTransform();
            img.Source = bmp;


            _dragItem = img;

            xWrapper.Children.Add(_dragItem);
            args.Handled = true;
        }

        public async Task AddNode(Point pos, Size size, ElementType elementType, object data = null)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;
            if (elementType == ElementType.Recording)
            {
                var r = new RecordingNodeView(new RecordingNodeViewModel(new RecordingNodeController(new ElementModel("")
                {
                    X = pos.X,
                    Y = pos.Y,
                    Width = 300,
                    Height = 300
                })));

                vm.AtomViewList.Add(r);
                
            } else if (elementType == ElementType.Text || elementType == ElementType.Web || elementType == ElementType.Collection)
            {


                var title = string.Empty;
                if (elementType == ElementType.Text)
                    title = "Unnamed Text";
                if (elementType == ElementType.Collection)
                    title = "Unnamed Collection";
                var p = pos;

            var dict = new Message();
            Dictionary<string, object> metadata;
           
            var contentId = SessionController.Instance.GenerateId();

            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["type"] = elementType.ToString();
            dict["title"] = title;
            dict["x"] = (p.X).ToString();
            dict["y"] = (p.Y).ToString();
            dict["contentId"] = contentId;
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            dict["autoCreate"] = true;
           
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, data == null ? "" : data.ToString(), elementType, dict.ContainsKey("title") ? dict["title"].ToString() : null));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(dict));

            }

            // Adds a toolview to the atom view list when an tool is droped
            else if (_elementType == ElementType.Tools)
            {

                //var tool = new TemporaryToolView(new ElementViewModel(new ElementController(new ElementModel("")
                //{
                //    X = pos.X,
                //    Y = pos.Y,
                //    Width = 300,
                //    Height = 300,
                //})));
                //BasicToolModel model = new BasicToolModel();
                //BasicToolController controller = new BasicToolController(model);
                //BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
                //TemporaryToolView view = new TemporaryToolView(viewmodel, pos.X, pos.Y);
                ToolFilterView filter = new ToolFilterView(pos.X, pos.Y);
                //rect.Background = new SolidColorBrush(Colors.Blue);
                vm.AtomViewList.Add(filter);

            }

          //  _lib.UpdateList();
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

    }
}