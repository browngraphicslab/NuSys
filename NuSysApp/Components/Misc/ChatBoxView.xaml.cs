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
                if (!text.Equals("") && 
                    SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(SessionController.Instance.LocalUserID))
                {
                    senderBox.Text = "";
                    //SessionController.Instance.NuSysNetworkSession.
                    //send text and user to server
                    var request =
                        new ChatRequest(
                            SessionController.Instance.NuSysNetworkSession.NetworkMembers[
                                SessionController.Instance.LocalUserID], text);
                    SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                }
            }
            //e.Handled = true;
        }

        public void ScrollToEnd()
        {
            scroller.UpdateLayout();
            scroller.ScrollToVerticalOffset(300);
        }

        public double ScrollerScrollableHeight
        {
            get { return scroller.ScrollableHeight; }
        }

        public double ScrollerVerticalOffset
        {
            get { return scroller.VerticalOffset; }
        }

        public string ChatText
        {
            get { return chatDisplayBox.Text; }
        }

        public void AppendText(string s)
        {
            chatDisplayBox.Text += s;
        }

        public Visibility Visibility
        {
            get { return chatCanvas.Visibility; }
            set { chatCanvas.Visibility = value; }
        }
    }
}
