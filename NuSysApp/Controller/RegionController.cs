using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionController : IMetadatable, ILinkTabable, IDetailViewable
    {
        public Region Model;
        
        public string Title
        {
            get { return Model?.Name; }
            set { SetTitle(value);}
        }

        public string ContentId
        {
            get { return SessionController.Instance.RegionsController.GetLibraryElementModelId(Model.Id); }
        }


        public delegate void TitleChangedEventHandler(object source, string title);
        public event TitleChangedEventHandler TitleChanged;
        public delegate void RegionUpdatedEventHandler(object source, Region region);
        public event RegionUpdatedEventHandler RegionUpdated;
        public delegate void SelectHandler(RegionController regionController);
        public event SelectHandler OnSelect;
        public delegate void DeselectHandler(RegionController regionController);
        public event DeselectHandler OnDeselect;
        public event EventHandler<LinkLibraryElementController> LinkAdded;
        public event EventHandler<string> LinkRemoved;
        public delegate void MetadataChangedEventHandler(object source);
        public event MetadataChangedEventHandler MetadataChanged;




        private bool _selected;
        private bool _blockServerUpdates;
        public RegionController(Region model)
        {
            Model = model;

        }
        public void SetTitle(string title)
        {
            Model.Name = title;
            TitleChanged?.Invoke(this, title);
            UpdateServer();
        }
        public Dictionary<string, MetadataEntry> GetMetadata()
        {
           if (Model == null)
            {
                return null;
            }

           if (Model.Metadata == null)
            {
                Model.Metadata = new ConcurrentDictionary<string, MetadataEntry>(new Dictionary<string, MetadataEntry>());
                UpdateServer();
            }
            return new Dictionary<string, MetadataEntry>(Model?.Metadata);

        }

        //public bool AddMetadata(MetadataEntry entry)
        //{
        //    if (entry.Values==null || string.IsNullOrEmpty(entry.Key) || string.IsNullOrWhiteSpace(entry.Key))
        //        return false;
        //    if (Model.Metadata.ContainsKey(entry.Key))
        //    {
        //        if (entry.Mutability==MetadataMutability.IMMUTABLE)//weird syntax in case we want to change mutability to an enum eventually
        //        {
        //            return false;
        //        }
        //        Model.Metadata.Remove(entry.Key);
        //    }
        //    Model.Metadata.Add(entry.Key,entry);
        //    return true;
        //}
        public bool AddMetadata(MetadataEntry entry)
        {
            //Keys should be unique; values obviously don't have to be.
            if (entry.Values == null || string.IsNullOrEmpty(entry.Key) ||
                string.IsNullOrWhiteSpace(entry.Key))
            {
                return false;
            }
            if (Model.Metadata == null)
            {
                Model.Metadata = new ConcurrentDictionary<string, MetadataEntry>();
                return false;
            }

            if (Model.Metadata.ContainsKey(entry.Key))
            {
                if (Model.Metadata[entry.Key].Mutability == MetadataMutability.IMMUTABLE)
                {
                    return false;
                }
                MetadataEntry outobj;
                Model.Metadata.TryRemove(entry.Key, out outobj);
            }
            Model.Metadata.TryAdd(entry.Key, entry);
            ChangeMetadata(new Dictionary<string, MetadataEntry>(Model.Metadata));
            return true;
        }

        private void ChangeMetadata(Dictionary<string, MetadataEntry> metadata)
        {
            Model.SetMetadata(metadata);
            MetadataChanged?.Invoke(this);
            UpdateServer();
        }
        public bool RemoveMetadata(string key)
        {
            if (string.IsNullOrEmpty(key) || !Model.Metadata.ContainsKey(key) || string.IsNullOrWhiteSpace(key))
                return false;
            var value = Model.Metadata[key];
            Model.Metadata.TryRemove(key, out value);
            return true;
        }

        /// <summary>
        /// Updates the original metadata entry with the passed in key and values 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="updated"></param>
        /// <returns></returns>
        public bool UpdateMetadata(MetadataEntry original, string key, List<string> values)
        {
            // Error checking for the passed in parameters
            if (original == null || string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key)||values==null || !Model.Metadata.ContainsKey(original.Key))
            {
                return false;
            }

            // Updates the metadata entry
            var newEntry = new MetadataEntry(key, values, original.Mutability);
            Model.Metadata.TryUpdate(original.Key, newEntry, newEntry);
            ChangeMetadata(new Dictionary<string, MetadataEntry>(Model.Metadata));
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

        protected void UpdateServer()
        {
            if (!_blockServerUpdates)
            {
                SessionController.Instance.NuSysNetworkSession.UpdateRegion(Model);
            }
        }

        protected void SetBlockServerBoolean(bool blockServerUpdates)
        {
            _blockServerUpdates = blockServerUpdates;
        }
        public MetadatableType MetadatableType()
        {
            return NuSysApp.MetadatableType.Region;
        }
        public void UpdateRegion(Region region)
        {
            RegionUpdated?.Invoke(this, region);
            UpdateServer();
        }

        public void Select()
        {
            _selected = true;
            OnSelect?.Invoke(this);
        }

        public void Deselect()
        {
            _selected = false;
            OnDeselect?.Invoke(this);
        }

        public void AddLink(LinkLibraryElementController linkController)
        {
            LinkAdded?.Invoke(this, linkController);
        }

     /*   public void RemoveLink(LinkLibraryElementController linkController)
        {
            LinkRemoved?.Invoke(this, linkController.ContentId);
        }*/

        #region Linking methods
        public async Task RequestAddNewLink(string idToLinkTo, string title)
        {
            var m = new Message();
        //    var contentId = SessionController.Instance.RegionsController.GetLibraryElementModelId(this.Model.ContentId); // The ID of the library element this region is associated with.
            m["id1"] = this.ContentId;
            m["id2"] = idToLinkTo;
            m["title"] = title;
            await SessionController.Instance.LinksController.RequestLink(m);
            //SessionController.Instance.LinksController.RequestLink(new LinkId( SessionController.Instance.RegionsController.GetLibraryElementModelId(this.Model.ContentId),this.Model.ContentId), idToLinkTo);
        }

        public void RequestRemoveLink(string linkLibraryElementID)
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                new DeleteLibraryElementRequest(linkLibraryElementID));
        }
        
        public HashSet<LinkLibraryElementController> GetAllLinks()
        {
            //var libraryElementIdForRegion = SessionController.Instance.RegionsController.GetLibraryElementModelId(Model.Id);
            var linkedIds = SessionController.Instance.LinksController.GetLinkedIds(Model.Id);
            var controllers = linkedIds.Select(id => SessionController.Instance.ContentController.GetLibraryElementController(id) as LinkLibraryElementController);
            return new HashSet<LinkLibraryElementController>(controllers);
        }

        /// <summary>
        /// This mehtod should only be called from the server upon other updates.  It will pass in a region
        /// you should extract the region's properties and call the update methods in the controllers
        /// </summary>
        /// <param name="region"></param>
        public virtual void UnPack(Region region)
        { 
            SetBlockServerBoolean(true);//this is a must otherwise infinite loops will occur
            if (Model.Name != region.Name)
            {
                SetTitle(region.Name);
            }
            SetBlockServerBoolean(false);//THIS is a must otherwise changes wont be saved
        }

        public string TabId()
        {
            return Model.Id;
        }
        #endregion

    }
}
