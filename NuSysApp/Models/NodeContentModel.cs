
﻿using System;
using System.Collections.Generic;
﻿using System.Threading.Tasks;
﻿using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel : BaseINPC
    {
        public bool Loaded { get; set; }//TODO Add a loaded event
        //TODO add in 'MakeNewController' method that creates a new controller-model pair pointing to this and returns it

        public delegate void ContentChangedEventHandler(ElementViewModel originalSenderViewModel = null);
        public event ContentChangedEventHandler OnContentChanged;

        public delegate void TitleChangedEventHandler(string newTitle);
        public event TitleChangedEventHandler OnTitleChanged;

        public ElementType Type { get; set; }
        public string Data { get; set; }
        public string Id { get; set; }
        public string Title {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
            } 
        }
        public string TimeStamp { get; set; }//TODO maybe put in a timestamp, maybe remove the field from the library

        private string _title;

        public Dictionary<string,object> ViewUtilBucket = new Dictionary<string, object>(); 
        public NodeContentModel(string data, string id, ElementType elementType,string contentName = null)
        {
            Data = data;
            Id = id;
            Title = contentName;
            Type = elementType;
            Loaded = data != null;
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
        public void FireContentChanged()
        {
            ViewUtilBucket = new Dictionary<string, object>();
            OnContentChanged?.Invoke();
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
            Data = data;

            Task.Run(async delegate
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(Id,data));
            });
            ViewUtilBucket = new Dictionary<string, object>();
            OnContentChanged?.Invoke(originalSenderViewModel);
        }
    }
}
