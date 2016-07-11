using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RegionsController 
    {


        public delegate void NewRegionEventHandler(RegionController regionController);
        /// <summary>
        /// Fired every time a region is added
        /// </summary>
        public event NewRegionEventHandler OnNewRegion;

        private ConcurrentDictionary<string, RegionController> _regionControllers = new ConcurrentDictionary<string, RegionController>();

    
        //returns the library element model id for a region id
        private ConcurrentDictionary<string, string> _regionLibraryElementModels = new ConcurrentDictionary<string, string>();

        private RegionControllerFactory _regionControllerFactory = new RegionControllerFactory();
        public ConcurrentDictionary<string, string> RegionIdsToLibraryElementIds
        {
            get { return _regionLibraryElementModels; }
        }

        public RegionController GetRegionController(string id)
        {
            if (id == null)
            {
                return null;
            }
            return _regionControllers.ContainsKey(id) ? _regionControllers[id] : null;
        }

        public string GetLibraryElementModelId(string id)
        {
            Debug.Assert(id != null && _regionLibraryElementModels.ContainsKey(id));
            return _regionLibraryElementModels[id];
        }
        /// <summary>
        /// Will return the given parameter if it is a content id
        /// will return the region's content id if it is a region
        /// null otherwise
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string GetContentIdOfRegionOrContent(string id)
        {
            Debug.Assert(id != null);
            if (SessionController.Instance.ContentController.GetContent(id) != null)
            {
                return id;
            }
            if (_regionLibraryElementModels.ContainsKey(id))
            {
                return _regionLibraryElementModels[id];
            }
            Debug.Fail("Should always be in one of the two");
            return null;
        }

        public bool IsRegionId(string id)
        {
            return id != null && _regionLibraryElementModels.ContainsKey(id);
        }


        public string Add(RegionController regionController, string contentId)
        {
            if (regionController == null)
            {
                return null;
            }
            var regionModel = regionController.Model;
            _regionLibraryElementModels.TryAdd(regionModel.Id, contentId);
            if (!_regionControllers.ContainsKey(regionModel.Id))
            {
                _regionControllers.TryAdd(regionModel.Id, regionController);
                OnNewRegion?.Invoke(regionController);

                return regionModel.Id;
            }
            else
            {
                //THIS IS THE CAUSE OF HALF OUR REGIONS PROBLEMS
                //throw new Exception("TRIED TO ADD A SECOND REGION CONTROLLER");
                //Debug.Fail("^^ stop commenting this out");
                return regionModel.Id;
            }
            return null;
        }

        public async Task Load()
        {
        
            _regionLibraryElementModels =  new ConcurrentDictionary<string, string>(await SessionController.Instance.NuSysNetworkSession.GetRegionMapping(
                    SessionController.Instance.ActiveFreeFormViewer.ContentId));
            Debug.Assert(_regionLibraryElementModels != null);

            var regionIds = _regionLibraryElementModels.Keys;
            foreach (var regionId in regionIds)
            {
                var libraryElementModel =
                    SessionController.Instance.ContentController.GetLibraryElementController(
                        _regionLibraryElementModels[regionId]).LibraryElementModel;
                var regionHashSet = libraryElementModel.Regions;
                foreach (var regionModel in regionHashSet)
                {
                    var regionController = _regionControllerFactory.CreateFromSendable(regionModel, libraryElementModel.LibraryElementId);
                    Add(regionController, libraryElementModel.LibraryElementId);
                }
            }

        }

    }
}
