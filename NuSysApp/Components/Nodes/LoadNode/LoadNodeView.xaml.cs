using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class LoadNodeView : AnimatableUserControl///, IThumbnailable
    {
        public LoadNodeView(LoadNodeViewModel vm)
        {
            this.InitializeComponent();
            this.DataContext = vm;

            nodeTpl.Background = new SolidColorBrush(Color.FromArgb(50, 173, 216, 230));
        }

        public Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void StartBar()
        {
            Bar.Start();
        }

        public void EndBar()
        {
            Bar.Finished();
        }
    }
}
