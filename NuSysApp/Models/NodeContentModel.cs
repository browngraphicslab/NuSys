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
