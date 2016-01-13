using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class UserLabel : Button
    {
        private NetworkUser _user;

        public UserLabel(NetworkUser user)
        {
            
            _user = user;
            this.MakeProperties();
           
        }

        private void MakeProperties()
        {
            this.Height = 50;
            this.Width = 150;
            this.HorizontalAlignment = HorizontalAlignment.Right;
            this.Content = _user.Name ?? _user.IP;
            this.Background = new SolidColorBrush(_user.Color);
        }
    }
}
