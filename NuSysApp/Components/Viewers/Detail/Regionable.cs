using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace NuSysApp
{
    public abstract class Regionable <T> :BaseINPC
    {
        public UserControl View;

        public abstract void AddRegion(object sender, RegionController regionController);

        public abstract void RemoveRegion(object sender, T displayedRegion);
        public abstract void SizeChanged(object sender, double width, double height);

        public abstract void SetExistingRegions();

        public abstract Message GetNewRegionMessage();

    }
}
