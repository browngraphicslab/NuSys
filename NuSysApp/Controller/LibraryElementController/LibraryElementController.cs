using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Takes care of all the modifying and events invoking for the library element model
    /// Should manage keeping the library element model up to date as well as updating the server
    /// </summary>
    public class LibraryElementController
    {
        protected DebouncingDictionary _debouncingDictionary;
        private LibraryElementModel _libraryElementModel;
        private bool _loading = false;
        #region Events
        public delegate void ContentChangedEventHandler(object source, string contentData);
        public delegate void RegionAddedEventHandler(object source, string regionString);
        public delegate void RegionRemovedEventHandler(object source, string regionString);
        public delegate void MetadataChangedEvenetHandler(object source);
        public delegate void DisposeEventHandler(object source);
        public delegate void TitleChangedEventHandler(object sender, string title);
        public delegate void FavoritedEventHandler(object sender, bool favorited);
        public delegate void LoadedEventHandler(object sender);
        public delegate void DeletedEventHandler(object sender);
        public event ContentChangedEventHandler ContentChanged;
        public event RegionAddedEventHandler RegionAdded;
        public event RegionRemovedEventHandler RegionRemoved;
        public event MetadataChangedEvenetHandler MetadataChanged;
        public event DisposeEventHandler Disposed;
        public event TitleChangedEventHandler TitleChanged;
        public event FavoritedEventHandler Favorited;
        public event DeletedEventHandler Deleted;
        public event LoadedEventHandler Loaded
        {
            add
            {
                _onLoaded += value;
                if (!IsLoaded && !_loading)
                {
                    _loading = true;
                    Task.Run(async delegate{ SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(_libraryElementModel.LibraryElementId);});
                }
            }
            remove { _onLoaded -= value; }
        }
        private event LoadedEventHandler _onLoaded;
        #endregion Events

        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Constuctor just takes in the library element model it will be controller
        /// </summary>
        /// 
        public LibraryElementController(LibraryElementModel libraryElementModel)
        {
            Debug.Assert(libraryElementModel != null);
            _libraryElementModel = libraryElementModel;
            _debouncingDictionary = new DebouncingDictionary(libraryElementModel.LibraryElementId, true);
        }

        /// <summary>
        /// This will change the library element model's data and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetContentData (string contentData)
        {
            _libraryElementModel.Data = contentData;
            ContentChanged?.Invoke(this, contentData);
            _debouncingDictionary.Add("data", contentData);
        }

        /// <summary>
        /// This will ADD a region to the library element model and will update the server accordingly
        /// It will then fire an even notfying all listeners of the new region added
        /// </summary>
        public void AddRegion(string regionString)
        {
            _libraryElementModel.Regions.Add(regionString);
            RegionAdded?.Invoke(this, regionString);
            SessionController.Instance.NuSysNetworkSession.AddRegionToContent(LibraryElementModel.LibraryElementId, regionString);
        }

        /// <summary>
        /// This will REMOVE a region to the library element model and will update the server accordingly
        /// It will then fire an even notfying all listeners of the old region that was deleted
        /// The entire list of regions can be refetched from the library element model directly if needed
        /// </summary>
        public void RemoveRegion(string regionString)
        {
            _libraryElementModel.Regions.Remove(regionString);
            RegionRemoved?.Invoke(this, regionString);
            SessionController.Instance.NuSysNetworkSession.RemoveRegionFromContent(LibraryElementModel.LibraryElementId, regionString);
        }

        /// <summary>
        /// This will change the library element model's title and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetTitle(string title)
        {
            _libraryElementModel.Title = title;
            TitleChanged?.Invoke(this, title);
            _debouncingDictionary.Add("title", title);
        }

        /// <summary>
        /// This will change the library element model's favorited status boolean and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetFavorited(bool favorited)
        {
            _libraryElementModel.Favorited = favorited;
            Favorited?.Invoke(this, favorited);
            _debouncingDictionary.Add("favorited", favorited); 
        }

        /// <summary>
        /// This will change the library element model's metadata dictionary and update the server.  
        /// Then it will fire an event notifying all listeners of the new dictionary they can fetch 
        /// </summary>
        public void ChangeMetadata(Dictionary<string,Tuple<string,bool>> metadata)
        {
            _libraryElementModel.Metadata = metadata;
            MetadataChanged?.Invoke(this);
            _debouncingDictionary.Add("metadata", metadata);
        }

        /// <summary>
        /// Checks if entry is valid, then adds its data to the Metadata dictionary and sends the updated dictionary to the server.
        /// </summary>
        /// <param name="entry"></param>
        public void AddMetadata(MetadataEntry entry)
        {
            //Keys should be unique; values obviously don't have to be.
            if (_libraryElementModel.Metadata.ContainsKey(entry.Key) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return;
            _libraryElementModel.Metadata.Add(entry.Key, new Tuple<string, bool>(entry.Value, entry.Mutability));
            ChangeMetadata(_libraryElementModel.Metadata);
        }

        /// <summary>
        /// Checks if the key string is valid, then updates the metadata dictionary and sends a message to the server with the new dictionary.
        /// </summary>
        /// <param name="k"></param>
        public void RemoveMetadata(String k)
        {
            if (string.IsNullOrEmpty(k) || !_libraryElementModel.Metadata.ContainsKey(k) || string.IsNullOrWhiteSpace(k))
                return;

            _libraryElementModel.Metadata.Remove(k);
            ChangeMetadata(_libraryElementModel.Metadata);
        }

        /// <summary>
        /// This will change make the content controller remove the library element model and this controller
        /// then it will fire the deleted event and dispose of this controller
        /// </summary>
        public void Delete(object sender)
        {
            SessionController.Instance.ActiveFreeFormViewer.DeselectAll();
            SessionController.Instance.ContentController.Remove(this.LibraryElementModel);
            Deleted?.Invoke(this);
            Dispose();
        }

        /// <summary>
        /// This will cause the library element model to load with the associated arguments in the loadingArgs
        /// This will then fire the OnLoaded event
        /// </summary>
        public void Load(LoadContentEventArgs e)
        {
            _libraryElementModel.Data = e.Data;
            _libraryElementModel.Regions = e.RegionStrings;
            //_libraryElementModel.InkLinkes = e.InkStrings;

            IsLoaded = true;
            _onLoaded?.Invoke(this);
        }
        public Uri GetSource()
        {
            string extension = "";
            switch (_libraryElementModel.Type)
            {
                case ElementType.PDF:
                    extension = ".pdf";
                    break;
                case ElementType.Video:
                    extension = ".mp4";
                    break;
                case ElementType.Audio:
                    extension = ".mp3";
                    break;
                case ElementType.Image:
                    extension = ".jpg";
                    break;
            }
            var url = _libraryElementModel.LibraryElementId + extension;
            if (_libraryElementModel.ServerUrl != null)
            {
                url = _libraryElementModel.ServerUrl;
            }
            return new Uri("http://" + WaitingRoomView.ServerName + "/" + url);
        }
        public LibraryElementModel LibraryElementModel
        {
            get
            {
                return _libraryElementModel;
            }
        }
        public virtual void Dispose()
        {
            Disposed?.Invoke(this);
        }
        public void SetLoading(bool loading)
        {
            _loading = true;
        }
        public bool LoadingOrLoaded
        {
            get { return _loading || IsLoaded; }
        }
    }
}
