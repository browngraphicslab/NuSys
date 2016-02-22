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
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Messaging;
using MyToolkit.UI;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class GroupNodeView : AnimatableUserControl, IThumbnailable
    {
        private bool _isExpanded;
        private Storyboard _circleAnim;
        private Storyboard _expandedAnim;
        private Storyboard _expandedListAnim; // for data grid view
        private Storyboard _timelineAnim;
        private GroupNodeExpandedDefaultView xExpandedDefaultView;

        public GroupNodeView( GroupNodeViewModel vm)
        {
            RenderTransform = new CompositeTransform();
            InitializeComponent();
            DataContext = vm;
            Resizer.ManipulationDelta += ResizerOnManipulationDelta;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                PositionResizer();
            };

            // create expanded view
            xExpandedDefaultView = new GroupNodeExpandedDefaultView((NodeContainerModel)vm.Model); // create default view
            xExpandedDefaultView.Opacity = 0;
            GroupNodeCanvas.Children.Add(xExpandedDefaultView);
        }

        private void ResizerOnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = (GroupNodeViewModel) DataContext;
            vm.Controller.Resize(e.Delta.Translation.X, e.Delta.Translation.Y);
            
            if (vm.Height > 400 && !_isExpanded)
            {
                _expandedAnim?.Stop();
                _circleAnim?.Stop();

                _expandedAnim = Anim.To(xExpandedDefaultView, "Alpha", 1, 450);
                _circleAnim = Anim.To(xCircleView, "Alpha", 0, 450);
                _isExpanded = true;
            }
            else if (vm.Height < 400 && _isExpanded)
            {
                _expandedAnim?.Stop();
                _circleAnim?.Stop();

                _expandedAnim = Anim.To(xExpandedDefaultView, "Alpha", 0, 450);
                _circleAnim = Anim.To(xCircleView, "Alpha", 1, 450);
                _isExpanded = false;         
            }
            PositionResizer();
            e.Handled = true;
        }

        private void PositionResizer()
        {
            var vm = (GroupNodeViewModel)DataContext;
            Canvas.SetLeft(Resizer, vm.Width - 50);
            Canvas.SetTop(Resizer, vm.Height - 50);
        }
        
        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(this, width, height);
            return r;
        }
    }
}
