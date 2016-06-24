using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Util
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class TemporaryToolView : AnimatableUserControl, IThumbnailable
    {
        public TemporaryToolView(ElementViewModel vm)
        {
            
            DataContext = vm;
            vm.Controller.SetSize(vm.Width, vm.Height);
            this.InitializeComponent();
            nodeTpl.OnTemplateReady += delegate
            {
                nodeTpl.DuplicateElement.Visibility = nodeTpl.Link.Visibility = nodeTpl.PresentationLink.Visibility = nodeTpl.PresentationMode.Visibility = Visibility.Collapsed;
                nodeTpl.titleContainer.Visibility = Visibility.Collapsed;
            };
        }

        /// <summary>
        /// Creates a thumbnail
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            return new RenderTargetBitmap();
        }

        /// <summary>
        /// Removes the tool view from the atom canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            SessionController.Instance.ActiveFreeFormViewer.AtomViewList.Remove(this);
        }
    }
}
