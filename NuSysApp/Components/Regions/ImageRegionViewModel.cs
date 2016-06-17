using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{


    class ImageRegionViewModel
    {
        public ObservableCollection<ImageRegionView> Regions;

        public ImageRegionViewModel()
        {
            Regions = new ObservableCollection<ImageRegionView>();
        }

    }
}
