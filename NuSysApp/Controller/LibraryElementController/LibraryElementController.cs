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
        public LibraryElementController(LibraryElementModel libraryElementModel)
        {
            Debug.Assert(libraryElementModel != null);
            _libraryElementModel = libraryElementModel;
            _debouncingDictionary = new DebouncingDictionary(libraryElementModel.LibraryElementId, true);
        }
        public void SetContentData (string contentData)
        {
            _libraryElementModel.Data = contentData;
            ContentChanged?.Invoke(this, contentData);
            _debouncingDictionary.Add("data", contentData);
        }
        public void AddRegion(string regionString)
        {
            _libraryElementModel.Regions.Add(regionString);
            RegionAdded?.Invoke(this, regionString);
            SessionController.Instance.NuSysNetworkSession.AddRegionToContent(LibraryElementModel.LibraryElementId, regionString);
        }
        public void RemoveRegion(string regionString)
        {
            _libraryElementModel.Regions.Remove(regionString);
            RegionRemoved?.Invoke(this, regionString);
            SessionController.Instance.NuSysNetworkSession.RemoveRegionFromContent(LibraryElementModel.LibraryElementId, regionString);
        }
        public void SetTitle(string title)
        {
            _libraryElementModel.Title = title;
            TitleChanged?.Invoke(this, title);
            _debouncingDictionary.Add("title", title);
        }
        public void SetFavorited(bool favorited)
        {
            _libraryElementModel.Favorited = favorited;
            Favorited?.Invoke(this, favorited);
            _debouncingDictionary.Add("favorited", favorited); 
        }
        public void ChangeMetadata(Dictionary<string,Tuple<string,bool>> metadata)
        {
            _libraryElementModel.Metadata = metadata;
            MetadataChanged?.Invoke(this);
            _debouncingDictionary.Add("metadata", metadata);
        }
        public void Delete(object sender)
        {
            Deleted?.Invoke(this);
            SessionController.Instance.ActiveFreeFormViewer.DeselectAll();
            SessionController.Instance.ContentController.Remove(this.LibraryElementModel);
            Dispose();
        }
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
