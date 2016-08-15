﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Takes care of all the modifying and events invoking for the library element model
    /// Should manage keeping the library element model up to date as well as updating the server
    /// </summary>
    public class LibraryElementController : IMetadatable
    {
        protected DebouncingDictionary _debouncingDictionary;
        private LibraryElementModel _libraryElementModel;
        private RegionControllerFactory _regionControllerFactory = new RegionControllerFactory();
        protected bool _blockServerInteraction = false;
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

        /// <summary>
        /// To replace the old data stored in the libraryElementModel.  
        /// Will use the contentDataModel Id of the library element model to fetch the string data.  
        /// </summary>
        public string Data
        {
            get
            {
                if (string.IsNullOrEmpty(LibraryElementModel.ContentDataModelId))
                {
                    return null;
                }
                var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(LibraryElementModel.ContentDataModelId);
                Debug.Assert(contentDataModel != null);

                return contentDataModel?.Data;
            }
        }

        #region Events
        public delegate void ContentChangedEventHandler(object source, string contentData);
        public delegate void MetadataChangedEventHandler(object source);
        public delegate void FavoritedEventHandler(object sender, bool favorited);
        public delegate void DeletedEventHandler(object sender);
        public delegate void NetworkUserChangedEventHandler(object source, NetworkUser user);
        public delegate void KeywordsChangedEventHandler(object sender, HashSet<Keyword> keywords);
        public event ContentChangedEventHandler ContentChanged;
        public event MetadataChangedEventHandler MetadataChanged;
        public event EventHandler Disposed;
        public event EventHandler<string> TitleChanged;
        public event FavoritedEventHandler Favorited;
        public event DeletedEventHandler Deleted;
        public event KeywordsChangedEventHandler KeywordsChanged;
        public event NetworkUserChangedEventHandler UserChanged;
        public event EventHandler<LinkLibraryElementController> LinkAdded;
        public event EventHandler<string> LinkRemoved;

        /// <summary>
        /// the event that is fired when the access type of this controller's library element changes. 
        /// The passed AccessType is the new AccessType of the LibraryElementModel;
        /// </summary>
        public event EventHandler<NusysConstants.AccessType> AccessTypeChanged;

        #endregion Events

        /// <summary>
        /// returns the contentDataModel for this LibraryElementController's LibraryElementModel's ContentDataModelId.  
        /// Will return null if it doesn't exist LOCALLY.  
        /// </summary>
        public ContentDataModel ContentDataModel
        {
            get
            {
                Debug.Assert(LibraryElementModel.ContentDataModelId != null);
                return SessionController.Instance.ContentController.GetContentDataModel(LibraryElementModel.ContentDataModelId);
            }
        }

        /// <summary>
        /// returns whether the current library element's content Data Model is loaded (aka just locally present);
        /// </summary>
        public bool ContentLoaded
        {
            get
            {
                return SessionController.Instance.ContentController.ContainsContentDataModel( LibraryElementModel.ContentDataModelId);
            }
        }

        /// <summary>
        /// use this as a getter of the entire set of metadata for an object.
        /// The 'automatic metadata' will be found here
        /// </summary>
        public Dictionary<string, MetadataEntry> FullMetadata
        {
            get
            {
                var metadata = new Dictionary<string, MetadataEntry>(LibraryElementModel.Metadata ?? new ConcurrentDictionary<string, MetadataEntry>());
                if (!metadata.ContainsKey("Timestamp"))
                {
                    metadata.Add("Timestamp", new MetadataEntry("Timestamp", new List<string> { LibraryElementModel.Timestamp }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Creator"))
                {
                    metadata.Add("Creator", new MetadataEntry("Creator", new List<string> { LibraryElementModel.Creator }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Title"))
                {
                    metadata.Add("Title", new MetadataEntry("Title", new List<string> { Title }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Type"))
                {
                    metadata.Add("Type", new MetadataEntry("Type", new List<string> { LibraryElementModel.Type.ToString() }, MetadataMutability.IMMUTABLE));
                }
                if (!metadata.ContainsKey("Keywords"))
                {
                    var keywords = (LibraryElementModel.Keywords ?? new HashSet<Keyword>()).Select(key => key.Text);
                    metadata.Add("Keywords", new MetadataEntry("Keywords", new List<string>(keywords), MetadataMutability.IMMUTABLE));
                }
                return metadata;
            }
        }

        /// <summary>
        /// Constuctor just takes in the library element model it will be libraryElementController
        /// </summary>
        /// 
        public LibraryElementController(LibraryElementModel libraryElementModel)
        {
            Debug.Assert(libraryElementModel != null);
            _libraryElementModel = libraryElementModel;
            _debouncingDictionary = new DebouncingDictionary(libraryElementModel.LibraryElementId, true);
            Title = libraryElementModel.Title;
            SessionController.Instance.EnterNewCollectionStarting += OnSessionControllerEnterNewCollectionStarting;
        }

        /// <summary>
        /// the event handler will be called whenever a new collection is entered via the session controller's EnterCollection method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="newCollectionLibraryId"></param>
        protected virtual void OnSessionControllerEnterNewCollectionStarting(object sender, string newCollectionLibraryId){ }

        /// <summary>
        /// This will change the library element model's data and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetContentData (string contentData)
        {
            //TODO add in checks and error handling for the line below
            SessionController.Instance.ContentController.GetContentDataModel(LibraryElementModel.ContentDataModelId).SetData(contentData);
            ContentChanged?.Invoke(this, contentData);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("data", contentData);
            }
        }

        /// <summary>
        /// This will change the library element model's title and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetTitle(string title)
        {
            _libraryElementModel.Title = title;
            TitleChanged?.Invoke(this, title);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY, title);
            }
        }

        /// <summary>
        /// This will change the library element model's favorited status boolean and update the server.  
        /// Then it will fire an event notifying all listeners of the change
        /// </summary>
        public void SetFavorited(bool favorited)
        {
            _libraryElementModel.Favorited = favorited;
            Favorited?.Invoke(this, favorited);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY, favorited);
            }
        }

        /// <summary>
        /// This will change the library element model's metadata dictionary and update the server.  
        /// Then it will fire an event notifying all listeners of the new dictionary they can fetch 
        /// </summary>
        private void ChangeMetadata(Dictionary<string,MetadataEntry> metadata)
        {
            
            LibraryElementModel.SetMetadata(metadata);
            MetadataChanged?.Invoke(this);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add("metadata", LibraryElementModel.Metadata);
            }
        }
        /// <summary>
        /// will prevent the controller from updating the server at all if true
        /// </summary>
        /// <param name="blockServerUpdates"></param>
        protected void SetBlockServerBoolean(bool blockServerUpdates)
        {
            _blockServerInteraction = blockServerUpdates;
        }

        /// <summary>
        /// This method should be called whenever you want to set the access Type of the library element model for this controller.
        /// It takes in a new access type enum.  
        /// It will fire an event notifying all listeners of the new access type. 
        /// This method will also update th server and all other clients IF this controler is not currently in 'block server interaction" mode indicated by the _blockServerInteraction boolean.
        /// </summary>
        /// <param name="newAccessType"></param>
        public void SetAccessType(NusysConstants.AccessType newAccessType)
        {
            //TODO set the model's access type after the merge and the access type exists in the LEM base class
            AccessTypeChanged?.Invoke(this, newAccessType);
            if (!_blockServerInteraction)
            {
                //it's important here to add the enum as a string
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY, newAccessType.ToString());
            }
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
                if (_libraryElementModel.Metadata[entry.Key].Mutability == MetadataMutability.IMMUTABLE)
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
            ChangeMetadata(LibraryElementModel.Metadata);
            return true;
        }

        /// <summary>
        /// Updates the passed in metadata with the passed in key and values
        /// </summary>
        /// <param name="original"></param>
        /// <param name="key"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool UpdateMetadata(MetadataEntry original, string key, List<string> values)
        {
            // Error checking for the passed in parameters
            if (original == null || string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key) || values == null || !_libraryElementModel.Metadata.ContainsKey(original.Key))
            {
                return false;
            }

            // Updates the metadata entry
            var newEntry = new MetadataEntry(key, values, original.Mutability);
            _libraryElementModel.Metadata.TryUpdate(original.Key, newEntry,newEntry);
            ChangeMetadata(FullMetadata);
            return true;
        }

        /// <summary>
        /// This is an overload for Update Metadata that only taken in the original key instead of taking in the entire entry
        /// </summary>
        /// <param name="originalKey"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public bool UpdateMetadata(string originalKey, List<string> values)
        {
            // Error checking for the passed in parameters
            if (originalKey == null || string.IsNullOrEmpty(originalKey) || string.IsNullOrWhiteSpace(originalKey) || values == null || !_libraryElementModel.Metadata.ContainsKey(originalKey))
            {
                return false;
            }

            // Updates the metadata entry
            var newEntry = new MetadataEntry(originalKey, values, MetadataMutability.MUTABLE);
            _libraryElementModel.Metadata.TryUpdate(originalKey, newEntry, newEntry);
            ChangeMetadata(FullMetadata);
            return true;
        }

        /// <summary>
        /// Returns the value of the metadata at the specified key
        /// null if not exist
        /// </summary>
        public List<string> GetMetadata(string key)
        {
            var full = FullMetadata;
            if (string.IsNullOrEmpty(key) || !full.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            return full[key].Values;
        }

        /// <summary>
        /// This will change make the content libraryElementController remove the library element model and this libraryElementController
        /// then it will fire the deleted event and dispose of this libraryElementController
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
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY, keywords);
            }
        }

        /// <summary>
        /// This will add a single keyword to the model's list of keywords and update the server
        /// it will fire the generic 'KeywordsChanged' event
        /// </summary>
        public void AddKeyword(Keyword keyword)
        {
            _libraryElementModel.Keywords.Add(keyword);
            KeywordsChanged?.Invoke(this, _libraryElementModel.Keywords);
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY, _libraryElementModel.Keywords);
            }
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
            if (!_blockServerInteraction)
            {
                _debouncingDictionary.Add(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY, _libraryElementModel.Keywords);
            }
        }

        /// <summary>
        /// will await the full loading of the content for this library element model.  
        /// Simply calls the nusysNetworkSession's FetchContentDataModelAsync method
        /// </summary>
        /// <returns></returns>
        public async Task LoadContentDataModelAsync()
        {
            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(LibraryElementModel.ContentDataModelId);
        }

        public Uri LargeIconUri
        {
            get
            {
                if (!string.IsNullOrEmpty(LibraryElementModel.LargeIconUrl))
                {
                    return new Uri(LibraryElementModel.LargeIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case NusysConstants.ElementType.Image:
                    case NusysConstants.ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_large.jpg");//TODO just had default icons 
                        break;
                    case NusysConstants.ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case NusysConstants.ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case NusysConstants.ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case NusysConstants.ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case NusysConstants.ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case NusysConstants.ElementType.Link:
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
                if (!string.IsNullOrEmpty(LibraryElementModel.MediumIconUrl))
                {
                    return new Uri(LibraryElementModel.MediumIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case NusysConstants.ElementType.Image:
                    case NusysConstants.ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_medium.jpg");//TODO just had default icons 
                        break;
                    case NusysConstants.ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case NusysConstants.ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case NusysConstants.ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case NusysConstants.ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case NusysConstants.ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case NusysConstants.ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                }
            }
        }

        public Uri SmallIconUri
        {
            get
            {
                if (!string.IsNullOrEmpty(LibraryElementModel.SmallIconUrl))
                {
                    return new Uri(LibraryElementModel.SmallIconUrl);
                }
                switch (LibraryElementModel.Type)
                {
                    case NusysConstants.ElementType.Image:
                    case NusysConstants.ElementType.Video:
                        return new Uri("http://" + WaitingRoomView.ServerName + "/" + LibraryElementModel.LibraryElementId + "_thumbnail_small.jpg");//TODO just had default icons 
                        break;
                    case NusysConstants.ElementType.PDF:
                        return new Uri("ms-appx:///Assets/library_thumbnails/pdf.png");
                        break;
                    case NusysConstants.ElementType.Audio:
                        return new Uri("ms-appx:///Assets/library_thumbnails/audio.png");
                        break;
                    case NusysConstants.ElementType.Text:
                        return new Uri("ms-appx:///Assets/library_thumbnails/text.png");
                        break;
                    case NusysConstants.ElementType.Collection:
                        return new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png");
                        break;
                    case NusysConstants.ElementType.Word:
                        return new Uri("ms-appx:///Assets/library_thumbnails/word.png");
                        break;
                    case NusysConstants.ElementType.Link:
                        return new Uri("ms-appx:///Assets/library_thumbnails/link.png");
                        break;
                    default:
                        return new Uri("ms-appx:///Assets/icon_chat.png");
                        break;
                }
            }
        }

        public virtual void UnPack(Message message)
        {
            SetBlockServerBoolean(true);
            if (message.ContainsKey("metadata"))
            {
                var metadata = message.GetDict<string, MetadataEntry>("metadata");
                if (metadata != null)
                {
                    ChangeMetadata(metadata);
                }
            }
            if (message.ContainsKey("data"))
            {
                SetContentData(message.GetString("data"));
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY))
            {
                SetTitle(message.GetString(NusysConstants.LIBRARY_ELEMENT_TITLE_KEY));
            }
            if (message.ContainsKey(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY))
            {
                SetKeywords(message.GetHashSet<Keyword>(NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY));
            }
            if (message.GetString(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY) != null)
            {
                LibraryElementModel.SmallIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_SMALL_ICON_URL_KEY);
            }
            if (message.GetString(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY) != null)
            {
                LibraryElementModel.MediumIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY);
            }
            if (message.GetString(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY) != null)
            {
                LibraryElementModel.LargeIconUrl = message.GetString(NusysConstants.LIBRARY_ELEMENT_LARGE_ICON_URL_KEY);
            }
            if (message.GetString("creator_user_id") != null)
            {
                LibraryElementModel.Creator = message.GetString("creator_user_id");
            }
            if (message.GetString("library_element_creation_timestamp") != null)
            {
                LibraryElementModel.Timestamp = message.GetString("library_element_creation_timestamp");
            }
            if (message.GetString("server_url") != null)
            {
                LibraryElementModel.ServerUrl = message.GetString("server_url");
            }
            if (message.ContainsKey("content__id"))
            {
                LibraryElementModel.ContentDataModelId = message.GetString("content__id");
            }
            //TODO set regions maybe
            SetBlockServerBoolean(false);
        }

        public Dictionary<string, MetadataEntry> GetMetadata()
        {
            return FullMetadata;
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
            SessionController.Instance.EnterNewCollectionStarting -= OnSessionControllerEnterNewCollectionStarting;
            Disposed?.Invoke(this, EventArgs.Empty);
        }

        public string ContentId
        {
            get
            {
                Debug.Assert(LibraryElementModel?.LibraryElementId != null);
                return LibraryElementModel.LibraryElementId;
            }
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

        #region Linking methods
        public async Task RequestAddNewLink(string idToLinkTo, string title)
        {
            var m = new Message();
            //these seem to be backwards, but it works, probably
            m["id1"] = idToLinkTo; 
            m["id2"] = LibraryElementModel.LibraryElementId;
            m["title"] = title;
            await SessionController.Instance.LinksController.RequestLink(m);
        }

        public void RequestRemoveLink(string linkLibraryElementID)
        {
            Debug.Assert(SessionController.Instance.LinksController.GetLinkableIdsOfContentIdInstances(linkLibraryElementID).Count() != 0);
            LinkRemoved?.Invoke(this, linkLibraryElementID);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(
                new DeleteLibraryElementRequest(linkLibraryElementID));
            
        }
        public HashSet<LinkLibraryElementController> GetAllLinks()
        {
            var linkedIds = SessionController.Instance.LinksController.GetLinkedIds(LibraryElementModel.LibraryElementId);
            var controllers = linkedIds.Select(id =>SessionController.Instance.ContentController.GetLibraryElementController(id) as LinkLibraryElementController);
            return new HashSet<LinkLibraryElementController>(controllers);
        }
        #endregion

        /// <summary>
        /// Adds a shape property to the library element if the element is a collection
        /// </summary>
        /// <param name="shapePoints"></param>
        public void SetShape(List<Windows.Foundation.Point> shapePoints)
        {
            if (LibraryElementModel is CollectionLibraryElementModel)
            {
                var collectionModel = LibraryElementModel as CollectionLibraryElementModel;
                collectionModel.ShapePoints = new List<PointModel>(shapePoints.Select(p => new PointModel(p.X,p.Y)));

            }    
        }

        /// <summary>
        /// Sets if the collection is finite
        /// </summary>
        /// <param name="isFinite"></param>
        public void SetFinite(bool isFinite)
        {
            if (LibraryElementModel is CollectionLibraryElementModel)
            {
                var collectionModel = LibraryElementModel as CollectionLibraryElementModel;
                collectionModel.IsFinite=isFinite;

            }

        }

        /// <summary>
        /// creates a new element of this controller's libraryElementModel.  
        /// It creates it at the passed in X and Y location, and on the given collection Id.
        /// If the given collection ID is null, it will default to the Session's current workspace.
        /// The Id is the LibraryElementId of the collection. 
        /// 
        /// returns whether request was succesful and the element added
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public async Task<bool> AddElementAtPosition(double x, double y, string collectionId = null)
        {
            //the workspace id we are using is the passes in one, or the session's current workspace Id if it is null
            collectionId = collectionId ?? SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;

            //create the request args 
            var elementArgs = new NewElementRequestArgs();
            elementArgs.LibraryElementId = LibraryElementModel.LibraryElementId;
            elementArgs.Height = 300;//TODO abstract to constant in NusysApp.Constants class
            elementArgs.Width = 300;//TODO abstract to constant in NusysApp.Constants class
            elementArgs.ParentCollectionId = collectionId;
            elementArgs.X = x;
            elementArgs.Y = y;
            
            //create the request
            var request = new NewElementRequest(elementArgs);

            //execute the request, await return
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);

            if (request.WasSuccessful() == true) //if it returned sucesssfully
            {
                request.AddReturnedElementToSession();
                return true;
            }
            else
            {
                //maybe notify user
                return false;
            }
        }

    }
}
