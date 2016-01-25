using SQLite.Net.Attributes;

namespace NuSysApp
{
    public class NodeContentModel
    {
        public NodeContentModel() { }
        public NodeContentModel(string data, string id)
        {
            Data = data;
            Id = id;
        }

        public string Data { get; set; }
        public string Id { get; set; }

    }
}
