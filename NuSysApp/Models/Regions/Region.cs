using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public abstract class Region  
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
        public ConcurrentDictionary<string, MetadataEntry> Metadata { get; set; }
        public Region(string name = "Untitled Region", Dictionary<string, MetadataEntry> metadata = null)
        {
            Id = SessionController.Instance.GenerateId();
            Name = name;
            Metadata = new ConcurrentDictionary<string, MetadataEntry>(metadata ?? new Dictionary<string, MetadataEntry>());
            AddDefaultMetadata();

        }
        
        private void AddDefaultMetadata()
        {
            //Add immutable data to the dictionary; this can then be displayed in the Metadata Editor
            
            if (!Metadata.ContainsKey("Title"))
            {
                Metadata.TryAdd("Title", new MetadataEntry("Title", new List<string> { Name }, MetadataMutability.IMMUTABLE));
            }
            if (!Metadata.ContainsKey("Type"))
            {
                Metadata.TryAdd("Type", new MetadataEntry("Type", new List<string> { Type.ToString() + "Region" }, MetadataMutability.IMMUTABLE));
            }
        }

        public void SetMetadata(Dictionary<string, MetadataEntry> metadata)
        {
            Metadata = new ConcurrentDictionary<string, MetadataEntry>(metadata);
            AddDefaultMetadata();
        }

    }
}