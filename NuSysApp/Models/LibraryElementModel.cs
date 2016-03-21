
﻿using System;
using System.Collections.Generic;
﻿using System.Threading.Tasks;
﻿using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class LibraryElementModel : BaseINPC
    {
        public bool Loaded { get; set; }

        public delegate void OnLoadedEventHandler();
        public event OnLoadedEventHandler OnLoaded {
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

        private event OnLoadedEventHandler _onLoaded;

        public delegate void ContentChangedEventHandler(ElementViewModel originalSenderViewModel = null);
        public event ContentChangedEventHandler OnContentChanged;

        public delegate void TitleChangedEventHandler(string newTitle);
        public event TitleChangedEventHandler OnTitleChanged;

        public delegate void ElementDeletedEventHandler();
        public event ElementDeletedEventHandler OnDelete;
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
        public string Title {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            } 
        }
        public string Creator { set; get; }
        public string Timestamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library
        private string _title;

        public Dictionary<string,object> ViewUtilBucket = new Dictionary<string, object>();
        private string _data;
        private bool _loading = false;
        public LibraryElementModel(string id, ElementType elementType, string contentName = null)
        {
            Data = null;
            Id = id;
            Title = contentName;
            Type = elementType;
            Loaded = false;
            SessionController.Instance.OnEnterNewCollection += OnSessionControllerEnterNewCollection;
        }

        protected virtual void OnSessionControllerEnterNewCollection()
        {
            ViewUtilBucket.Clear();
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
        public bool InSearch(string s)
        {
            var title = Title?.ToLower() ?? "";
            var type = Type.ToString().ToLower();
            if (title.Contains(s) || type.Contains(s))
            {
                return true;
            }
            return false;
        }

        public void SetTitle(string title)
        {
            Task.Run(async delegate
            {
                var m  = new Message();
                m["contentId"] = Id;
                m["title"] = title;
                var request = new ChangeContentRequest(m);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            });
            Title = title;
            OnTitleChanged?.Invoke(title);
        }
        public void SetContentData(ElementViewModel originalSenderViewModel, string data)
        {
            _data = data;

            Task.Run(async delegate
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(Id,data));
            });
            ViewUtilBucket = new Dictionary<string, object>();
            OnContentChanged?.Invoke(originalSenderViewModel);
        }

        public void SetLoading(bool loading)
        {
            _loading = loading;
        }
    }
}
