
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class LibraryElementModel : BaseINPC
    {
        public bool Loaded { get; set; }

        public HashSet<string> Keywords { get; set; }


        public static LibraryElementModel LitElement;

        public delegate void OnLoadedEventHandler();
        public event OnLoadedEventHandler OnLoaded
        {
            add
            {
                _onLoaded += value;
                if (!Loaded && !_loading)
                {
                    _loading = true;
                    Task.Run(async delegate
                    {
                        SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(Id);
                    });
                }
            }
            remove { _onLoaded -= value; }
        }

        private bool _favorited;

        private event OnLoadedEventHandler _onLoaded;

        public delegate void ContentChangedEventHandler(ElementViewModel originalSenderViewModel = null);
        public event ContentChangedEventHandler OnContentChanged;

        public delegate void TitleChangedEventHandler(object sender, string newTitle);
        public event TitleChangedEventHandler OnTitleChanged;

        public delegate void ElementDeletedEventHandler();
        public event ElementDeletedEventHandler OnDelete;

        public delegate void LightupContentEventHandler(LibraryElementModel sender, bool lightup);
        public event LightupContentEventHandler OnLightupContent;

        public delegate void IsSearchedEventHandler(LibraryElementModel sender, bool lightup);
        public event IsSearchedEventHandler OnSearched;

        public delegate void ElementFavoritedEventHandler(LibraryElementModel sender, bool favorited);
        public event ElementFavoritedEventHandler OnFavorited;

        public delegate void ElementMetadataChangedEventHandler(LibraryElementModel sender);
        public event ElementMetadataChangedEventHandler OnMetadataChanged;



        public ElementType Type { get; set; }

        public string Data
        {
            get { return _data; }
            set
            {
                _data = value;
                ViewUtilBucket = new Dictionary<string, object>();
                OnContentChanged?.Invoke();
            }
        }

        public string Id { get; set; }
        public string Title
        {
            get { return _title; }
            private set
            {
                _title = value;
                RaisePropertyChanged("Title");
                OnTitleChanged?.Invoke(this, _title);
            }
        }

        public Dictionary<string, Tuple<string, Boolean>> Metadata
        {
            get
            {
                return _metadata;
            }
            set
            {
                _metadata = value;
                OnMetadataChanged?.Invoke(this);
            }
        }

        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        public string ServerUrl { get; set; }

        private string _title;

        public Dictionary<string, object> ViewUtilBucket = new Dictionary<string, object>();
        private string _data;
        private bool _loading = false;
        private Dictionary<string, Tuple<string,Boolean>> _metadata;
        public LibraryElementModel(string id, ElementType elementType, Dictionary<string, Tuple<string,Boolean>> metadata = null, string contentName = null, bool favorited = false)
        {
            Data = null;
            Id = id;
            Title = contentName;
            Type = elementType;
            Loaded = false;
            Favorited = favorited;
            Keywords = new HashSet<string>();
            Metadata = metadata;
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
        }
        /// <summary>
        /// Checks if entry is valid, then adds its data to the Metadata dictionary and sends the updated dictionary to the server.
        /// </summary>
        /// <param name="entry"></param>
      
        public void AddMetadata(MetadataEntry entry)
        {
            //Keys should be unique; values obviously don't have to be.
            if (Metadata.ContainsKey(entry.Key) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.Value) || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return;

            Metadata.Add(entry.Key, new Tuple<string, bool>(entry.Value, entry.Mutability));
            Task.Run(async delegate
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeNonAscii };

                var m = new Message();
                m["contentId"] = Id;
                m["metadata"] = JsonConvert.SerializeObject(Metadata, settings);
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });
            //OnMetadataChanged?.Invoke(this);
        }
        /// <summary>
        /// Checks if the key string is valid, then updates the metadata dictionary and sends a message to the server with the new dictionary.
        /// </summary>
        /// <param name="k"></param>
        public void RemoveMetadata(String k)
        {
            if (string.IsNullOrEmpty(k) || !Metadata.ContainsKey(k) || string.IsNullOrWhiteSpace(k))
                return;

            Metadata.Remove(k);

            Task.Run(async delegate
            {
                var m = new Message();
                m["contentId"] = Id;
                m["metadata"] = JsonConvert.SerializeObject(Metadata);
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });

            //OnMetadataChanged?.Invoke(this);

        }

        public void FireLightupContent(bool lightup)
        {
            if (LitElement != null && LitElement != this)
            {
                LitElement.FireLightupContent(false);
            }
            if (lightup)
            {
                LitElement = this;
            }
            else
            {
                LitElement = null;
            }
            OnLightupContent?.Invoke(this, lightup);
        }
        protected virtual void OnSessionControllerEnterNewCollection()
        {
            ViewUtilBucket.Clear();
            Data = null;

            Loaded = false;
            _loading = false;

            var ds = OnContentChanged?.GetInvocationList();
            if (ds != null)
            {
                foreach (var d in ds)
                {
                    OnContentChanged -= (ContentChangedEventHandler)d;
                }
            }
            ds = OnTitleChanged?.GetInvocationList();
            if (ds != null)
            {
                foreach (var d in ds)
                {
                    OnTitleChanged -= (TitleChangedEventHandler)d;
                }
            }
            ds = OnDelete?.GetInvocationList();
            if (ds != null)
            {
                foreach (var d in ds)
                {
                    OnDelete -= (ElementDeletedEventHandler)d;
                }
            }
            ds = OnLightupContent?.GetInvocationList();
            if (ds != null)
            {
                foreach (var d in ds)
                {
                    OnLightupContent -= (LightupContentEventHandler)d;
                }
            }
            ds = _onLoaded?.GetInvocationList();
            if (ds != null)
            {
                foreach (var d in ds)
                {
                    _onLoaded -= (OnLoadedEventHandler)d;
                }
            }
        }

        public void FireDelete()
        {
            OnDelete?.Invoke();
            SessionController.Instance.ContentController.Remove(this);
        }
        public bool LoadingOrLoaded()
        {
            return Loaded || _loading;
        }
        public void Load(string data)
        {
            Data = data;
            Loaded = true;
            _onLoaded?.Invoke();
        }

        public void SetFavorited(bool favorited)
        {

            Task.Run(async delegate
            {
                var m = new Message();
                m["contentId"] = Id;
                m["favorited"] = favorited;
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });
            //OnFavorited?.Invoke(this, favorited);
            Favorited = favorited;
        }

        public bool Favorited
        {
            get { return _favorited; }

            set
            {
                _favorited = value;
                //RaisePropertyChanged("Favorited");
                OnFavorited?.Invoke(this, _favorited);
            }
        }

        public void SetTitle(string title, bool sendRequest = true)
        {
            if (sendRequest)
            {
                Task.Run(async delegate
                {
                    var m = new Message();
                    m["contentId"] = Id;
                    m["title"] = title;
                    var request = new ChangeContentRequest(m);
                    SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                });
            }
            Title = title;
            //  OnTitleChanged?.Invoke(this, title);
        }
        public void SetContentData(ElementViewModel originalSenderViewModel, string data)
        {
            _data = data;

            Task.Run(async delegate
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(Id, data));
            });
            ViewUtilBucket = new Dictionary<string, object>();
            OnContentChanged?.Invoke(originalSenderViewModel);
        }

        public void SetLoading(bool loading)
        {
            _loading = loading;
        }
        public long GetTimestampTicks()
        {
            if (!String.IsNullOrEmpty(Timestamp))
            {
                try
                {
                    return DateTime.Parse(Timestamp).Ticks;
                }
                catch (Exception e)
                {
                    return 0;
                }
            }

            return 0;
        }
    }
}
