﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MyToolkit.Messaging;

namespace NuSysApp
{
    public class MediaUtil
    {


        public static  async Task<BitmapImage>  ByteArrayToBitmapImage(byte[] byteArray)
        {
            
            BitmapImage bmi = new BitmapImage();

            InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream();

            await stream.WriteAsync(byteArray.AsBuffer());
            stream.Seek(0);
            bmi.CreateOptions = BitmapCreateOptions.None;

            bmi.SetSource(stream);
            await stream.FlushAsync();
            stream.Dispose();
           
            return bmi;
        }

        private static byte[] ATask(byte[] b)
        {
            return null;
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
        /*
        public static async Task<StorageFile> ConvertByteToAudio(byte[] byteArray)
        {
            var recordStorageFile = await _rootFolder.CreateFileAsync(Id + ".mp3", CreationCollisionOption.GenerateUniqueName);
            await FileIO.WriteBytesAsync(recordStorageFile, byteArray);
            return recordStorageFile;
        }*/
    }
}
