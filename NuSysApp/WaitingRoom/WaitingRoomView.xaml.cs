using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace NuSysApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WaitingRoomView : Page
    {
        private NetworkConnector _networkConnector;
        public WaitingRoomView()
        {
            this.InitializeComponent();
            Init();
        }

        public async void Init()
        {
            _networkConnector = new NetworkConnector();   
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WorkspaceView));
            _networkConnector.WorkspaceViewModel = (WorkspaceViewModel) ((WorkspaceView)this.Frame.Content).DataContext;
            //_networkConnector.sendMassUDPMessage("test from button");
            // _networkConnector.SendTCPMessage("tcp message!", "10.38.22.71","302");
        }

        private void TCP_OnClick(object sender, RoutedEventArgs e)
        {
            _networkConnector.SendMassTCPMessage("TCP Test!");
        }
        private void UDP_OnClick(object sender, RoutedEventArgs e)
        {
            _networkConnector.SendMassUDPMessage("TCP Test!");
        }
    }
}
