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

namespace NuSysApp.Components.GroupChat
{
    public sealed partial class ChatPopupView : UserControl
    {
        private SortedList<int, Button> _texts = new SortedList<int, Button>();
        public ChatPopupView()
        {
            this.InitializeComponent();
            Texts.ItemsSource = _texts;
        }

        private void Enter_Click(object sender, RoutedEventArgs e)
        {
            var text = TextBox.Text;

        }
    }
}
