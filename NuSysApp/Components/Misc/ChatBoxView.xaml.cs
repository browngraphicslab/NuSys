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




            DataContextChanged += delegate (FrameworkElement sender, DataContextChangedEventArgs args)
            {
                if (!(DataContext is ChatBoxViewModel))
                    return;
                var vm = (ChatBoxViewModel) DataContext;
                vm.MakeMessageList();
            };
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
                if (!SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(SessionController.Instance.LocalUserID))
                {
                    throw new Exception("user is not in NetworkMembers");
                }
            }
            //if it gets handled, for whatever reason, you can't type in the chatbox
            //e.Handled = true;
        }

        /// <summary>
        /// Adds a new message to the bottom of the chatbox
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        public void AppendText(NetworkUser user, string message)
        {
            var vm = DataContext as ChatBoxViewModel;
            if (vm == null)
            {
                return;
            }
            vm.AddMessage(user, message);
            ScrollToEnd();
        }

        /// <summary>
        /// scrolls to the bottom of the ListView
        /// </summary>
        public void ScrollToEnd()
        {
            //scroll to the most recently added item in the list
            chatDisplayListView.ScrollIntoView(chatDisplayListView.Items[chatDisplayListView.Items.Count-1], ScrollIntoViewAlignment.Leading);
        }



        public Visibility Visibility
        {
            get { return chatCanvas.Visibility; }
            set { chatCanvas.Visibility = value; }
        }
    }
}
