using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ChatBoxView : UserControl
    {
        public ChatBoxView()
        {
            this.InitializeComponent();
            chatInputBox.KeyDown += ChatInputBox_KeyDown;
        }
        /*
        private void BtnGlobalChat_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _isChatVisible = !_isChatVisible;
            if (_isChatVisible)
            {
                chatCanvas.Visibility = Visibility.Visible;
            }
            else
            {
                chatCanvas.Visibility = Visibility.Collapsed;
            }

        }
        */
        private void ChatInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                TextBox senderBox = sender as TextBox;
                Debug.Assert(senderBox != null);
                string text = senderBox.Text;
                senderBox.Text = "";
                //SessionController.Instance.NuSysNetworkSession.
                //send text and user to server
                new ChatRequest(SessionController.Instance.NuSysNetworkSession.NetworkMembers[SessionController.Instance.LocalUserID], text);
            }
            //e.Handled = true;
        }

        public string ChatText
        {
            get { return chatDisplayBox.Text; }
            set { chatDisplayBox.Text = value; }
        }

        public Visibility Visibility
        {
            get { return chatCanvas.Visibility; }
            set { chatCanvas.Visibility = value; }
        }
    }
}
