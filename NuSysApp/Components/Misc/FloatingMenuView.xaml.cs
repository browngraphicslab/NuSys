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
        MainSelect,
            SelectNode,
            SelectMarquee,
        MainSearch,
        MainPen,
            PenGlobalInk,
            PenErase,
            PenHighlight,
        MainAdd,
            AddTextNode,
            AddInkNode,
            AddMedia,
            AddWeb,
            AddAudioCapture,
            AddRecord,
            AddBucket,
            AddVideo,
        MainSaveLoad,
            Load,
            Save,
        MainMisc,
            MiscLoad,
            MiscSave,
            MiscPin,
            MiscUsers
    }

    public sealed partial class FloatingMenuView : UserControl
    {
        private FreeFormViewer _freeFormViewer;
        private FrameworkElement _dragItem;
        private ElementType _elementType;
        private LibraryView _lib;

        /// <summary>
        /// Maps all buttons to its corresponding enum entry.
        /// </summary>
        private readonly BiDictionary<FloatingMenuButtonView, Options> _buttons;

       

        /// <summary>
        /// Maps each main menu button to its current active submenu button
        /// </summary>
        private Dictionary<FloatingMenuButtonView, FloatingMenuButtonView> _activeSubMenuButtons;

        public FloatingMenuView()
        {
            DataContext = new FloatingMenuViewModel();
            this.InitializeComponent();
            
            btnLibrary.ManipulationMode = ManipulationModes.All;
            btnAddNode.ManipulationMode = ManipulationModes.All;

            btnAddNode.ManipulationStarting += BtnAddNodeOnManipulationStarting;
            btnAddNode.ManipulationStarted += BtnAddNodeOnManipulationStarted;
            btnAddNode.ManipulationDelta += BtnAddNodeOnManipulationDelta;
            btnAddNode.ManipulationCompleted += BtnAddNodeOnManipulationCompleted;

            btnLibrary.Tapped += BtnLibrary_Tapped;
            LibraryElementPropertiesWindow libProp = new LibraryElementPropertiesWindow();
            _lib = new LibraryView(new LibraryBucketViewModel(), libProp, this);
            xWrapper.Children.Add(_lib);
            xWrapper.Children.Add(libProp);
            libProp.Visibility = Visibility.Collapsed;
            Canvas.SetLeft(_lib, 100);
            Canvas.SetTop(_lib, 110);
            Canvas.SetLeft(libProp, 545);
            Canvas.SetTop(libProp, 160);
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
            xWrapper.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(args.Position.X, args.Position.Y, 300, 300));
            await AddNode(new Point(r.X, r.Y), new Size(r.Width, r.Height), _elementType);

        }

        private void BtnAddNodeOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Delta.Translation.X;
            t.TranslateY += args.Delta.Translation.Y;
        }

        private void BtnAddNodeOnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            _dragItem.Opacity = 0.5;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Position.X - _dragItem.ActualWidth / 2;
            t.TranslateY += args.Position.Y - _dragItem.ActualHeight / 2;
        }

        private async void BtnAddNodeOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        {
            _elementType = sender == btnAddNode ? ElementType.Text : ElementType.Image;

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
        }

        public async Task AddNode(Point pos, Size size, ElementType elementType, object data = null)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;
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
            dict["x"] = p.X;
            dict["y"] = p.Y;
            dict["contentId"] = contentId;
            dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
            dict["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
            dict["metadata"] = metadata;
            dict["autoCreate"] = true;

            var request = new NewElementRequest(dict);
            
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, data == null ? "" : data.ToString(), elementType, dict.ContainsKey("title") ? dict["title"].ToString() : null));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
    
            _lib.UpdateList();
            vm.ClearSelection();
        }
       
        public SessionView SessionView
        {
            get;set;
        }
    }
}