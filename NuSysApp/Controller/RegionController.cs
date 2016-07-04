using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class RegionController : IMetadatable, ILinkable
    {
        public Region Model;
        public string Title { get; set; }

        public LinkId Id
        {
            get { return new LinkId(SessionController.Instance.RegionsController.GetLibraryElementModelId(this.Model.Id),this.Model.Id); }
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

        private bool _selected;
        public RegionController(Region model)
        {
            Model = model;
            Title = model.Name;
        }
        public void SetTitle(string title)
        {
            Model.Name = title;
            Title = title;
            TitleChanged?.Invoke(this, title);
            UpdateServer();
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

        protected void UpdateServer()
        {
            SessionController.Instance.NuSysNetworkSession.UpdateRegion(Model);
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
            LinkRemoved?.Invoke(this, linkController.Id);
        }*/

        #region Linking methods
        public void RequestAddNewLink(LinkId idToLinkTo)
        {
            SessionController.Instance.LinkController.RequestLink(new LinkId( SessionController.Instance.RegionsController.GetLibraryElementModelId(this.Model.Id),this.Model.Id), idToLinkTo);
        }

        public void RequestRemoveLink(LinkId linkLibraryElementID)
        {
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(linkLibraryElementID.LibraryElementId) as LinkLibraryElementController;
            SessionController.Instance.LinkController.RemoveLink(controller.Id.LibraryElementId);
        }
        public void ChangeLinkTitle(string linkLibraryElementID, string title)
        {
            SessionController.Instance.LinkController.ChangeLinkTitle(linkLibraryElementID, title);
        }

        public void ChangeLinkTags(string linkLibraryElementID, HashSet<String> tags)
        {
            SessionController.Instance.LinkController.ChangeLinkTags(linkLibraryElementID, tags);
        }
        
        public HashSet<LinkLibraryElementController> GetAllLinks()
        {
            throw new NotImplementedException();
            //return SessionController.Instance.LinkController.GetLinkLibraryElementControllers(this);
        }

        /// <summary>
        /// This mehtod should only be called from the server upon other updates.  It will pass in a region
        /// you should extract the region's properties and call the update methods in the controllers
        /// </summary>
        /// <param name="region"></param>
        public virtual void UnPack(Region region)
        {
            if (Model.Name != region.Name)
            {
                SetTitle(region.Name);
            }
        }
        #endregion

    }
}
