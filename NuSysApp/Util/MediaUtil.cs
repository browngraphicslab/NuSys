﻿using System;
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
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

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
        /// Method to call isntead of await CanvasBitmap.LoadAsync.
        /// This will try catch the load and make sure it has a proper url.
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static async Task<CanvasBitmap> LoadCanvasBitmapAsync(ICanvasResourceCreator resourceCreator, Uri uri, float? dpi = 0)
        {
            try
            {
                Debug.WriteLine(uri.AbsoluteUri);
                if (dpi != null)
                {
                    return await CanvasBitmap.LoadAsync(resourceCreator, uri, dpi.Value);
                }
                else
                {
                    return await CanvasBitmap.LoadAsync(resourceCreator, uri);
                }
            }
            catch(Exception e)
            {
                if (dpi != null)
                {
                    return await CanvasBitmap.LoadAsync(resourceCreator, new Uri("ms-appx:///Assets/node icons/icon_play.png"),dpi.Value);
                }
                return await CanvasBitmap.LoadAsync(resourceCreator, new Uri("ms-appx:///Assets/node icons/icon_play.png"));
            }
        }

        /// <summary>
        /// Returns a dictionary mapping thumbnail strings to all thumbnailsize 
        /// enums for the passed in storage file
        /// </summary>
        /// <param name="storageFile"></param>
        /// <returns></returns>
        public static async Task<Dictionary<NusysConstants.ThumbnailSize,string>>  GetThumbnailDictionary(StorageFile storageFile)
        {
            // Create some variables to help create the dictionary
            var thumbnails = new Dictionary<NusysConstants.ThumbnailSize, string>();
            var intSizes = new uint[] { 50, 150, 300 };
            var thumbSizes = new NusysConstants.ThumbnailSize[] { NusysConstants.ThumbnailSize.Small, NusysConstants.ThumbnailSize.Medium, NusysConstants.ThumbnailSize.Large };

            // Fill out the dictionary for every thumbnail size
            for (int i = 0; i < 3; i++)
            {
                var thumbnail = new BitmapImage();
                var source = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem, intSizes[i]);
                var byteArray = await MediaUtil.IRandomAcessStreamToByteArray(source);
                thumbnails[thumbSizes[i]] = Convert.ToBase64String(byteArray);
            }
            if (storageFile.FileType == ".mp4")
            {
                var thumbnail = new BitmapImage();
                var videoprops = await storageFile.Properties.GetVideoPropertiesAsync();
                
                var source = await storageFile.GetThumbnailAsync(ThumbnailMode.SingleItem,videoprops.Width);
                var byteArray = await MediaUtil.IRandomAcessStreamToByteArray(source);
                thumbnails[NusysConstants.ThumbnailSize.Large] = Convert.ToBase64String(byteArray);
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
        /// <summary>
        /// returns a color based on a passed in string.
        /// The same color will result from an identical string each time
        /// </summary>
        /// <returns></returns>
        public static Color GetHashColorFromString(string stringToGetColorFrom)
        {
            Color color = Colors.Black;
            if (stringToGetColorFrom == null)
            {
                return color;
            }
            try
            {
                var idHash = WaitingRoomView.Encrypt(stringToGetColorFrom);
                long number = Math.Abs(BitConverter.ToInt64(idHash, 3));
                long r1 = BitConverter.ToInt64(idHash, 1);
                long r2 = BitConverter.ToInt64(idHash, 2); 

                var mod = 255;

                int r = (int)Math.Abs(((int)number % mod));
                int b = (int)Math.Abs((r1 * number) % mod);
                int g = (int)Math.Abs((r2 * number) % mod);
                long a = ((r + g + b + number)%50) + 175;
                color = Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b);
            }
            catch (Exception e)
            {
                color = Colors.Black;
            }
            return color;
        }
    }
}
