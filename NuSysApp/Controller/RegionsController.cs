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
        
        public RegionController GetRegionController(string id)
        {
            if (id == null)
            {
                return null;
            }
            return _regionControllers.ContainsKey(id) ? _regionControllers[id] : null;
        }

        public string Add(RegionController regionController)
        {
            if (regionController == null)
            {
                return null;
            }
            var regionModel = regionController.Model;
            if (!_regionControllers.ContainsKey(regionModel.Id))
            {
                _regionControllers.TryAdd(regionModel.Id, regionController);
                return regionModel.Id;
            }
            return null;
        }
    }
}
