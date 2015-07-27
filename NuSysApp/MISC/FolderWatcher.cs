using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;

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
            if (FilesChanged != null)
                FilesChanged();
        }

    }
}
