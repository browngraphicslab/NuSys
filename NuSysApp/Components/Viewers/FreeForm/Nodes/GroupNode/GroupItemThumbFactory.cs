using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;


namespace NuSysApp
{
    public class GroupItemThumbFactory : INodeViewFactory
    {
        public async Task<FrameworkElement> CreateFromSendable(ElementController controller, List<FrameworkElement> AtomViewList)
        {
            var rect = new Rectangle();
            if (!(controller.Model is ElementModel)) return null;
            
            var img = new Image();
            if (SessionController.Instance.Thumbnails.ContainsKey(controller.Model.Id))
                img.Source = SessionController.Instance.Thumbnails[controller.Model.Id];
            else
            {
                rect.DataContext = new GroupItemViewModel(controller);
                return rect;
            }
           
            rect.Fill = new ImageBrush
            {
                AlignmentX = AlignmentX.Center,
                AlignmentY = AlignmentY.Center,
                ImageSource = img.Source,
                Stretch = Stretch.Uniform,

            };
            if (img.Source is RenderTargetBitmap)
            {
                rect.Width = (img.Source as RenderTargetBitmap).PixelWidth/
                             DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                rect.Height = (img.Source as RenderTargetBitmap).PixelHeight/
                              DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            }
            else
            {
                rect.Width = (img.Source as BitmapImage).PixelWidth /
                             DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
                rect.Height = (img.Source as BitmapImage).PixelHeight /
                              DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            }

            rect.DataContext = new GroupItemViewModel(controller);

            return rect;
        }
    }
}
