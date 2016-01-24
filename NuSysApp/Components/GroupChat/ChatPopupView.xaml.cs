using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Network.Requests;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ChatPopupView : UserControl
    {
        private ObservableCollection<DialogBlock> _texts = new ObservableCollection<DialogBlock>();
        private bool _touching = false;
        //private Dictionary<DialogBlock,long> _textTimes = new Dictionary<DialogBlock, long>(); 
        public ChatPopupView()
        {
            this.InitializeComponent();
            Texts.ItemsSource = _texts;
            KeyUp += ChatPopupView_KeyUp;
        }

        private async void ChatPopupView_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == VirtualKey.Enter)
            {
                await Send();
            }
        }

        private async void Enter_Click(object sender, RoutedEventArgs e)
        {
            await Send();
        }

        private async Task Send()
        {
            var text = TextBox.Text;
            var request = new ChatDialogRequest(text);
            TextBox.Text = "";
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }

        public void AddText(string text, long time, NetworkUser user)
        {
            var block = new DialogBlock(text,user);
            block.MaxWidth = DialogPanel.ActualWidth * .67;
            if (user.IP == SessionController.Instance.NuSysNetworkSession.LocalIP)
            {
                block.HorizontalAlignment = HorizontalAlignment.Right;
                block.Margin = new Thickness(0, 0, 6, 0);
            }
            else
            {
                block.HorizontalAlignment = HorizontalAlignment.Left;
                block.Margin = new Thickness(6, 0, 0, 0);
            }
            /*
            var index = _texts.Count - 1;
            while (index > -1 && time < _textTimes[_texts[index]])
            {
                index--;
            }
            _texts.Insert(index+1, block);
            _textTimes.Add(block, time);
            */
            _texts.Add(block);
            if(Visibility == Visibility.Collapsed || !_touching)
            {
                Scroller.ScrollToVerticalOffset(Scroller.ScrollableHeight);
            }
        }

        private void UIElement_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _touching = false;
            e.Handled = true;
        }

        private void UIElement_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _touching = true;
            e.Handled = true;
        }
    }
}
