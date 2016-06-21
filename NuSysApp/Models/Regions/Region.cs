using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public abstract class Region : IMetadatable
    {
        public enum RegionType
        {
            Rectangle,
            Time,
            Compound,
            Video,
            Pdf
        }
        public RegionType Type { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public Dictionary<string, Tuple<string, bool>> Metadata { get; set; }
        public Region(string name = "Untitled Region")
        {
            Id = SessionController.Instance.GenerateId();
            Name = name;
        }

        public Dictionary<string, Tuple<string, bool>> GetMetadata()
        {
            return Metadata;
        }

        public bool AddMetadata(MetadataEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.Key) || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return false;
            if (Metadata.ContainsKey(entry.Key))
            {
                if (Metadata[entry.Key].Item2 == false)//weird syntax in case we want to change mutability to an enum eventually
                {
                    return false;
                }
                Metadata.Remove(entry.Key);
            }
            Metadata.Add(entry.Key, new Tuple<string, bool>(entry.Value, entry.Mutability));
            return true;
        }

        public bool RemoveMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
                return false;

            Metadata.Remove(key);
            return true;
        }
        public string GetMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            return Metadata[key].Item1;
        }
    }
}