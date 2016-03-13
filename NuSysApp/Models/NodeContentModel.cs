using System;
using System.Collections.Generic;
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public NodeContentModel() { }
        public NodeContentModel(string data, string id, ElementType elementType,string contentName = null)
        {
            Data = data;
            Id = id;
            ContentName = contentName;
            Type = elementType;
        }

        public NodeContentModel(Dictionary<string, object> dict)
        {
            //id, data, type, title
            Id = (string)dict["id"];
            if (dict.ContainsKey("title"))
            {
                ContentName = (string)dict["title"]; // title
            }
            if (dict.ContainsKey("type"))
            {
                Type = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
            }
        }

        public bool InSearch(string s)
        {
            var title = ContentName?.ToLower() ?? "";
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
        public string Id { get; set; }
        public string ContentName { get; set; }
    }
}
