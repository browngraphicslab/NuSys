using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RegionControllersController
    {
        private ConcurrentDictionary<string, RegionController> _regionControllersController = new ConcurrentDictionary<string, RegionController>();

        private RegionControllerFactory _regionControllerFactory = new RegionControllerFactory();
        public delegate void NewContentEventHandler(Region region);
        public event NewContentEventHandler OnNewContent;

        public delegate void ElementDeletedEventHandler(Region region);
        public event ElementDeletedEventHandler OnElementDelete;


        
        public int Count
        {
            get { return _regionControllersController.Count; }
        }


        public RegionControllersController()
        {

        }
        public RegionController GetRegionController(string id)
        {
            if (id == null)
            {
                return null;
            }
            return _regionControllersController.ContainsKey(id) ? _regionControllersController[id] : null;
        }

        public RegionController Add(Region regionModel)
        {
            if (!String.IsNullOrEmpty(regionModel.Id) && !_regionControllersController.ContainsKey(regionModel.Id))
            {
                var regionController = _regionControllerFactory.CreateFromSendable(regionModel);

                _regionControllersController.TryAdd(regionModel.Id, regionController);
                Debug.WriteLine("regioncontroller directly added with ID: " + regionModel.Id);
                OnNewContent?.Invoke(regionModel);
                return regionController;
            }
            Debug.WriteLine("content failed to add directly due to invalid id");
            return null;
        }

        public bool Remove(Region model)
        {
            if (!_regionControllersController.ContainsKey(model.Id))
            {
                return false;
            }
            RegionController removedController;
            _regionControllersController.TryRemove(model.Id, out removedController);
            OnElementDelete?.Invoke(model);
            return true;
        }
        public string OverWrite(Region model)
        {
            if (!String.IsNullOrEmpty(model.Id))
            {
                _regionControllersController[model.Id] = new RegionController(model);
                return model.Id;
            }
            return null;
        }
    }
}
