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
        private ConcurrentDictionary<string, RegionController> _regionControllers = new ConcurrentDictionary<string, RegionController>();
        private ConcurrentDictionary<string, string> _regionLibraryElementModels = new ConcurrentDictionary<string, string>();

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
            return _regionLibraryElementModels[id];
        }

        public string Add(RegionController regionController, string contentId)
        {
            if (regionController == null)
            {
                return null;
            }
            _regionLibraryElementModels[regionController.Model.Id] = contentId;
            var regionModel = regionController.Model;
            if (!_regionControllers.ContainsKey(regionModel.Id))
            {
                _regionControllers.TryAdd(regionModel.Id, regionController);
                return regionModel.Id;
            }
            else
            {
                //THIS IS THE CAUSE OF HALF OUR REGIONS PROBLEMS
                throw new Exception("TRIED TO ADD A SECOND REGION CONTROLLER");
            }
            return null;
        }
    }
}
