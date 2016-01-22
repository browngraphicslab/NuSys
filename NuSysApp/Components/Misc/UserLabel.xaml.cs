using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Text;
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
    public sealed partial class UserLabel : UserControl
    {
        private NetworkUser _user;

        public UserLabel(NetworkUser user)
        {
            this.InitializeComponent();
            _user = user;
            UserButton.Background = new SolidColorBrush(_user.Color);
            var content = _user.Name ?? _user.IP;
            if (content != "Me")
            {
                UserBubbleText.Text = content.Substring(0, 1).ToUpper();
            }
            else
            {
                UserBubbleText.Text = "Me";
            }
            if (user.IP == SessionController.Instance.NuSysNetworkSession.HostIP)//if the user is host
            {
                var weight = new FontWeight();
                weight.Weight = (ushort) (UserBubbleText.FontWeight.Weight + UserBubbleText.FontWeight.Weight);
                UserBubbleText.FontWeight = weight;
            }
        }
    }
}
