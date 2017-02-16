using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace NuSysApp
{
    class FileManager
    {
        public static async Task<IReadOnlyList<StorageFile>> PromptUserForFiles(IEnumerable<string> allowedFileTypes = null, PickerViewMode viewMode = PickerViewMode.Thumbnail, bool singleFileOnly = false)
        {
            var fileOpenPicker = new FileOpenPicker { ViewMode = viewMode };
            if (allowedFileTypes != null)
            {
                foreach (var fileType in allowedFileTypes)
                {
                    fileOpenPicker.FileTypeFilter.Add(fileType);
                    //fileOpenPicker.FileTypeFilter.Add(fileType.ToUpper());
                }
            }
            try
            {
                IReadOnlyList<StorageFile> storageFiles = null;
                if (singleFileOnly)
                {
                    StorageFile storageFile = null;
                    await UITask.Run(async () =>
                    {
                        storageFile = await fileOpenPicker.PickSingleFileAsync();
                    });
                    if (storageFile != null)
                    {
                        storageFiles = new List<StorageFile> {storageFile};
                    }
                }
                else
                {
                    storageFiles = await fileOpenPicker.PickMultipleFilesAsync();
                }
                return storageFiles;
            }
            catch
            {
                Debug.WriteLine("Error Caught");
                return null;
            }
        }
    }
}
