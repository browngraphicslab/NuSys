using System.Collections.Generic;
using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public NodeContentModel() { }
        public NodeContentModel(string data, string id, string contentName = null, List<string> aliases = null)
        {
            Data = data;
            Id = id;
            ContentName = contentName;
            Aliases = aliases ?? new List<string>();
        }

        public string Data { get; set; }
        public string Id { get; set; }
        public string ContentName { get; set; }
        public List<string> Aliases { get; set; }
    }
}
