using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class GroupDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }

        public LibraryElementModel Model { get; }

        public bool Finite { get; }

        public bool Shape { get; }

        public GroupDetailHomeTabViewModel(LibraryElementController controller, HashSet<Region> regionsToLoad) : base(controller, regionsToLoad)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;

            var collectionmodel = Model as CollectionLibraryElementModel;
            Finite = collectionmodel.IsFinite;
            if (collectionmodel.ShapePoints != null)
            {
                Shape = true;
            }
            Shape = false;
        }

        public override void AddRegion(object sender, RegionController controller)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            //throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {
            //throw new NotImplementedException();
        }

        public override Region GetNewRegion()
        {
            throw new NotImplementedException();
        }
    }
}
