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

        private void ChatInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                TextBox senderBox = sender as TextBox;
                Debug.Assert(senderBox != null);
                string text = senderBox.Text;
                //there have been a few times where NetworkMembers did not contain LocalUserID
                //something to look out for
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
            //if it gets handled, for whatever reason, you can't type in the chatbox
            //e.Handled = true;
        }

        /// <summary>
        /// scrolls to the bottom of the ListView
        /// </summary>
        public void ScrollToEnd()
        {
            //scroll to the most recently added item in the list
            chatDisplayListView.ScrollIntoView(chatDisplayListView.Items[chatDisplayListView.Items.Count-1], ScrollIntoViewAlignment.Leading);
        }

        /// <summary>
        /// adds string s to the bottom of the chat box by creating a new textblock from string s
        /// and adding that to the bottom of the ListView
        /// </summary>
        /// <param name="s"></param>
        public void AppendText(string s)
        {
            TextBlock block = new TextBlock();
            block.TextWrapping = TextWrapping.Wrap;
            block.Text = s;
            block.Width = 280;
            block.Margin = new Thickness(0,2,0,0);
            chatDisplayListView.Items.Add(block);
            ScrollToEnd();
        }

        public Visibility Visibility
        {
            get { return chatCanvas.Visibility; }
            set { chatCanvas.Visibility = value; }
        }
    }
}
