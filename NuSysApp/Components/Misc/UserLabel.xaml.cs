using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class UserLabel : UserControl
    {
        private NetworkUser _user;
        private ushort _startingFontWeight;
        private string _userName;

        public UserLabel(NetworkUser user)
        {
            this.InitializeComponent();
            _user = user;
            UserButton.Background = new SolidColorBrush(_user.Color);
            var content = _user.Name ?? _user.IP;
            if (content != "Me")
            {
                _userName = content.Substring(0, 1).ToUpper();
            }
            else
            {
                _userName = "Me";
            }
            UserBubbleText.Text = _userName;
            if (user.IP == SessionController.Instance.NuSysNetworkSession.HostIP) //if the user is host
            {
                MakeHost();
            }
            else
            {
                MakeNotHost();
            }
            _startingFontWeight = UserBubbleText.FontWeight.Weight;
            user.OnHostStatusChange += delegate (bool isHost)
            {
                if (isHost)
                {
                    MakeHost();
                }
                else
                {
                    MakeNotHost();
                }
            };

            UserButton.Click += UserButton_Click;
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserInfoBox.Opacity == 1)
            {
                UserInfoBox.Opacity = 0;
            }
            else
            {
                UserInfoBox.Opacity = 1;
                UserInfoBox.Foreground = new SolidColorBrush(_user.Color);
                UserName.Text = _user.Name;
                UserIP.Text = _user.IP;
            }
        }

        private void UserButton_OnLostFocus(object sender, RoutedEventArgs e)
        {
            UserInfoBox.Opacity = 0;
        }

        private async Task MakeHost()
        {
            await UITask.Run(async delegate
            {
                var weight = UserBubbleText.FontWeight;
                weight.Weight = (ushort)(_startingFontWeight*(ushort)2.5);
                UserBubbleText.FontWeight = weight;
                //UserButton.Foreground = new SolidColorBrush(Colors.Gold);

                UserBubbleText.Inlines.Clear();

                Underline HostUnderline = new Underline();
                HostUnderline.Foreground = new SolidColorBrush(Colors.White);
                Run r = new Run();
                r.Text = _userName;
                HostUnderline.Inlines.Add(r);
                UserBubbleText.Inlines.Add(HostUnderline);
            });
        }

        private async Task MakeNotHost()
        {
            await UITask.Run(async delegate
            {
                var weight = UserBubbleText.FontWeight;
                weight.Weight = _startingFontWeight;
                UserBubbleText.FontWeight = weight;
                UserButton.Foreground = new SolidColorBrush(Colors.White);

                UserBubbleText.Inlines.Clear();
                UserBubbleText.Text = _userName;
            });
        }
    }
}
