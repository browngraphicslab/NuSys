using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    interface IThumbnailable
    {
        Task<ImageSource> ToThumbnail(int width, int height);

    }
}
