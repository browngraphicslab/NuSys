using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NusysIntermediate;
using NuSysApp.Network.Requests;

namespace NuSysApp
{
    public sealed partial class SessionView : Page
    {
        private bool _isInitialized;


        /// <summary>
        /// Gets the instance of the FileAddedAclsPopup from the main canvas
        /// </summary>
        public FileAddedAclsPopup FileAddedAclsPopup => xFileAddedAclesPopup;

        private string _prevCollectionLibraryId;
        private FreeFormViewer _activeFreeFormViewer;

        public SessionView()
        {
            this.InitializeComponent();

            SessionController.Instance.SessionView = this;

            _activeFreeFormViewer = new FreeFormViewer();
            _activeFreeFormViewer.Width = ActualWidth;
            _activeFreeFormViewer.Height = ActualHeight;
            mainCanvas.Children.Insert(0, _activeFreeFormViewer);
            
            xLoadingGrid.PointerPressed += XLoadingGridOnPointerPressed;
        }

        private void XLoadingGridOnPointerPressed(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            SessionController.Instance.LoadCapturedState();
        }

        public async Task Init()
        {
            if (!_isInitialized)
            {

                SizeChanged += delegate (object sender, SizeChangedEventArgs args)
                {
                    Clip = new RectangleGeometry { Rect = new Rect(0, 0, args.NewSize.Width, args.NewSize.Height) };
                    if (_activeFreeFormViewer != null)
                    {
                        _activeFreeFormViewer.Width = args.NewSize.Width;
                        _activeFreeFormViewer.Height = args.NewSize.Height;
                    }
                };

                xFileAddedAclesPopup.DataContext = new FileAddedAclsPopupViewModel();

                await SessionController.Instance.InitializeRecog();
            }
            _isInitialized = true;
            var collectionId = WaitingRoomView.InitialWorkspaceId;
            await SessionController.Instance.EnterCollection(collectionId);


        }

        public void ShowBlockingScreen(bool visible)
        {
            xLoadingGrid.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public Canvas MainCanvas
        {
            get { return mainCanvas; }
        }

        public FreeFormViewer FreeFormViewer { get { return _activeFreeFormViewer; } }

    }
}