using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class UserWindowView : UserControl
    {
        private FloatingMenuView _floatingMenu;

        public UserWindowView()
        {
            this.InitializeComponent();
        }

        public void setFloatingMenu(FloatingMenuView floatingMenu)
        {
            _floatingMenu = floatingMenu;
        }

        private void Exit_OnClick(object sender, RoutedEventArgs e)
        {
            _floatingMenu.CloseAllSubMenus();
        }
    }

    
}
