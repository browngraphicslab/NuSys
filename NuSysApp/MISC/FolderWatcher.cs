using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Search;

namespace NuSysApp.MISC
{
    public class FolderWatcher
    {
        public event FilesChangedHandler FilesChanged;
        public delegate void FilesChangedHandler();

        private StorageFileQueryResult _query;

        public FolderWatcher(StorageFolder path)
        {
            Init(path);
        }

        private void Init(StorageFolder path)
        {
            _query = path.CreateFileQuery();
            _query.ContentsChanged += OnTransferFolderChange;
            var files = _query.GetFilesAsync();
        }

        private void OnTransferFolderChange(IStorageQueryResultBase sender, object args)
        {
            Debug.WriteLine("CONTENTS CHANGED! " + args);
            //if (FilesChanged != null)
            //    FilesChanged();
            FilesChanged?.Invoke();
        }

    }
}
