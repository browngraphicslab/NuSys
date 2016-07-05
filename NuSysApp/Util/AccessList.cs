using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace NuSysApp
{

    public class AccessList
    {
        public static Dictionary<string, string> FileTokenDict = new Dictionary<string, string>();

        public static async Task<StorageFolder> GetWorkspaceFolder()
        {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var workspaceToken = localSettings.Values[Constants.NuSysWorkspaceToken];

            StorageFolder workspaceFolder = null;

            if (workspaceToken == null)
            {
                var FolderPicker = new Windows.Storage.Pickers.FolderPicker();
                FolderPicker.ViewMode = PickerViewMode.Thumbnail;
                FolderPicker.SuggestedStartLocation = PickerLocationId.Desktop;
                FolderPicker.FileTypeFilter.Add("*");

                workspaceFolder = await FolderPicker.PickSingleFolderAsync();
                string folderToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(workspaceFolder);
                localSettings.Values[Constants.NuSysWorkspaceToken] = folderToken;
            }else
            {
                workspaceFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFolderAsync(workspaceToken.ToString());
            }       

            return workspaceFolder;
        }

        public static async Task OpenFile(string token)
        {
            if (String.IsNullOrEmpty(token))
            {
                return;
            }

            if (!String.IsNullOrEmpty(token) && !Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token))
            {
                return;
            }

            StorageFile file = await Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.GetFileAsync(token);
            bool success = await Windows.System.Launcher.LaunchFileAsync(file);
        }

        public static async Task ReadFileTokens()
        {
            StorageFile NuSysFALFiles = await GetNuSysFAL(Constants.NuSysFALFiles);

            if (NuSysFALFiles != null)
            {
                using (StreamReader fileReader = new StreamReader(await NuSysFALFiles.OpenStreamForReadAsync()))
                {
                    String path = String.Empty;
                    String token = String.Empty;
                    while (!fileReader.EndOfStream)
                    {
                        path = fileReader.ReadLine();
                        token = fileReader.ReadLine();
                        FileTokenDict.Add(path, token);
                    }
                }
            }
        }

        public static async Task SaveFileTokens()
        {
            StorageFile NuSysFALFiles = await GetNuSysFAL(Constants.NuSysFALFiles);

            if (NuSysFALFiles != null)
            {
                using (StreamWriter fileWriter = new StreamWriter(await NuSysFALFiles.OpenStreamForWriteAsync()))
                {
                    foreach (KeyValuePair<string, string> tokenPair in FileTokenDict)
                    {
                        fileWriter.WriteLine(tokenPair.Key);
                        fileWriter.WriteLine(tokenPair.Value);
                    }
                }
            }
        }

        private static async Task<StorageFile> GetNuSysFAL(String FALFileName)
        {
            StorageFile NuSysFAL = null;
            StorageFolder appDataFolder = Windows.Storage.ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile NuSysFALFiles = await appDataFolder.GetFileAsync(FALFileName);
            }
            catch (System.IO.FileNotFoundException)
            {
                StorageFile NuSysFALFiles = await appDataFolder.CreateFileAsync(FALFileName);
            }

            return NuSysFAL;
        }

    }
}
