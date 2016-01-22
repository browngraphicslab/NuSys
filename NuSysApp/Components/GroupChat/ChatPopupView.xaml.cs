using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private Dictionary<DialogBlock,long> _textTimes = new Dictionary<DialogBlock, long>(); 
        public ChatPopupView()
        {
            this.InitializeComponent();
            Texts.ItemsSource = _texts;
        }

        private async void Enter_Click(object sender, RoutedEventArgs e)
        {
            var text = TextBox.Text;
            var request = new ChatDialogRequest(text);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(request);
        }

        public void AddText(string text, long time, NetworkUser user)
        {
            var block = new DialogBlock(text,user);
            block.MaxWidth = DialogPanel.ActualWidth * .67;
            if (user.IP == SessionController.Instance.NuSysNetworkSession.LocalIP)
            {
                block.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                block.HorizontalAlignment = HorizontalAlignment.Left;
            }
            var index = _texts.Count - 1;
            while (index > -1 && time < _textTimes[_texts[index]])
            {
                index--;
            }
            _texts.Insert(index+1, block);
            _textTimes.Add(block, time);
        }
    }
}
