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
            StorageFolder storageFolder =
                ApplicationData.Current.LocalFolder;

            Debug.WriteLine(storageFolder.Path);

            StorageFile sampleFile =
                await storageFolder.CreateFileAsync(file,
                    CreationCollisionOption.ReplaceExisting);
                
            // TODO : Exception Handling
            await FileIO.WriteTextAsync(sampleFile, content);
        }

        public static async Task<string> readFile(string fileName)
        {
            string file = fileName + ".txt";
            StorageFolder storageFolder =
                ApplicationData.Current.LocalFolder;
            StorageFile sampleFile =
                await storageFolder.GetFileAsync(file);
            
            // TODO : Exception Handling

            string text = await FileIO.ReadTextAsync(sampleFile);

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