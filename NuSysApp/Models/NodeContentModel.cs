using System;
using System.Collections.Generic;
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public NodeContentModel(string data, string id, ElementType elementType,string contentName = null)
        {
            Data = data;
            _dataLoaded = data != null;
            ContentID = id;
            Title = contentName;
            Type = elementType;
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

        public delegate void ContentChangedEventHandler();
        public event ContentChangedEventHandler OnContentChanged;

        public void FireContentChanged()
        {
            OnContentChanged?.Invoke();
        }
        public ElementType Type { get; set; }
        public string Data { get; set; }
        public string ContentID { get; set; }
        public string Title { get; set; }
        private bool _dataLoaded = false;
        public bool DataLoaded { set { _dataLoaded = value; } }
   
    }
}
