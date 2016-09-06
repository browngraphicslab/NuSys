using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.VoiceCommands;
using NusysIntermediate;

namespace NuSysApp
{
    public class RegionsController 
    {


        // Keeps track of all the region library elements associated with a library element
        private ConcurrentDictionary<string, HashSet<string>> _clippingParentIdToRegionLibraryElementIds = new ConcurrentDictionary<string, HashSet<string>>();
        
        // Keeps track of all the region library elements associated with a content 
        private ConcurrentDictionary<string, HashSet<string>> _contentDataModelIdToRegionLibraryElementIds = new ConcurrentDictionary<string, HashSet<string>>();

        /// <summary>
        /// Takes in a library element model id
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <returns>a list of region library element ids which were clipped from the library element model passed in</returns>
        public HashSet<string> GetClippingParentRegionLibraryElementIds(string libraryElementId)
        {
            if (libraryElementId == null)
            {
                return null;
            }
            
            return _clippingParentIdToRegionLibraryElementIds.ContainsKey(libraryElementId) ? _clippingParentIdToRegionLibraryElementIds[libraryElementId] : new HashSet<string>();
        }

        /// <summary>
        /// Takes in a content data model id and returns a list of region library element ids for regions created from that content
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <returns></returns>
        public HashSet<string> GetContentDataModelRegionLibraryElementIds(string contentDataModelId)
        {
            if (contentDataModelId == null)
            {
                return null;
            }

            return _contentDataModelIdToRegionLibraryElementIds.ContainsKey(contentDataModelId) ? _contentDataModelIdToRegionLibraryElementIds[contentDataModelId] : new HashSet<string>();
        }


        /// <summary>
        /// to be called when we make the regon library element model.  Adds it to dictionaries
        /// </summary>
        /// <param name="regionModel"></param>
        public void AddRegion(LibraryElementModel regionModel)
        {
            Debug.Assert(regionModel != null);
            var clippingParentId = regionModel.ParentId;
            var contentId = regionModel.ContentDataModelId;
            if (clippingParentId == null)   {
                clippingParentId = "";
            }

            Debug.Assert(clippingParentId != null && contentId != null, "This should never be null");
            // create hashsets in the concurrent dictionaries if they don't exist
            if (!_clippingParentIdToRegionLibraryElementIds.ContainsKey(clippingParentId))
            {
                _clippingParentIdToRegionLibraryElementIds.TryAdd(clippingParentId, new HashSet<string>());
            }
            if (!_contentDataModelIdToRegionLibraryElementIds.ContainsKey(contentId))
            {
                _contentDataModelIdToRegionLibraryElementIds.TryAdd(contentId, new HashSet<string>());
            }

            // add the current region model library element id to the clipping parent dictionary and contentId dictionary
            _clippingParentIdToRegionLibraryElementIds[clippingParentId].Add(regionModel.LibraryElementId);
            _contentDataModelIdToRegionLibraryElementIds[contentId].Add(regionModel.LibraryElementId);

            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(contentId);
            if (contentDataModel == null)
            {
                return;
            }
            contentDataModel.AddRegion(regionModel.LibraryElementId);
        }

        public void RemoveRegion(LibraryElementModel regionModel)
        {
            Debug.Assert(regionModel != null);
            var clippingParentId = regionModel.ParentId;
            var contentId = regionModel.ContentDataModelId;
            Debug.Assert(clippingParentId != null || contentId != null, "This should never be null");
            /*
            if (_clippingParentIdToRegionLibraryElementIds.ContainsKey(clippingParentId) && _contentDataModelIdToRegionLibraryElementIds.ContainsKey(contentId)){
                return;
            }
            */
            string outParentIds;

            //Removed the region Id from both of the dictionaries
            _clippingParentIdToRegionLibraryElementIds[clippingParentId].Remove(regionModel.LibraryElementId);
            _contentDataModelIdToRegionLibraryElementIds[contentId].Remove(regionModel.LibraryElementId);

            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(contentId);
            contentDataModel.RemoveRegion(regionModel.LibraryElementId);
        }

    }
}
