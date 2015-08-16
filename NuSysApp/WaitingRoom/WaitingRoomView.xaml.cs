using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public WorkspaceView _workspaceView;
        public WaitingRoomView()
        {
            this.InitializeComponent();
            Init();
        }

        public async void Init()
        {
            _workspaceView = new WorkspaceView();
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (Globals.Network.isReady())
            {
                this.Frame.Navigate(typeof (WorkspaceView), _workspaceView);
            }
            else
            {
                Debug.WriteLine("Can't join workspace yet, it's not synced with host");
            }
        }

        private void TCP_OnClick(object sender, RoutedEventArgs e)
        {
            Globals.Network.SendMassTCPMessage("TCP Test from "+Globals.Network.LocalIP);
        }
        private void UDP_OnClick(object sender, RoutedEventArgs e)
        {
            Globals.Network.SendMassUDPMessage("UDP Test from " + Globals.Network.LocalIP);
        }
    }
}
