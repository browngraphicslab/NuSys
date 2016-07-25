using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public class MediaUtil
    {

        public static  async Task<BitmapImage> ByteArrayToBitmapImage(byte[] byteArray)
        {

            var bitmapImage = new BitmapImage();
            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();

            await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);
            bitmapImage.CreateOptions = BitmapCreateOptions.None;

            bitmapImage.SetSource(stream);
            await stream.FlushAsync();
            stream.Dispose();

            return bitmapImage;
        }

        /// <summary>
        /// Converts the passed in source into a byte array
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static async Task<byte[]> IRandomAcessStreamToByteArray(IRandomAccessStream s)
        {
                var dr = new DataReader(s.GetInputStreamAt(0));
                var bytes = new byte[s.Size];
                await dr.LoadAsync((uint)s.Size);
                dr.ReadBytes(bytes);
                return bytes;
            
        }

        public static async Task<byte[]> RenderTargetBitmapToByteArray(RenderTargetBitmap source)
        {
            var pixels = (await source.GetPixelsAsync());
            var stream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)source.PixelWidth, (uint)source.PixelHeight, 96, 96, pixels.ToArray());
            await encoder.FlushAsync();
            var ms = new MemoryStream();
            stream.GetInputStreamAt(0).AsStreamForRead().CopyTo(ms);
            return ms.ToArray(); ;
        }
        
        public static async Task<byte[]> StorageFileToByteArray(StorageFile file)
        {
            byte[] fileBytes = null;
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (DataReader reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }

        public static async Task<MuPDFWinRT.Document> DataToPDF(string base64StringData)
        {
            var dataBytes = Convert.FromBase64String(base64StringData ?? "");
            var c = dataBytes.Length;
            var ms = new MemoryStream(dataBytes);
            MuPDFWinRT.Document document;
            using (IInputStream inputStreamAt = ms.AsInputStream())
            {
                using (var dataReader = new DataReader(inputStreamAt))
                {
                    uint u = await dataReader.LoadAsync((uint)dataBytes.Length);
                    IBuffer readBuffer = dataReader.ReadBuffer(u);
                    document = MuPDFWinRT.Document.Create(readBuffer, MuPDFWinRT.DocumentType.PDF, 120);
                }
            }
            return document;
        }


        /// <summary>
        /// Returns a dictionary mapping thumbnail strings to all thumbnailsize 
        /// enums for the passed in storage file
        /// </summary>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        public static async Task<Dictionary<ThumbnailSize,string>>  GetThumbnailDictionary(StorageFile storageFile)
        {
            // Create some variables to help create the dictionary
            var thumbnails = new Dictionary<ThumbnailSize, string>();
            var intSizes = new uint[] { 50, 150, 300 };
            var thumbSizes = new ThumbnailSize[] { ThumbnailSize.SMALL, ThumbnailSize.MEDIUM, ThumbnailSize.LARGE };

            // Fill out the dictionary for every thumbnail size
            for (int i = 0; i < 3; i++)
            {
                var thumbnail = new BitmapImage();
                var source = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, intSizes[i]);
                var byteArray = await MediaUtil.IRandomAcessStreamToByteArray(source);
                thumbnails[thumbSizes[i]] = Convert.ToBase64String(byteArray);
            }

            // Return the completed dictionary
            return thumbnails;
        }
        /*
public static async Task<StorageFile> ConvertByteToAudio(byte[] byteArray)
{
   var recordStorageFile = await _rootFolder.CreateFileAsync(ContentId + ".mp3", CreationCollisionOption.GenerateUniqueName);
   await FileIO.WriteBytesAsync(recordStorageFile, byteArray);
   return recordStorageFile;
}*/
    }
}
