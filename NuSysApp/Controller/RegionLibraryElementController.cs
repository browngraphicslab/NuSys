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
        public Region RegionModel
        {
            get
            {
                Debug.Assert(LibraryElementModel is Region);
                return LibraryElementModel as Region;
            }
        }

        public RegionLibraryElementController(Region model): base(model)
        {

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
