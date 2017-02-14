using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
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

        /// <summary>
        /// method used to fetch a from the save folder any saved settings object.  
        /// If it finds one, it will override the sessionController's version.
        /// Be careful of race conditions here.
        /// </summary>
        public static SessionSettingsData LoadSavedSettings()
        {
            if (File.Exists(NuSysStorages.SaveFolder.Path + "\\settings.txt"))
            {
                try
                {
                    var text = File.ReadAllText(ApplicationData.Current.LocalFolder.Path + "\\settings.txt");
                    return JsonConvert.DeserializeObject<SessionSettingsData>(text) ?? new SessionSettingsData();
                }
                catch(Exception e)
                {
                    return new SessionSettingsData();
                }
            }
            return new SessionSettingsData();
        }


        /// <summary>
        /// used to save the settings to file for later reading
        /// </summary>
        /// <param name="data"></param>
        public static void SaveSettings(SessionSettingsData data)
        {
            try
            {
                File.WriteAllText(ApplicationData.Current.LocalFolder.Path + "\\settings.txt", JsonConvert.SerializeObject(data));
            }
            catch(Exception e)
            {
                return;
            }
        }

    }
}
