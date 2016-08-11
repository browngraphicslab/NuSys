using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using LdaLibrary;
using NusysIntermediate;

namespace NuSysApp
{
    public class RegionLibraryElementController : LibraryElementController
    {
        public event EventHandler<string> TitleChanged;
        public delegate void SelectHandler(RegionLibraryElementController regionLibraryElementController);
        public event SelectHandler OnSelect;
        public delegate void DeselectHandler(RegionLibraryElementController regionLibraryElementController);
        public event DeselectHandler OnDeselect;
        public event EventHandler<LinkLibraryElementController> LinkAdded;
        public event EventHandler<string> LinkRemoved;
        public delegate void MetadataChangedEventHandler(object source);
        public event MetadataChangedEventHandler MetadataChanged;

        public Region RegionModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is Region);
                return LibraryElementModel as Region;
            }
        }


        private bool _selected;
        public RegionLibraryElementController(Region model): base(model)
        {

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

        /// <summary>
        /// This mehtod should only be called from the server upon other updates.  It will pass in a region
        /// you should extract the region's properties and call the update methods in the controllers
        /// </summary>
        /// <param name="region"></param>
        public override void UnPack(Message message)
        { 
            SetBlockServerBoolean(true);//this is a must otherwise infinite loops will occur
            if (message.ContainsKey("clipping_parent_library_id"))
            {
                RegionModel.ClippingParentId = message.GetString("clipping_parent_library_id");
            }
            base.UnPack(message);
            SetBlockServerBoolean(false);//THIS is a must otherwise changes wont be saved
        }
        
    }
}
