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
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp2
{
    public sealed partial class DialogBlock : UserControl
    {
        public DialogBlock(string text, NetworkUser user)
        {
            this.InitializeComponent();
            var brush = new SolidColorBrush(user.Color);
            TheGrid.BorderBrush = brush;
            Box.Foreground = brush;
            Box.Text = text;
            MinHeight = 25;
            MinWidth = 25;
        }
    }
}
