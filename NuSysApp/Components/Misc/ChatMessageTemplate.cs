using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class ChatMessageTemplate
    {
        public string UserName { set; get; }
        public SolidColorBrush UserColor { set; get; }
        public string MessageText { set; get; }
        public string TimeStamp { set; get; }

        public ChatMessageTemplate(NetworkUser user, string message)
        {
            UserName = user.Name;
            UserColor = new SolidColorBrush(user.Color);
            MessageText = message;
            TimeStamp = DateTime.Now.ToString("h:mm tt");
        }
    }
}
