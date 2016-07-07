﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NuSysApp
{
    /// <summary>
    /// Takes care of all the modifying and events invoking for the library element model
    /// Should manage keeping the library element model up to date as well as updating the server
    /// </summary>
    public class LibraryElementController : IMetadatable, ILinkable, IDetailViewable
    {
        protected DebouncingDictionary _debouncingDictionary;
        private LibraryElementModel _libraryElementModel;
        private bool _loading = false;
        private RegionControllerFactory _regionControllerFactory = new RegionControllerFactory();
        public string Title {
            get
            {
                return LibraryElementModel?.Title;
            }
            set
            {
                LibraryElementModel.Title = value;
            } 
        }
        
        #region Events
        public delegate void ContentChangedEventHandler(object source, string contentData);
        public delegate void RegionAddedEventHandler(object source, RegionController regionController);
        public delegate void RegionRemovedEventHandler(object source, Region region);
        public delegate void MetadataChangedEventHandler(object source);
        public delegate void DisposeEventHandler(object source);
        public delegate void TitleChangedEventHandler(object sender, string title);
        public delegate void FavoritedEventHandler(object sender, bool favorited);
        public delegate void LoadedEventHandler(object sender);
        public delegate void DeletedEventHandler(object sender);
        public delegate void NetworkUserChangedEventHandler(object source, NetworkUser user);
        public delegate void KeywordsChangedEventHandler(object sender, HashSet<Keyword> keywords);
        public event ContentChangedEventHandler ContentChanged;
        public event RegionAddedEventHandler RegionAdded;
        public event RegionRemovedEventHandler RegionRemoved;
        public event MetadataChangedEventHandler MetadataChanged;
        public event DisposeEventHandler Disposed;
        public event TitleChangedEventHandler TitleChanged;
        public event FavoritedEventHandler Favorited;
        public event DeletedEventHandler Deleted;
        public event KeywordsChangedEventHandler KeywordsChanged;
        public event NetworkUserChangedEventHandler UserChanged;
        public event EventHandler<LinkLibraryElementController> LinkAdded;
        public event EventHandler<string> LinkRemoved;
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
            Title = libraryElementModel.Title;
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
        public void AddRegion(Region region)
        {
            if (_libraryElementModel.Regions == null)
            {
                _libraryElementModel.Regions = new HashSet<Region>();
            }


            _libraryElementModel.Regions.Add(region);


            var factory = new RegionControllerFactory();
            var regionController = factory.CreateFromSendable(region, this.LibraryElementModel.LibraryElementId);

            RegionAdded?.Invoke(this, regionController);
            SessionController.Instance.NuSysNetworkSession.AddRegionToContent(LibraryElementModel.LibraryElementId, region);
        }

        /// <summary>
        /// This will REMOVE a region to the library element model and will update the server accordingly
        /// It will then fire an even notfying all listeners of the old region that was deleted
        /// The entire list of regions can be refetched from the library element model directly if needed
        /// </summary>
        public void RemoveRegion(Region region)
        {
            _libraryElementModel.Regions.Remove(region);
            RegionRemoved?.Invoke(this, region);
            SessionController.Instance.NuSysNetworkSession.RemoveRegionFromContent(region);
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
            Title = title;
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
        private void ChangeMetadata(Dictionary<string,MetadataEntry> metadata)
        {
            LibraryElementModel.SetMetadata(metadata);
            MetadataChanged?.Invoke(this);
            _debouncingDictionary.Add("metadata", LibraryElementModel.Metadata);
        }

        /// <summary>
        /// overloads the other change metadata function
        /// </summary>
        /// <param name="metadata"></param>
        private void ChangeMetadata(ConcurrentDictionary<string, MetadataEntry> metadata)
        {
            ChangeMetadata(new Dictionary<string, MetadataEntry>(metadata));
        }

        /// <summary>
        /// Checks if entry is valid, then adds its data to the Metadata dictionary and sends the updated dictionary to the server.
        /// </summary>
        /// <param name="entry"></param>
        public bool AddMetadata(MetadataEntry entry)
        {
            //Keys should be unique; values obviously don't have to be.
            if (entry.Values==null || string.IsNullOrEmpty(entry.Key) ||
                string.IsNullOrWhiteSpace(entry.Key))
            {
                return false;
            }
            if (_libraryElementModel.Metadata == null)
            {
                _libraryElementModel.Metadata = new ConcurrentDictionary<string, MetadataEntry>();
                return false;
            }

            if (_libraryElementModel.Metadata.ContainsKey(entry.Key))
            {
                if (_libraryElementModel.Metadata[entry.Key].Mutability == MetadataMutability.IMMUTABLE)//weird syntax in case we want to change mutability to an enum eventually
                {
                    return false;
                }
                MetadataEntry outobj;
                _libraryElementModel.Metadata.TryRemove(entry.Key, out outobj);
            }
            _libraryElementModel.Metadata.TryAdd(entry.Key,entry);
            ChangeMetadata(_libraryElementModel.Metadata);
            return true;
        }

        /// <summary>
        /// Checks if the key string is valid, then updates the metadata dictionary and sends a message to the server with the new dictionary.
        /// </summary>
        /// <param name="k"></param>
        public bool RemoveMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !_libraryElementModel.Metadata.ContainsKey(key) ||
                string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            MetadataEntry outobj;
            _libraryElementModel.Metadata.TryRemove(key, out outobj);
            ChangeMetadata(_libraryElementModel.Metadata);
            return true;
        }

        /// <summary>
        /// Returns the value of the metadata at the specified key
        /// null if not exist
        /// </summary>
        public List<string> GetMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !_libraryElementModel.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            return _libraryElementModel.Metadata[key].Values;
        }

        /// <summary>
        /// This will change make the content controller remove the library element model and this controller
        /// then it will fire the deleted event and dispose of this controller
        /// </summary>
        public void Delete()
        {
            SessionController.Instance.ActiveFreeFormViewer?.DeselectAll();
            SessionController.Instance.ContentController.Remove(this.LibraryElementModel);
            Deleted?.Invoke(this);
            Dispose();
        }

        /// <summary>
        /// This will change the library element model's Tags  and update the server.  
        /// Then it will fire an event notifying all listeners of the new list of tags
        /// </summary>
        public void SetKeywords(HashSet<Keyword> keywords)
        {
            _libraryElementModel.Keywords = keywords;
            KeywordsChanged?.Invoke(this, keywords);
            _debouncingDictionary.Add("keywords", keywords);
        }

        /// <summary>
        /// This will add a single keyword to the model's list of keywords and update the server
        /// it will fire the generic 'KeywordsChanged' event
        /// </summary>
        public void AddKeyword(Keyword keyword)
        {
            _libraryElementModel.Keywords.Add(keyword);
            KeywordsChanged?.Invoke(this, _libraryElementModel.Keywords);
            _debouncingDictionary.Add("keywords", _libraryElementModel.Keywords);
        }

        /// <summary>
        /// This will remove a single keyword to the model's list of keywords and update the server
        /// it will fire the generic 'KeywordsChanged' event
        /// </summary>
        public void RemoveKeyword(Keyword keyword)
        {
            foreach (var kw in _libraryElementModel.Keywords)
            {
                if (kw.Equals(keyword))
                {
                    _libraryElementModel.Keywords.Remove(kw);
                    break;
                }
            }
            //_libraryElementModel.Keywords.Remove(keyword);
            KeywordsChanged?.Invoke(this, _libraryElementModel.Keywords);
            _debouncingDictionary.Add("keywords", _libraryElementModel.Keywords);
        }

        /// <summary>
        /// This will cause the library element model to load with the associated arguments in the loadingArgs
        /// This will then fire the OnLoaded event
        /// </summary>
        public void Load(LoadContentEventArgs e)
        {
            if (e.Data != null)
            {
                _libraryElementModel.Data = e.Data;
                ContentChanged?.Invoke(this,e.Data);
            }
            if (e.RegionStrings != null)
            {
                _libraryElementModel.Regions = e.RegionStrings;
            }
            //_libraryElementModel.InkLinkes = e.InkStrings;

            IsLoaded = true;
            _onLoaded?.Invoke(this);
        }
        public Uri LargeIconUri
        {
            get
            {
                if (LibraryElementModel.LargeIconUrl != null)
                {
                    return new Uri("http://" + WaitingRoomView.ServerName + "/" +LibraryElementModel.LargeIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case ElementType.Image:
                    case ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_large.jpg");
                        break;
                    case ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                }
            }
        }
        public Uri MediumIconUri
        {
            get
            {
                if (LibraryElementModel.MediumIconUrl != null)
                {
                    return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.MediumIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case ElementType.Image:
                    case ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_medium.jpg");
                        break;
                    case ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                }
            }
        }

        public virtual void UnPack(Message message)
        {
            if (message.ContainsKey("metadata"))
            {
                var metadata = message.GetDict<string, MetadataEntry>("metadata");
                if (metadata != null)
                {
                    ChangeMetadata(metadata);
                }
            }
        }

        public Uri SmallIconUri
        {
            get
            {
                if (LibraryElementModel.SmallIconUrl != null)
                {
                    return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.SmallIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case ElementType.Image:
                    case ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_small.jpg");
                        break;
                    case ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                        break;
                }
            }
        }
        public Dictionary<string, MetadataEntry> GetMetadata()
        {
            return new Dictionary<string,MetadataEntry>(_libraryElementModel?.Metadata);
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

        public LinkId Id
        {
            get { return new LinkId(this.LibraryElementModel.LibraryElementId); }
        }

        public void SetNetworkUser(NetworkUser user)
        {
            UserChanged?.Invoke(this, user);
        }

        public MetadatableType MetadatableType()
        {
            return NuSysApp.MetadatableType.Content;
        }

        public void AddLink(LinkLibraryElementController linkController)
        {
            LinkAdded?.Invoke(this, linkController);
        }

    /*    public void RemoveLink(LinkLibraryElementController linkController)
        {
            LinkRemoved?.Invoke(this, linkController.Id);
        }*/

        #region Linking methods
        public async Task RequestAddNewLink(LinkId idToLinkTo, string title)
        {
            var m = new Message();
            m["id1"] = new LinkId(this.LibraryElementModel.LibraryElementId);
            m["id2"] = idToLinkTo;
            m["title"] = title;
            await SessionController.Instance.LinkController.RequestLink(m);
        }

        public void RequestRemoveLink(LinkId linkLibraryElementID)
        {
            SessionController.Instance.LinkController.RemoveLink(linkLibraryElementID.LibraryElementId);
        }

        public void ChangeLinkTitle(string linkLibraryElementID, string title)
        {
            SessionController.Instance.LinkController.ChangeLinkTitle(linkLibraryElementID, title);
        }

        public void ChangeLinkTags(string linkLibraryElementID, HashSet<String> tags)
        {
            SessionController.Instance.LinkController.ChangeLinkTags(linkLibraryElementID, tags);
        }

        public HashSet<LinkLibraryElementController> GetAllLinks()
        {
            var linkedIds = SessionController.Instance.LinkController.GetLinkedIds(new LinkId(this.LibraryElementModel.LibraryElementId));
            var controllers = linkedIds.Select(id =>SessionController.Instance.ContentController.GetLibraryElementController(id) as LinkLibraryElementController);
            return new HashSet<LinkLibraryElementController>(controllers);
        }

        public string TabId()
        {
            return LibraryElementModel.LibraryElementId;
        }
        #endregion

    }
}
