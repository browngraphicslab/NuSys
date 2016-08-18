using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using NusysIntermediate;

namespace NuSysApp
{
    class FileAddedAclsPopupViewModel : BaseINPC
    {

        private double _height;
        private double _width;
        private bool _isEnabled;

        /// <summary>
        /// The height of the FileAddedAclsPopup we bind to this
        /// </summary>
        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        /// <summary>
        /// The width of the FileAddedAclsPopup we bind to this
        /// </summary>
        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        /// <summary>
        /// Simple boolean that controls enabling and disabling behavior of the pop up
        /// including things such as visibility
        /// </summary>
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set
            {
                _isEnabled = value;
                RaisePropertyChanged("IsEnabled");
            }
        }

        /// <summary>
        /// A list of storage files, we bind the list view to this
        /// </summary>
        public ObservableCollection<StorageFile> Files { get; set; }

        /// <summary>
        /// A dictionary of storage file ids to access types, used to determine when all files have been set
        /// </summary>
        public Dictionary<string, NusysConstants.AccessType> AccessDictionary { get; }

        public FileAddedAclsPopupViewModel()
        {
            Files = new ObservableCollection<StorageFile>();
            AccessDictionary = new Dictionary<string, NusysConstants.AccessType>();
        }

    }
}
