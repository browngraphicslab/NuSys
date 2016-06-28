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
        public Dictionary<string, Tuple<string, bool>> GetMetadata()
        {
            if (Model.Metadata == null)
            {
                Model.Metadata = new Dictionary<string, Tuple<string, bool>>();
            }
            return Model.Metadata;
        }

        public bool AddMetadata(MetadataEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Value) || string.IsNullOrEmpty(entry.Key) || string.IsNullOrWhiteSpace(entry.Key) || string.IsNullOrWhiteSpace(entry.Value))
                return false;
            if (Model.Metadata.ContainsKey(entry.Key))
            {
                if (Model.Metadata[entry.Key].Item2 == false)//weird syntax in case we want to change mutability to an enum eventually
                {
                    return false;
                }
                Model.Metadata.Remove(entry.Key);
            }
            Model.Metadata.Add(entry.Key, new Tuple<string, bool>(entry.Value, entry.Mutability));
            return true;
        }

        public bool RemoveMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Model.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
                return false;

            Model.Metadata.Remove(key);
            return true;
        }
        public string GetMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Model.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
            {
                return null;
            }
            return Model.Metadata[key].Item1;
        }

        public MetadatableType MetadatableType()
        {
            return NuSysApp.MetadatableType.Region;
        }
    }
}
