using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _isExpanded;

        public GroupNodeView( GroupNodeViewModel vm)
        {
            RenderTransform = new CompositeTransform();
            InitializeComponent();
            DataContext = vm;
            xCircleView.RenderTransform = new CompositeTransform();

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {
                //IC.Clip = new RectangleGeometry {Rect = new Rect(0, 0, 1000, vm.Height-40)};
            };

            Resizer.ManipulationDelta += ResizerOnManipulationDelta;

            Tapped += OnTapped;
        }

        private void ResizerOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel) DataContext;
            vm.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            if (vm.Height > 400 && !_isExpanded)
            {
                Anim.To(xExpandedView, "Alpha", 1, 600);
                Anim.To(xCircleView, "Alpha", 0, 600);
                _isExpanded = true;

                Debug.WriteLine("opening");
            }
            else if (vm.Height < 400 && _isExpanded)
            {
                Anim.To(xExpandedView, "Alpha", 0, 600);
                Anim.To(xCircleView, "Alpha", 1, 600);
                _isExpanded = false;

                Debug.WriteLine("closing");
            
            }
            e.Handled = true;
        }

        public void Collapse()
        {
            xExpandedView.Visibility = Visibility.Collapsed;

        }

        public void Expand()
        {
            xExpandedView.Visibility = Visibility.Visible;
        }

        private void OnTapped(object sender, TappedRoutedEventArgs tappedRoutedEventArgs)
        {
            xExpandedView.Visibility = Visibility.Visible;
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(this, width, height);
            return r;
        }
    }
}
