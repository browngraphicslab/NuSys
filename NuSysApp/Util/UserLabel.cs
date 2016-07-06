using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

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
            this.Width = 50;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Bottom;
            //this.Margin = new Thickness(10);

            var con = _user.Name ?? _user.IP;
            if (con != "Me")
            {
                this.Content = con.Substring(0, 1).ToUpper();
            }
            else
            {
                this.Content = con;
            }
            this.Background = new SolidColorBrush(_user.Color);

            /*
            Border myBorder = new Border();
            myBorder.CornerRadius = new CornerRadius(50);
            myBorder.Background = new SolidColorBrush(_user.Color);
            

            this.Content = myBorder;
            
            TextBlock myNames = new TextBlock();
            myNames.Text = con;
            myNames.VerticalAlignment = VerticalAlignment.Center;
            myNames.HorizontalAlignment = HorizontalAlignment.Center;*/
        }
    }
}
