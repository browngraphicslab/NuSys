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
    public sealed partial class KeyboardKey : UserControl
    {

        public string TextExposedInXaml
        {
            get { return xTextBlock.Text; }

            set { xTextBlock.Text = value; }
        }


        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("TextExposedInXaml", typeof(string), typeof(KeyboardKey), null);

        public KeyboardKey()
        {
            this.InitializeComponent();

        }
    }
}
