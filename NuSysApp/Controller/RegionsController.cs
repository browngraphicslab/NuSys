using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;

namespace NuSysApp
{
    public class RegionsController 
    {


        // Keeps track of all the region library elements associated with a library element
        private ConcurrentDictionary<string, HashSet<string>> _libraryElementIdToRegionLibraryElementIds = new ConcurrentDictionary<string, HashSet<string>>();
        
        public HashSet<string> GetRegionLibraryElementIds(string libraryElementId)
        {
            if (libraryElementId == null)
            {
                return null;
            }
            
            return _libraryElementIdToRegionLibraryElementIds.ContainsKey(libraryElementId) ? _libraryElementIdToRegionLibraryElementIds[libraryElementId] : null;
        }
        
        /// <summary>
        /// to be called when we make the regon library element model.  Adds it to dictionaries
        /// </summary>
        /// <param name="regionModel"></param>

        public void AddRegion(Region regionModel)
        {
            Debug.Assert(regionModel != null);
            var clippingParentId = regionModel.ClippingParentId;
            if (clippingParentId == null)
            {
                return;
            }
            if (!_libraryElementIdToRegionLibraryElementIds.ContainsKey(clippingParentId))
            {
                _libraryElementIdToRegionLibraryElementIds.TryAdd(clippingParentId, new HashSet<string>());
            }
            _libraryElementIdToRegionLibraryElementIds[clippingParentId].Add(regionModel.LibraryElementId);
            return;
        }

    }
}
