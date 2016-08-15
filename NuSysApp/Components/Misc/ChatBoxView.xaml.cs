using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using WinRTXamlToolkit.Controls.Extensions;

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
        private async void ChatInputBox_KeyDown(object sender, KeyRoutedEventArgs e)
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
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    request.AddSuccesfullChatLocally();
                }
                if (!SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(SessionController.Instance.LocalUserID))
                {
                    throw new Exception("user is not in NetworkMembers");
                }
            }
            //if it gets handled, for whatever reason, you can't type in the chatbox
            //e.Handled = true;
        }

        private ScrollViewer GetScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer)
            {
                return (ScrollViewer)element;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }

        private async Task ScrollToIndex(ListViewBase listViewBase, int index)
        {
            bool isVirtualizing = default(bool);
            double previousHorizontalOffset = default(double), previousVerticalOffset = default(double);

            // get the ScrollViewer withtin the ListView/GridView
            var scrollViewer = listViewBase.GetScrollViewer();
            // get the SelectorItem to scroll to
            var selectorItem = listViewBase.ContainerFromIndex(index) as SelectorItem;

            // when it's null, means virtualization is on and the item hasn't been realized yet
            if (selectorItem == null)
            {
                isVirtualizing = true;

                previousHorizontalOffset = scrollViewer.HorizontalOffset;
                previousVerticalOffset = scrollViewer.VerticalOffset;

                // call task-based ScrollIntoViewAsync to realize the item
                await ScrollIntoViewAsync(listViewBase, listViewBase.Items[index]);
                await Task.Delay(10);
                // this time the item shouldn't be null again
                selectorItem = listViewBase.ContainerFromIndex(index) as SelectorItem;
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            // when virtualized, scroll back to previous position without animation
            if (isVirtualizing)
            {
                await ChangeViewAsync(scrollViewer, previousHorizontalOffset, previousVerticalOffset, true);
            }

            // scroll to desired position with animation!
            scrollViewer.ChangeView(position.X, position.Y, null);
        }

        private async Task ScrollToItem(ListViewBase listViewBase, object item)
        {
            bool isVirtualizing = default(bool);
            double previousHorizontalOffset = default(double), previousVerticalOffset = default(double);

            // get the ScrollViewer withtin the ListView/GridView
            var scrollViewer = listViewBase.GetScrollViewer();
            // get the SelectorItem to scroll to
            var selectorItem = listViewBase.ContainerFromItem(item) as SelectorItem;

            // when it's null, means virtualization is on and the item hasn't been realized yet
            if (selectorItem == null)
            {
                isVirtualizing = true;

                previousHorizontalOffset = scrollViewer.HorizontalOffset;
                previousVerticalOffset = scrollViewer.VerticalOffset;

                // call task-based ScrollIntoViewAsync to realize the item
                listViewBase.ScrollIntoView(item);

                // this time the item shouldn't be null again
                selectorItem = (SelectorItem)listViewBase.ContainerFromItem(item);
            }

            // calculate the position object in order to know how much to scroll to
            var transform = selectorItem.TransformToVisual((UIElement)scrollViewer.Content);
            var position = transform.TransformPoint(new Point(0, 0));

            // when virtualized, scroll back to previous position without animation
            if (isVirtualizing)
            {
                await ChangeViewAsync(scrollViewer,previousHorizontalOffset, previousVerticalOffset, true);
            }

            // scroll to desired position with animation!
            scrollViewer.ChangeView(position.X, position.Y, null);
        }

        private async Task ScrollIntoViewAsync(ListViewBase listViewBase, object item)
        {
            var tcs = new TaskCompletionSource<object>();
            var scrollViewer = listViewBase.GetScrollViewer();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                listViewBase.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
        }

        private async Task ChangeViewAsync(ScrollViewer scrollViewer, double? horizontalOffset, double? verticalOffset, bool disableAnimation)
        {
            var tcs = new TaskCompletionSource<object>();

            EventHandler<ScrollViewerViewChangedEventArgs> viewChanged = (s, e) => tcs.TrySetResult(null);
            try
            {
                scrollViewer.ViewChanged += viewChanged;
                scrollViewer.ChangeView(horizontalOffset, verticalOffset, null, disableAnimation);
                await tcs.Task;
            }
            finally
            {
                scrollViewer.ViewChanged -= viewChanged;
            }
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
            chatDisplayListView.ScrollIntoView(chatDisplayListView.Items[chatDisplayListView.Items.Count-1]);
            //ScrollToIndex(chatDisplayListView, chatDisplayListView.Items.Count-1);
        }



        public Visibility Visibility
        {
            get { return chatCanvas.Visibility; }
            set { chatCanvas.Visibility = value; }
        }
    }
}
