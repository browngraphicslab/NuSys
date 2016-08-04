using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
namespace NuSysApp
{
    public class ChatBoxViewModel : BaseINPC
    {
        public ObservableCollection<ChatMessageTemplate> Messages { get; set; }
        public ChatBoxViewModel()
        {
            Messages = new ObservableCollection<ChatMessageTemplate>();
        }

        public void AddMessage(NetworkUser user, string message)
        {
            ChatMessageTemplate messageTemplate = new ChatMessageTemplate(user, message);
            Messages.Add(messageTemplate);

        }

        public void MakeMessageList()
        {
            //Currently useless method.
            Messages = new ObservableCollection<ChatMessageTemplate>();
            RaisePropertyChanged("Messages");
        }
    }
}
