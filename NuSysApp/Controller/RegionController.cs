using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionController : IMetadatable
    {
        public Region Model;

        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;

        public string Title { get; set; }

        public delegate void RegionUpdatedEventHandler(object source, Region region);
        public event RegionUpdatedEventHandler RegionUpdated;
        public RegionController(Region model)
        {
            Model = model;
            Title = model.Name;
            SessionController.Instance.RegionsController.Add(this);
        }

        public void SetTitle(string title)
        {
            Model.Name = title;
            TitleChanged?.Invoke(this, title);
            SessionController.Instance.NuSysNetworkSession.UpdateRegion(Model);
            Title = title;
            
        }
        public Dictionary<string, MetadataEntry> GetMetadata()
        {
            if (Model.Metadata == null)
            {
                Model.Metadata = new Dictionary<string, MetadataEntry>();
            }
            return Model.Metadata;
        }

        public bool AddMetadata(MetadataEntry entry)
        {
            if (entry.Values==null || string.IsNullOrEmpty(entry.Key) || string.IsNullOrWhiteSpace(entry.Key))
                return false;
            if (Model.Metadata.ContainsKey(entry.Key))
            {
                if (entry.Mutability==MetadataMutability.IMMUTABLE)//weird syntax in case we want to change mutability to an enum eventually
                {
                    return false;
                }
                Model.Metadata.Remove(entry.Key);
            }
            Model.Metadata.Add(entry.Key,entry);
            return true;
        }

        public bool RemoveMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Model.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
                return false;

            Model.Metadata.Remove(key);
            return true;
        }
        public List<string> GetMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Model.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            return Model.Metadata[key].Values;
        }

        public MetadatableType MetadatableType()
        {
            return NuSysApp.MetadatableType.Region;
        }
        public void UpdateRegion(Region region)
        {
            RegionUpdated?.Invoke(this, region);
            SessionController.Instance.NuSysNetworkSession.UpdateRegion(region);
        }
    }
}
