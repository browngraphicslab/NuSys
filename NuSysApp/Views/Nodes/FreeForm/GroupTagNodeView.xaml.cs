using Windows.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupTagNodeView : UserControl
    {
    
        public GroupTagNodeView(GroupTagNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
        
    }
}