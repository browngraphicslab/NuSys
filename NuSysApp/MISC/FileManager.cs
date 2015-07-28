﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace NuSysApp
{
    class FileManager
    {
        public static async Task<StorageFile> PromptUserForFile(IEnumerable<string> allowedFileTypes = null, PickerViewMode viewMode = PickerViewMode.Thumbnail)
        {
            var fileOpenPicker = new FileOpenPicker {ViewMode = viewMode};
            //fileOpenPicker.FileTypeFilter.Clear();
            if (allowedFileTypes != null)
            {
                foreach (var fileType in allowedFileTypes)
                {
                    fileOpenPicker.FileTypeFilter.Add(fileType);
                }
            }
            try
            {
                var storageFile = await fileOpenPicker.PickSingleFileAsync();
                return storageFile;
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Error Caught");
                return null;
            }
        }
    }
}
