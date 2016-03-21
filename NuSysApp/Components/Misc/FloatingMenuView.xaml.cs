using System;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
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

        public FloatingMenuView()
        {
            RenderTransform = new CompositeTransform();
            InitializeComponent();

            btnLibrary.Tapped += BtnLibrary_Tapped;
            btnAddNode.Tapped += BtnAddNode_Tapped;
            LibraryElementPropertiesWindow libProp = new LibraryElementPropertiesWindow();
            _lib = new LibraryView(new LibraryBucketViewModel(), libProp, this);
            xWrapper.Children.Add(_lib);
            xWrapper.Children.Add(libProp);
            libProp.Visibility = _lib.Visibility = Visibility.Collapsed;
            xAddNodeMenu.Visibility = Visibility.Collapsed;

            Canvas.SetTop(_lib, 80);
            Canvas.SetLeft(libProp, 400);
            AddNodeSubmenuButton(btnText);
            AddNodeSubmenuButton(btnRecording);
            AddNodeSubmenuButton(btnTag);
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
            if (xAddNodeMenu.Visibility == Visibility.Visible)
                xAddNodeMenu.Visibility = Visibility.Collapsed;
            else
                xAddNodeMenu.Visibility = Visibility.Visible;
        }

        private void BtnLibrary_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (_lib.Visibility == Visibility.Visible)
            {
                _lib.Visibility = Visibility.Collapsed;
                btnLibrary.Icon = "ms-appx:///Assets/icon_mainmenu_media.png";
            }
            else
            {
                _lib.Visibility = Visibility.Visible;
                btnLibrary.Icon = "ms-appx:///Assets/icon_mainmenu_collapse.png";
            }
        }

        private async void BtnAddNodeOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs args)
        {
            var c = (CompositeTransform) RenderTransform;
            
            xWrapper.Children.Remove(_dragItem);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = wvm.CompositeTransform.Inverse.TransformPoint(new Point(args.Position.X + c.TranslateX, args.Position.Y + c.TranslateY));
            await AddNode(new Point(r.X, r.Y), new Size(300, 300), _elementType);

        }
         
        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
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
            if (sender == btnText)
                _elementType = ElementType.Text;
            if (sender == btnRecording)
                _elementType = ElementType.Recording;

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
                var r = new RecordingNodeView(new ElementViewModel(new ElementController(new ElementModel("")
                {
                    X = pos.X,
                    Y = pos.Y,
                    Width = 300,
                    Height = 300
                })));

                vm.AtomViewList.Add(r);
                
            } else if (elementType == ElementType.Text) { 
                

       
            var p = pos;

            var dict = new Message();
            Dictionary<string, object> metadata;
           
            var contentId = SessionController.Instance.GenerateId();

            metadata = new Dictionary<string, object>();
            metadata["node_creation_date"] = DateTime.Now;
            metadata["node_type"] = elementType + "Node";

            dict = new Message();
            dict["width"] = size.Width.ToString();
            dict["height"] = size.Height.ToString();
            dict["nodeType"] = elementType.ToString();
            dict["x"] = (p.X - size.Width/2).ToString();
            dict["y"] = (p.Y - size.Height/2).ToString();
            dict["contentId"] = contentId;
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;
           
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, data == null ? "" : data.ToString(), elementType, dict.ContainsKey("title") ? dict["title"].ToString() : null));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(dict));

            }

            _lib.UpdateList();
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
    }
}