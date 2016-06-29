using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public class StorageUtil
    {
        
        public static Task<StorageFolder> CreateFolderIfNotExists(StorageFolder parent, string folderName)
        {
            return parent.CreateFolderAsync(folderName, CreationCollisionOption.OpenIfExists).AsTask();     
        }

        public static Task<StorageFile> CreateFileIfNotExists(StorageFolder parent, string fileName)
        {
            return parent.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).AsTask();
        }

        /// <summary>
        /// Take the passed-in base 64 string data and converts it into a picture storage file
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task SaveAsStorageFile(string data, string filepath)
        {
            var bytes = Convert.FromBase64String(data);
            Windows.Storage.StorageFolder storageFolder =
    Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile file = await StorageFile.GetFileFromPathAsync(filepath);
            await FileIO.WriteBytesAsync(file, bytes);
        }

    }
}
