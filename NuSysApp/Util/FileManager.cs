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
        public static async Task<IReadOnlyList<StorageFile>> PromptUserForFiles(IEnumerable<string> allowedFileTypes = null, PickerViewMode viewMode = PickerViewMode.Thumbnail, bool singleFileOnly = false, bool allowAllFileTypes = false)
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
            if (allowAllFileTypes)
            {
                var l = new List<string>()
                {
                    ".exe",
                    ".sln",
                    ".vim",
                    ".md",
                    ".git",
                    ".db",
                    ".dll",
                    ".cs",
                    ".xaml",
                    ".app",
                    ".appx",
                    ".user",
                    ".data",
                    ".nusys",
                    ".manifset",
                    ".config",
                    ".pfx",
                    ".py",
                    ".js",
                    ".java",
                    ".xls",
                    ".xlsx",
                    ".cer",
                    ".csproj",
                    ".lock",
                    ".pdb",
                    ".pri",
                    ".xsl",
                    ".save",
                    ".log",
                    ".aac",
                    ".abw",
                    ".arc",
                    ".avi",
                    ".azw",
                    ".bin",
                    ".bz",
                    ".bz2",
                    ".csh",
                    ".css",
                    ".csv",
                    ".doc",
                    ".epub",
                    ".gif",
                    ".htm",
                    ".html",
                    ".ico",
                    ".ics",
                    ".jar",
                    ".jpeg",
                    ".jpg",
                    ".js",
                    ".json",
                    ".mid",
                    ".midi",
                    ".mpeg",
                    ".mpkg",
                    ".odp",
                    ".ods",
                    ".odt",
                    ".oga",
                    ".ogv",
                    ".ogx",
                    ".pdf",
                    ".ppt",
                    ".rar",
                    ".rtf",
                    ".sh",
                    ".svg",
                    ".swf",
                    ".tar",
                    ".tif",
                    ".tiff",
                    ".ttf",
                    ".vsd",
                    ".wav",
                    ".weba",
                    ".webm",
                    ".webp",
                    ".woff",
                    ".woff2",
                    ".xhtml",
                    ".xls",
                    ".xml",
                    ".xul",
                    ".zip",
                    ".3gp",
                    ".3g2",
                    ".7z"
                };
                foreach (var s in l)
                {
                    fileOpenPicker.FileTypeFilter.Add(s);
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
                        storageFiles = new List<StorageFile>() {storageFile};
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
