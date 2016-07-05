using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
{
    public class CachePDF
    {
        public static async void createWriteFile(string fileName, string content)
        {
            if (content == null)
            {
                Debug.WriteLine("Content shouldn't be null, caching pdfs");
                return;
            }

            string file = fileName + ".txt";
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;

            Debug.WriteLine(storageFolder.Path);

            Windows.Storage.StorageFile sampleFile =
                await storageFolder.CreateFileAsync(file,
                    Windows.Storage.CreationCollisionOption.ReplaceExisting);
                
            // TODO : Exception Handling
            await Windows.Storage.FileIO.WriteTextAsync(sampleFile, content);
        }

        public static async Task<string> readFile(string fileName)
        {
            string file = fileName + ".txt";
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile sampleFile =
                await storageFolder.GetFileAsync(file);
            
            // TODO : Exception Handling

            string text = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);

            return text;
        }

        public static async Task<bool> isFilePresent(string fileName)
        {
            string file = fileName + ".txt";
            var item = await ApplicationData.Current.LocalFolder.TryGetItemAsync(file);
            return item != null;
        }
    }
}