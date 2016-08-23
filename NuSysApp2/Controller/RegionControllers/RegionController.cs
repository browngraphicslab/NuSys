using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp2
{
    public class RegionController
    {
        public Region Model;

        public delegate void SizeChangedEventHandler(object sender, Point topLeft, Point bottomRight);
        public delegate void RegionUpdatedEventHandler(object sender, double height, double width);

        public event SizeChangedEventHandler SizeChanged;
        public event RegionUpdatedEventHandler RegionChanged;
        public RegionController(Region model)
        {
            Model = model;

        }
    }
}
