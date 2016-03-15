
﻿using System;
using System.Collections.Generic;
﻿using System.Threading.Tasks;
﻿using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public bool Loaded { get; set; }//TODO Add a loaded event
        //TODO add in 'MakeNewController' method that creates a new controller-model pair pointing to this and returns it

        public delegate void ContentChangedEventHandler(ElementViewModel originalSenderViewModel = null);
        public event ContentChangedEventHandler OnContentChanged;

        public ElementType Type { get; set; }
        public string Data { get; set; }
        public string Id { get; set; }
        public string ContentID { get; set; }
        public string Title { get; set; }
        public NodeContentModel(string data, string id, ElementType elementType,string contentName = null)
        {
            Data = data;
            ContentID = id;
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
            OnContentChanged?.Invoke();
        }

        public void SetContentData(ElementViewModel originalSenderViewModel, string data)
        {
            Data = data;

            Task.Run(async delegate
            {
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new ChangeContentRequest(Id,data));
            });

            OnContentChanged?.Invoke(originalSenderViewModel);
        }
    }
}
