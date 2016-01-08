using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class ImageUtil
    {

        public static  async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {

            var bitmapImage = new BitmapImage();
            using (var stream = new InMemoryRandomAccessStream()) { 
                await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);
                bitmapImage.SetSource(stream);
            }
            return bitmapImage;
        }

        public static async Task<byte[]> RenderTargetBitmapToByteArray(RenderTargetBitmap source)
        {

            var pixels = (await source.GetPixelsAsync());
            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)source.PixelWidth, (uint)source.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
            var ms = new MemoryStream();
            stream.GetInputStreamAt(0).AsStreamForRead().CopyTo(ms);
            return ms.ToArray(); ;
        }
    }
}
