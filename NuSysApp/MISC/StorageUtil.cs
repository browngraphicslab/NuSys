using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp.MISC
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

    }
}
