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
        private string _userName;

        public UserLabel(NetworkUser user)
        {
            UITask.Run(delegate
            {
                this.InitializeComponent();
                _user = user;
                UserButton.Background = new SolidColorBrush(_user.Color);
                var content = _user.Name ?? _user.ID;
                if (content != "Me")
                {
                    if (content.Length == 0)
                    {
                        _userName = "_";
                    }
                    else
                    {
                        _userName = content.Substring(0, 1).ToUpper();
                    }
                }
                else
                {
                    _userName = "Me";
                }
                UserBubbleText.Text = _userName;
                UserButton.Foreground = new SolidColorBrush(Constants.foreground6);

                UserBubbleText.Inlines.Clear();
                UserBubbleText.Text = _userName;

                UserButton.Click += UserButton_Click;
            });
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
                //UserIP.Text = _user.ID;
            }
        }

        private void UserButton_OnLostFocus(object sender, RoutedEventArgs e)
        {
            UserInfoBox.Opacity = 0;
        }
    }
}
